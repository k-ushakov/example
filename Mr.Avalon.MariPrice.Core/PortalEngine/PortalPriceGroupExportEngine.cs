using Mr.Avalon.File.Client;
using Mr.Avalon.MariPrice.Client;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public class PortalPriceGroupExportEngine
	{
		private ISqlFactory m_sql;
		private FileApiClient m_files;
		public PortalPriceGroupExportEngine(
			ISqlFactory sql,
			FileApiClient files
			)
		{
			m_sql = sql;
			m_files = files;
		}

		public MariPriceApi.PortalPrice.Group.Export.Response Exec(MariPriceApi.PortalPrice.Group.Export request)
		{
			var result = new MariPriceApi.PortalPrice.Group.Export.Response();

			// детали групп кластера
			var groups = GetGroups(request);

			// наполним excell файл
			byte[] data = GetExcellData(groups);

			// сохраним excell файл в azure
			result.Url = UploadExcellToSessionStorage(data);

			return result;
		}

		private byte[] GetExcellData(List<PriceGroup> groups)
		{
			var memoryStream = new MemoryStream();

			IWorkbook workbook = new XSSFWorkbook();
			ISheet excelSheet = workbook.CreateSheet("Группы ценового кластера");

			IRow row = excelSheet.CreateRow(0);
			IRow row2 = excelSheet.CreateRow(1);
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
			titleStyle.VerticalAlignment = VerticalAlignment.Center;
			titleStyle.Alignment = HorizontalAlignment.Center;

			// Название ценовой группы
			cell = row.CreateCell(0);
			cell.CellStyle = titleStyle;
			cell.SetCellValue("  Название ценовой группы  ");
			excelSheet.AddMergedRegion(new CellRangeAddress ( firstRow: 0, lastRow: 1, firstCol:0, lastCol: 0 ));

			// "% потерь"
			cell = row.CreateCell(1);
			cell.CellStyle = titleStyle;
			cell.SetCellValue("  % потерь  ");
			excelSheet.AddMergedRegion(new CellRangeAddress(firstRow: 0, lastRow: 1, firstCol: 1, lastCol: 1));

			var valueListTitle = groups.First().PriceGroupValueList;
			int columnIndex = 2;
			foreach (var title in valueListTitle)
			{
				// "От {0} г. (срок {1} дн.)"
				cell = row.CreateCell(columnIndex);
				
				cell.CellStyle = titleStyle;
				cell.SetCellValue(string.Format("    От {0:F0} г (срок {1} дн)    ", title.OrderMetalWeight, title.ProductionTime));
				excelSheet.AddMergedRegion(new CellRangeAddress(firstRow: 0, lastRow: 0, firstCol: columnIndex, lastCol: columnIndex+1));

				// с НДС
				cell = row2.CreateCell(columnIndex);
				cell.CellStyle = titleStyle;
				cell.SetCellValue(string.Format("  Базовая стоимость с НДС  ", title.OrderMetalWeight, title.ProductionTime));

				// без НДС
				cell = row2.CreateCell(columnIndex+1);
				cell.CellStyle = titleStyle;
				cell.SetCellValue(string.Format("  Базовая стоимость без НДС  ", title.OrderMetalWeight, title.ProductionTime));

				columnIndex += 2;
			}

			// данные

			int rowIndex = 2;
			var style = workbook.CreateCellStyle();
			style.BorderTop = BorderStyle.Thin;
			style.BorderBottom = BorderStyle.Thin;
			style.BorderLeft = BorderStyle.Thin;
			style.BorderRight = BorderStyle.Thin;

			foreach(var group in groups)
			{
				row = excelSheet.CreateRow(rowIndex);

				// Название ценовой группы
				cell = row.CreateCell(0);
				cell.CellStyle = style;
				cell.SetCellValue($"{group.Name}");

				// % потерь
				cell = row.CreateCell(1);
				cell.CellStyle = style;
				cell.SetCellValue($"{group.LossPercentage:F2}");

				columnIndex = 2;
				foreach (var value in group.PriceGroupValueList)
				{
					// с НДС
					cell = row.CreateCell(columnIndex);
					cell.CellStyle = style;
					cell.SetCellValue((value.WithNdsPrice.HasValue ?  $"{value.WithNdsPrice:F2}" : ""));

					// без НДС
					cell = row.CreateCell(columnIndex+1);
					cell.CellStyle = style;
					cell.SetCellValue((value.WithoutNdsPrice.HasValue ? $"{value.WithoutNdsPrice:F2}" : ""));

					columnIndex += 2;
				}

				rowIndex++;
			}

			excelSheet.AutoSizeColumn(0, true);
			excelSheet.AutoSizeColumn(1, true);

			for (int i = 0; i < valueListTitle.Count; i++)
			{
				excelSheet.AutoSizeColumn(2 + i * 2, true);
				excelSheet.AutoSizeColumn(3 + i * 2, true);
			}

			workbook.Write(memoryStream);

			return memoryStream.ToArray();
		}

		private string UploadExcellToSessionStorage(byte[] data)
		{
			var sessionId = Guid.NewGuid();
			var expiredTime = DateTime.UtcNow.AddHours(1);

			return new FileApi.Session.Upload
			{
				FileId = $"PriceGroup-{sessionId}",
				FileName = "PriceGroup.xlsx",
				Data = data,
				SessionId = sessionId,
				SessionExpireTime = expiredTime
			}.Exec(m_files);
		}


		private List<PriceGroup> GetGroups(MariPriceApi.PortalPrice.Group.Export request)
		{
			var result = new List<PriceGroup>();

			// список групп кластера
			var groupList = new MariPriceDb.Price.Group.List
			{
				ClusterIds = new List<int>{ request.PriceClusterId }
			}.Exec(m_sql);

			// список вариантов
			var settingList = new MariPriceDb.Price.ClusterSetting.List
			{
				PriceClusterIds = new List<int> { request.PriceClusterId }
			}.Exec(m_sql);

			// значения вариантов
			var valueList = new MariPriceDb.Price.GroupValue.List
			{
				GroupIds = groupList.Select(x => x.Id).ToList()
			}.Exec(m_sql);

			foreach(var group in groupList)
			{
				var priceGroup = new PriceGroup
				{
					Name = !string.IsNullOrWhiteSpace(group.Name) ? group.Name : group.DisplayName,
					LossPercentage = group.LossPercentage
				};

				var priceGroupValueList = new List<PriceGroupValue>();

				foreach(var setting in settingList)
				{
					var value = valueList.FirstOrDefault(x => x.PriceGroupId == group.Id && x.PriceClusterVariantId == setting.Id);

					var PriceGroupValue = new PriceGroupValue
					{
						OrderMetalWeight = setting.OrderMetalWeight,
						ProductionTime = setting.ProductionTime,
						WithNdsPrice = value?.WithNdsPrice,
						WithoutNdsPrice = value?.WithoutNdsPrice
					};

					priceGroupValueList.Add(PriceGroupValue);
				}

				priceGroup.PriceGroupValueList = priceGroupValueList;
				result.Add(priceGroup);
			}

			return result;
		}

		class PriceGroupValue
		{
			public decimal OrderMetalWeight { get; set; }
			public int ProductionTime { get; set; }
			public decimal? WithNdsPrice { get; set; }
			public decimal? WithoutNdsPrice { get; set; }
		}

		class PriceGroup
		{
			public string Name { get; set; }

			public decimal LossPercentage { get; set; }

			public List<PriceGroupValue> PriceGroupValueList { get; set; }

			public PriceGroup()
			{
				PriceGroupValueList = new List<PriceGroupValue>();
			}
		}
	}
}
