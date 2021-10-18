using Mr.Avalon.File.Client;
using Mr.Avalon.MariPrice.Client;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public class PortalPriceProductExportEngine
	{
		private ISqlFactory m_sql;
		private FileApiClient m_files;

		public PortalPriceProductExportEngine(
			ISqlFactory sql,
			FileApiClient files
			)
		{
			m_sql = sql;
			m_files = files;
		}

		public MariPriceApi.PortalPrice.Product.Export.Response Exec(MariPriceApi.PortalPrice.Product.Export request)
		{
			var result = new MariPriceApi.PortalPrice.Product.Export.Response();

			// детали продуктов кластера
			var products = GetProducts(request);

			// наполним excell файл
			byte[] data = GetExcellData(products);

			// сохраним excell файл в azure
			result.Url = UploadExcellToSessionStorage(data);

			return result;
		}

		private byte[] GetExcellData(PriceProduct[] products)
		{
			var memoryStream = new MemoryStream();

			IWorkbook workbook = new XSSFWorkbook();
			ISheet excelSheet = workbook.CreateSheet("Изделия ценового кластера");

			IRow row = excelSheet.CreateRow(0);
			ICell cell;
			int columnIndex = 0;

			// заголовок
			var titles = new[] { "Название ценовой группы", "Артикул изделия", "Артикул размера", "Размер" };
			var titleFont = workbook.CreateFont();
			titleFont.IsBold = true;

			var titleStyle = workbook.CreateCellStyle();
			titleStyle.SetFont(titleFont);
			titleStyle.BorderTop = BorderStyle.Thin;
			titleStyle.BorderBottom = BorderStyle.Thin;
			titleStyle.BorderLeft = BorderStyle.Thin;
			titleStyle.BorderRight = BorderStyle.Thin;

			foreach (var title in titles)
			{
				cell = row.CreateCell(columnIndex);
				cell.CellStyle = titleStyle;
				cell.SetCellValue(title);

				columnIndex++;
			}

			// данные
			int rowIndex = 1;
			var style = workbook.CreateCellStyle();
			style.BorderTop = BorderStyle.Thin;
			style.BorderBottom = BorderStyle.Thin;
			style.BorderLeft = BorderStyle.Thin;
			style.BorderRight = BorderStyle.Thin;

			products = products ?? new PriceProduct[0];
			foreach (var product in products)
			{
				row = excelSheet.CreateRow(rowIndex);

				// Название ценовой группы
				cell = row.CreateCell(0);
				cell.CellStyle = style;
				cell.SetCellValue($"{product.GroupName}");

				// Артикул изделия
				cell = row.CreateCell(1);
				cell.CellStyle = style;
				cell.SetCellValue($"{product.ProductPn}");

				// Артикул размера
				cell = row.CreateCell(2);
				cell.CellStyle = style;
				cell.SetCellValue($"{product.SizePn}");

				// Размер
				cell = row.CreateCell(3);
				cell.CellStyle = style;
				cell.SetCellValue($"{product.Size}");

				rowIndex++;
			}

			excelSheet.AutoSizeColumn(0);
			excelSheet.AutoSizeColumn(1);
			excelSheet.AutoSizeColumn(2);
			excelSheet.AutoSizeColumn(3);

			workbook.Write(memoryStream);

			return memoryStream.ToArray();
		}

		private PriceProduct[] GetProducts(MariPriceApi.PortalPrice.Product.Export request)
		{
			// определим версию
			var versionId = GetVersionId(request);

			// список групп кластера
			var groups = GetClusterGroups(request.PriceClusterId);
			if (groups.Count == 0)
				return null;

			// идентификаторы продуктов в группах
			var productUIds = GetProductUIds(request, versionId, groups);
			if (productUIds.Length == 0)
				return null;

			// описания продуктов
			return GetProductDetails(productUIds, versionId, groups);
		}

		private Dictionary<int, string> GetClusterGroups(int priceClusterId)
		{
			var groupList = new MariPriceDb.Price.Group.List().ForClusters(priceClusterId).Exec(m_sql);
			return groupList.ToDictionary(k => k.Id, k => k.DisplayName);
		}

		private PriceProduct[] GetProductDetails(Guid[] productUIds, int versionId, Dictionary<int, string> groups)
		{
			var productDbList = new MariPriceDb.Price.Product.SearchProduct
			{
				VersionId = versionId,
				ProductUids = productUIds
			}.Exec(m_sql);

			var woSizeProductList = productDbList.Where(x => x.SizeUid == Guid.Empty)
				.Select(x => new PriceProduct
				{
					ProductUid = x.ProductUid,
					GroupId = x.PriceGroupId ?? 0,
					ProductPn = x.Pn
				})
				.ToList();

			var products = new List<PriceProduct>();

			foreach (var dbProduct in productDbList)
			{
				// продукт с размерами в группе
				if (dbProduct.SizeUid != Guid.Empty && dbProduct.PriceGroupId.HasValue)
				{
					products.Add(new PriceProduct
					{
						ProductUid = dbProduct.ProductUid,
						GroupId = dbProduct.PriceGroupId.Value,
						GroupName = groups.ContainsKey(dbProduct.PriceGroupId.Value) ? groups[dbProduct.PriceGroupId.Value] : null,
						ProductPn = woSizeProductList.FirstOrDefault(x => x.ProductUid == dbProduct.ProductUid)?.ProductPn,
						SizePn = dbProduct.Pn,
						Size = GetFormattedSize(dbProduct)
					});
				}

				// один продукт без размеров в группе
				else if (dbProduct.SizeUid == Guid.Empty && dbProduct.PriceGroupId.HasValue && productDbList.Where(x => x.ProductUid == dbProduct.ProductUid).Count() == 1)
				{
					products.Add(new PriceProduct
					{
						ProductUid = dbProduct.ProductUid,
						GroupId = dbProduct.PriceGroupId.Value,
						GroupName = groups.ContainsKey(dbProduct.PriceGroupId.Value) ? groups[dbProduct.PriceGroupId.Value] : null,
						ProductPn = dbProduct.Pn
					});
				}

				// продукт с размерами не в группе - проверим "остальные размеры"
				else if (dbProduct.SizeUid != Guid.Empty && !dbProduct.PriceGroupId.HasValue)
				{
					var woSizeProduct = woSizeProductList.FirstOrDefault(x => x.ProductUid == dbProduct.ProductUid && x.GroupId > 0);
					if (woSizeProduct != null)
					{
						products.Add(new PriceProduct
						{
							ProductUid = dbProduct.ProductUid,
							GroupId = woSizeProduct.GroupId,
							GroupName = groups.ContainsKey(woSizeProduct.GroupId) ? groups[woSizeProduct.GroupId] : null,
							ProductPn = woSizeProduct.ProductPn,
							SizePn = dbProduct.Pn,
							Size = GetFormattedSize(dbProduct)
						});
					}
				}
			}

			return products.ToArray();
		}

		private string GetFormattedSize(MariPriceDb.Price.Product.SearchProduct.Item dbProduct)
		{
			if (!string.IsNullOrWhiteSpace(dbProduct.Size) &&
				dbProduct.WireThickness.HasValue)
				return $"Ø{dbProduct.WireThickness.Value} - р. {dbProduct.Size}";
			else if (!string.IsNullOrWhiteSpace(dbProduct.Size))
				return $"{dbProduct.Size}";
			else
				return null;
		}

		private Guid[] GetProductUIds(MariPriceApi.PortalPrice.Product.Export request, int versionId, Dictionary<int, string> groups)
		{
			var productList =  new MariPriceDb.Price.Product.Search
			{
				VersionId = versionId,
				CompanyId = request.CompanyId,
				PriceGroupIds = groups.Keys.ToList()
			}.Exec(m_sql);

			return productList.Select(x => x.ProductUid).Distinct().ToArray();
		}

		private int GetVersionId(MariPriceApi.PortalPrice.Product.Export request)
		{
			var companyVersion = new MariPriceDb.Price.Company.List
			{
				CompanyIds = new[] { request.CompanyId }
			}.Exec(m_sql)
			.Single();

			var versionIds = new List<int> { companyVersion.ActiveVersionId, companyVersion.DraftVersionId  };

			var companyCluster = new MariPriceDb.Price.Cluster.List
			{
				ClusterIds = new List<int> { request.PriceClusterId },
				VersionIds = versionIds
			}.Exec(m_sql)
			.Single();

			return companyCluster.VersionId;
		}

		private string UploadExcellToSessionStorage(byte[] data)
		{
			var sessionId = Guid.NewGuid();
			var expiredTime = DateTime.UtcNow.AddHours(1);

			return new FileApi.Session.Upload
			{
				FileId = $"PriceProduct-{sessionId}",
				FileName = "PriceProduct.xlsx",
				Data = data,
				SessionId = sessionId,
				SessionExpireTime = expiredTime
			}.Exec(m_files);
		}

		class PriceProduct
		{
			public Guid ProductUid { get; set; }
			public int GroupId { get; set; }			
			public string GroupName { get; set; }
			public string ProductPn { get; set; }
			public string SizePn { get; set; }
			public string Size { get; set; }
		}

		class ProductComparer : IEqualityComparer<MariPriceDb.Price.Product>
		{
			public bool Equals(MariPriceDb.Price.Product x, MariPriceDb.Price.Product y)
			{
				return x.ProductUid.Equals(y.ProductUid);
			}

			public int GetHashCode(MariPriceDb.Price.Product obj)
			{
				return obj.ProductUid.GetHashCode();
			}
		}

	}
}
