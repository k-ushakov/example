using Mr.Avalon.File.Client;
using Mr.Avalon.MariPrice.Client;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public class PortalPriceProductImportEngine
	{
		private ISqlFactory m_sql;
		private PriceEngine m_priceEngine;
		private PortalPriceGroupEngine m_portalPriceGroupEngine;
		private FileApiClient m_files;

		public PortalPriceProductImportEngine(
			ISqlFactory sql,
			PriceEngine priceEngine,
			PortalPriceGroupEngine portalPriceGroupEngine,
			FileApiClient files
			)
		{
			m_sql = sql;
			m_priceEngine = priceEngine;
			m_portalPriceGroupEngine = portalPriceGroupEngine;
			m_files = files;
		}

		public MariPriceApi.PortalPrice.Product.Import.Response Exec(MariPriceApi.PortalPrice.Product.Import request)
		{
			var groups = GetClusterGroups(request.PriceClusterId);
			if (groups.Count == 0)
				throw new System.Exception($"Price cluster {request.PriceClusterId} has no any groups");

			var workbook = GetWorkbook(request.Url);

			var products = Parse(workbook);

			EraseGroups(request.PriceClusterId, groups, products);

			foreach (var product in products)
			{
				try
				{
					if (string.IsNullOrWhiteSpace(product.GroupName))
						throw new System.Exception("Group is empty");

					KeyValuePair<int, string> pair = groups.FirstOrDefault(x => x.Value.Equals(product.GroupName, StringComparison.OrdinalIgnoreCase));
					if (pair.Key == 0)
						throw new System.Exception($"Price cluster {request.PriceClusterId} has no the group '{product.GroupName}'");

					var productId = GetProductId(product, request.CompanyId);

					m_portalPriceGroupEngine.CreateProductLink1(new MariPriceApi.PortalPrice.Product.Create { Id = productId, PriceGroupId = pair.Key, RemoveOldLink = true });
				}
				catch (System.Exception ex)
				{
					product.AppendError($"{ex.Message}");
				}
			}

			var resultUrl = WriteImportResult(products, workbook);

			return new MariPriceApi.PortalPrice.Product.Import.Response
			{
				Url = resultUrl,
				HasErrors = products.Any(x => !string.IsNullOrEmpty(x.Error))
			};
		}

		private void EraseGroups(int clusterId, Dictionary<int, string> groups, List<Product> products)
		{
			if (groups == null || !groups.Any())
				return;

			var groupNames = (products?.Where(x => !string.IsNullOrWhiteSpace(x.GroupName))?.Select(x => x.GroupName) ?? new string[0]).Distinct().ToList();
			foreach (var groupName in groupNames)
			{
				if (string.IsNullOrWhiteSpace(groupName))
					continue;

				// определим группу продукта
				KeyValuePair<int, string> pair = groups.FirstOrDefault(x => x.Value.Equals(groupName, StringComparison.OrdinalIgnoreCase));
				if (pair.Key == 0)
					continue;

				m_priceEngine.RemoveFromProductLinkAndCluster(clusterId, pair.Key);
			}
		}

		private string WriteImportResult(List<Product> products, XSSFWorkbook workbook)
		{
			ISheet sheet = workbook.GetSheetAt(0);
			IRow row;
			ICell cell;

			// заголовок			
			var titleFont = workbook.CreateFont();
			titleFont.IsBold = true;

			var titleStyle = workbook.CreateCellStyle();
			titleStyle.SetFont(titleFont);
			titleStyle.BorderTop = BorderStyle.Thin;
			titleStyle.BorderBottom = BorderStyle.Thin;
			titleStyle.BorderLeft = BorderStyle.Thin;
			titleStyle.BorderRight = BorderStyle.Thin;

			row = sheet.GetRow(0);
			int titleLastCellNum = row.LastCellNum;
			cell = row.CreateCell(titleLastCellNum);
			cell.CellStyle = titleStyle;
			cell.SetCellValue($"Результат импорта");

			// данные
			int startRow = 1;
			int productIndex = 0;
			int lastRow = sheet.LastRowNum;

			for (int i = startRow; i <= lastRow; i++)
			{
				row = sheet.GetRow(i);

				if (row == null) continue;

				if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

				while (row.LastCellNum < titleLastCellNum)
				{
					cell = row.CreateCell(row.LastCellNum);
					cell.SetCellValue("");
				}

				cell = row.CreateCell(row.LastCellNum);
				var error = products.ElementAt(productIndex).Error;
				cell.SetCellValue(string.IsNullOrEmpty(error) ? "OK" : error);

				productIndex++;
			}

			sheet.AutoSizeColumn(row.LastCellNum-1);

			var ыtream = new MemoryStream();
			workbook.Write(ыtream);

			var sessionId = Guid.NewGuid();
			var expiredTime = DateTime.UtcNow.AddHours(1);

			return new FileApi.Session.Upload
			{
				FileId = $"PriceProduct-{sessionId}",
				FileName = "PriceProductImportResult.xlsx",
				Data = ыtream.ToArray(),
				SessionId = sessionId,
				SessionExpireTime = expiredTime
			}.Exec(m_files);
		}

		private int GetProductId(Product row, int companyId)
		{
			MariPriceDb.Price.Product.List listRequest = null;

			// Артикул изделия
			if (string.IsNullOrEmpty(row.SizePn) &&
				string.IsNullOrEmpty(row.Size) &&
				!row.WireThickness.HasValue)
			{
				listRequest = new MariPriceDb.Price.Product.List
				{
					SizeUids = new List<Guid> { Guid.Empty },
					Pns = new List<string> { row.ProductPn },
					CompanyIds = new List <int> { companyId },
					OnlyEnabled = true
				};
			}

			// Артикул размера
			else if (!string.IsNullOrWhiteSpace(row.SizePn))
			{
				listRequest = new MariPriceDb.Price.Product.List
				{
					Pns = new List<string> { row.SizePn },
					CompanyIds = new List<int> { companyId },
					OnlyEnabled = true
				};
			}

			// Size и Диаметр
			else if (!string.IsNullOrWhiteSpace(row.Size) && row.WireThickness.HasValue)
			{

				listRequest = new MariPriceDb.Price.Product.List
				{
					ProductUids = GetProductUids(row, companyId),
					Sizes = GetSizes(row.Size),
					WireThickness = new List<decimal> {  row.WireThickness.Value },
					CompanyIds = new List<int> { companyId },
					OnlyEnabled = true
				};
			}

			// только Size
			else if (!string.IsNullOrWhiteSpace(row.Size) && !row.WireThickness.HasValue)
			{
				listRequest = new MariPriceDb.Price.Product.List
				{
					ProductUids = GetProductUids(row, companyId),
					Sizes = GetSizes(row.Size),
					WireThicknessIsNull = true,
					CompanyIds = new List<int> { companyId },
					OnlyEnabled = true
				};
			}

			if (listRequest == null)
				throw new System.Exception("Invalid request");

			var product = listRequest.Exec(m_sql).FirstOrDefault();

			if (product == null)
				throw new System.Exception("Product not found");

			return product.Id;
		}

		private List<string> GetSizes(string size)
		{
			var sizes = new List<string>();

			var parts = size.Trim().Split(".");

			// 15
			if (parts.Length == 1)
			{
				sizes.Add($"{size}");		// 15
				sizes.Add($"{parts[0]}.0");	// 15.0
			}
			
			// 15.0
			else if (parts.Length == 2)
			{
				if (parts[1] == "0")
					sizes.Add($"{parts[0]}");// 15
				sizes.Add($"{size}");		 // 15.0
			}

			else
			{
				sizes.Add($"{size}");
			}

			return sizes;
		}

		private List<Guid> GetProductUids(Product row, int companyId)
		{
			var productUids = new MariPriceDb.Price.Product.List
			{
				Pns = new List<string> { row.ProductPn },
				CompanyIds = new List<int> { companyId },
				OnlyEnabled = true
			}
			.Exec(m_sql)
			.Select(x => x.ProductUid)
			.Distinct()
			.ToList();

			if (!productUids.Any())
				throw new System.Exception("Product not found");

			return productUids;
		}

		private Dictionary<int, string> GetClusterGroups(int priceClusterId)
		{
			var groupList = new MariPriceDb.Price.Group.List().ForClusters(priceClusterId).Exec(m_sql);
			return groupList.ToDictionary(k => k.Id, k => k.DisplayName);
		}

		private List<Product> Parse(XSSFWorkbook workbook)
		{
			var result = new List<Product>();
			ISheet sheet = workbook.GetSheetAt(0);

			// строки со значениями

			int startRow = 1;
			int lastRow = sheet.LastRowNum;
			
			for (int i = startRow; i <= lastRow; i++)
			{
				IRow row = sheet.GetRow(i);

				if (row == null) continue;

				if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

				var product = new Product();

				// Название ценовой группы	
				product.GroupName = $"{row.GetCell(0)}".Trim();

				// Артикул изделия	
				product.ProductPn = $"{row.GetCell(1)}".Trim();

				// Артикул размера	
				product.SizePn = $"{row.GetCell(2)}".Trim();

				// Размер
				ParseSize(product, $"{row.GetCell(3)}".Trim());

				result.Add(product);
			}
			
			return result;

				//return new List<Product>
				//{
				//	new Product { GroupName = "Ценовая группа 1", ProductPn = "Артикул20210824-01" },
				//	new Product { GroupName = "Ценовая группа 1", ProductPn = "Артикул20210824-01", SizePn = "Артикул20210824-01.1" },
				//	new Product { GroupName = "Ценовая группа 2", ProductPn = "Артикул20210824-01", Size = "17.0" },
				//};
			}

		private void ParseSize(Product product, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			decimal size, wireThickness;

			var parts = value.Split(" ");

			// “диаметр - размер“ формат: “Ø<диаметр проволоки> - р. <размер>“, например: Ø3 - р. 45
			if (parts.Length == 4)
			{
				if (decimal.TryParse(parts[0].Replace("Ø", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out wireThickness))
				{
					product.WireThickness = wireThickness;
				}
				else
				{
					product.AppendError($"Could not parse WireThickness '{value}'");
				}

				if (decimal.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out size))
				{
					product.Size = size.ToString("F1", CultureInfo.InvariantCulture);
				}
				else
				{
					product.AppendError($"Could not parse Size '{value}'");
				}

				return;
			}

			// “размер“ формат: <размер>, например: 17.5
			if(parts.Length == 1)
			{
				if (decimal.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out size))
				{
					product.Size = size.ToString("F1", CultureInfo.InvariantCulture);
				}
				else
				{
					product.AppendError($"Could not parse Size '{value}'");
				}

				return;
			}

			product.AppendError($"Invalid size format '{value}'");
		}

		private XSSFWorkbook GetWorkbook(string url)
		{
			var client = new WebClient();
			var stream = new MemoryStream(client.DownloadData(url));

			var contentType = client.ResponseHeaders["Content-Type"];

			// проверить контент-тип файла
			if (contentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
				throw new System.Exception($"Could not parse file of content type '{contentType}'");

			return new XSSFWorkbook(stream);
		}

		class Product
		{
			private List<string> _errors = new List<string>();

			public string GroupName { get; set; }

			public int ProductId { get; set; }

			public string ProductPn { get; set; }
			public string SizePn { get; set; }

			public string Size { get; set; }
			public decimal? WireThickness { get; set; }

			
			public void AppendError(string error)
			{
				_errors.Add(error);
			}

			public string Error { get { return string.Join(".", _errors); } }
		}
	}
}
