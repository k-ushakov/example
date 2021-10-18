using Mr.Avalon.Common;
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
	public class PortalPriceGroupImportEngine
	{
		private ISqlFactory m_sql;
		private FileApiClient m_files;
		private PriceEngine m_priceEngine;

		public PortalPriceGroupImportEngine(
			ISqlFactory sql,
			FileApiClient files,
			PriceEngine priceEngine
			)
		{
			m_sql = sql;
			m_files = files;
			m_priceEngine = priceEngine;
		}

		public MariPriceApi.PortalPrice.Group.Import.Response Exec(MariPriceApi.PortalPrice.Group.Import request)
		{
			var cluster = m_priceEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(request.PriceClusterId), true)?.FirstOrDefault();
			if (cluster == null)
				throw new RecordNotFoundApiException($"Cluster {request.PriceClusterId} cannot be found ");

			// список групп кластера
			var groups = GetClusterGroups(request.PriceClusterId);

			// очистим кластер от групп и товаров в них
			EraseGroups(request.PriceClusterId, groups);

			var workbook = GetWorkbook(request.Url);

			var priceGroups = Parse(workbook);

			if (priceGroups.Any() && !cluster.InOrder)
			{
				var updateCluster = new MariPriceApi.Price.Cluster.Update() { Id = request.PriceClusterId };
				updateCluster.InOrder = true;
				updateCluster.UpdationList = new string[] { nameof(updateCluster.InOrder) };
				m_priceEngine.UpdateCluster(updateCluster);
			}

			// получить схему продажи
			var companySaleInfo = GetSaleSchema(request.CompanyId);

			// создание групп
			foreach (var priceGroup in priceGroups)
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(priceGroup.Error))
						continue;

					// создать новую группу
					priceGroup.PriceGroupId = m_priceEngine.CreateGroup(
						new MariPriceApi.Price.Group.Create
						{
							ClusterId = request.PriceClusterId,
							Name = priceGroup.Name,
							DisplayName = priceGroup.Name,
							LossPercentage = priceGroup.LossPercentage
						});

					// создать варианты исполнения
					foreach (var groupValue in priceGroup.PriceGroupValueList)
					{
						if (groupValue.IsDuplicate)
							continue;

						// получить или создать варианты исполнения
						if (groupValue.ClusterSettingId == 0)
						{
							groupValue.ClusterSettingId = m_priceEngine.CreateSetting(
								new MariPriceApi.Price.ClusterSetting.Create
								{
									ClusterId = request.PriceClusterId,
									OrderMetalWeight = groupValue.OrderMetalWeight,
									ProductionTime = groupValue.ProductionTime
								});

							// Установить всем значениям
							SetClusterSettingId(priceGroups, groupValue);
						}

						// создать или обновить значения варианта исполнения
						m_priceEngine.CreateGroupValue(new MariPriceApi.Price.GroupValue.Create
						{
							PriceGroupId = priceGroup.PriceGroupId,
							PriceClusterVariantId = groupValue.ClusterSettingId,

							// С НДС ( учесть схему продажи )
							WithNdsPrice = companySaleInfo.WithNdsPriceRequired && groupValue.WithNdsPrice.HasValue && groupValue.WithNdsPrice > 0
								? groupValue.WithNdsPrice
								: (decimal?)null,
							WithNdsMarkup = (decimal?)null,

							// БЕЗ НДС ( учесть схему продажи )
							WithoutNdsPrice = companySaleInfo.WithoutNdsPriceRequired && groupValue.WithoutNdsPrice.HasValue && groupValue.WithoutNdsPrice > 0
								? groupValue.WithoutNdsPrice
								: (decimal?)null,
							WithoutNdsMarkup = (decimal?)null
						});
					}
				}
				catch (System.Exception ex)
				{
					priceGroup.AppendError($"{ex.Message}");
				}
			}

			// сохранить ошибки в excell-файл
			var resultUrl = WriteImportResult(priceGroups, workbook);

			return new MariPriceApi.PortalPrice.Group.Import.Response
			{
				Url = resultUrl,
				HasErrors = priceGroups.Any(x => !string.IsNullOrEmpty(x.Error))
			};

		}

		private Dictionary<int, string> GetClusterGroups(int priceClusterId)
		{
			var groupList = new MariPriceDb.Price.Group.List().ForClusters(priceClusterId).Exec(m_sql);
			return groupList.ToDictionary(k => k.Id, k => k.DisplayName);
		}

		private void EraseGroups(int clusterId, Dictionary<int, string> groups)
		{
			foreach (var groupId in groups.Keys)
			{
				m_priceEngine.RemoveFromProductLinkAndCluster(clusterId, groupId);
				m_priceEngine.DeleteGroup(groupId);
			}

			var settings = m_priceEngine.GetSetting(new MariPriceApi.Price.ClusterSetting.List { PriceClusterIds = new List<int> { clusterId } });
			foreach (var setting in settings)
				m_priceEngine.DeleteSetting(new MariPriceApi.Price.ClusterSetting.Delete { ClusterId = clusterId, SettingsVariantId = setting.Id });
		}

		private string WriteImportResult(List<PriceGroup> groups, XSSFWorkbook workbook)
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
			cell = row.CreateCell(row.LastCellNum);
			cell.CellStyle = titleStyle;
			cell.SetCellValue($"Результат импорта");

			// есть/нет дубль среди вариантов исполнения
			if (groups.SelectMany(x => x.PriceGroupValueList).Any(x => x.IsDuplicate))
			{
				row = sheet.GetRow(1);
				cell = row.CreateCell(row.LastCellNum);
				cell.SetCellValue("При загрузке выявлены дубликаты, загружены только уникальные варианты исполнения");
			}

			// данные
			int startRow = 2;
			int index = 0;
			int lastRow = sheet.LastRowNum;

			for (int i = startRow; i <= lastRow; i++)
			{
				row = sheet.GetRow(i);

				if (row == null) continue;

				if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

				cell = row.CreateCell(row.LastCellNum);
				var error = groups.ElementAt(index).Error;
				cell.SetCellValue(string.IsNullOrEmpty(error) ? "OK" : error);

				index++;
			}

			sheet.AutoSizeColumn(row.LastCellNum - 1);

			var ыtream = new MemoryStream();
			workbook.Write(ыtream);

			var sessionId = Guid.NewGuid();
			var expiredTime = DateTime.UtcNow.AddHours(1);

			return new FileApi.Session.Upload
			{
				FileId = $"PriceGroup-{sessionId}",
				FileName = "PriceGroupImportResult.xlsx",
				Data = ыtream.ToArray(),
				SessionId = sessionId,
				SessionExpireTime = expiredTime
			}.Exec(m_files);
		}

		private void SetClusterSettingId(List<PriceGroup> rows, PriceGroupValue groupValue)
		{
			var groups = rows.SelectMany(i => i.PriceGroupValueList).Where(x => x.Id == groupValue.Id);
			foreach (var group in groups)
				group.ClusterSettingId = groupValue.ClusterSettingId;
		}

		private CompanySaleSchema GetSaleSchema(int companyId)
		{
			var companyVersionInfo = new MariPriceDb.Price.Company.List
			{
				CompanyIds = new[] { companyId }
			}.Exec(m_sql)
			?.FirstOrDefault();

			if (companyVersionInfo == null)
				throw new System.Exception($"Could not find PriceCompany for company {companyId}");

			var priceVersionInfo = new MariPriceDb.Price.PriceVersion.List
			{
				VersionIds = new[] { companyVersionInfo.DraftVersionId }
			}.Exec(m_sql)
			?.FirstOrDefault();

			if (priceVersionInfo == null)
				throw new System.Exception($"Could not find PriceVersion for company {companyId}");

			return new CompanySaleSchema
			{
				WithNdsPriceRequired = priceVersionInfo.WithNdsPriceRequired,
				WithoutNdsPriceRequired = priceVersionInfo.WithoutNdsPriceRequired
			};
		}

		private List<PriceGroup> Parse(XSSFWorkbook workbook)
		{
			var result = new List<PriceGroup>();
			ISheet sheet = workbook.GetSheetAt(0);

			var priceGroupValueList = new List<PriceGroupValue>();

			// заголовок
			IRow headerRow = sheet.GetRow(0);
			int cellCount = headerRow.LastCellNum;

			for (int j = 0; j < cellCount; j++)
			{
				if (j <= 1) continue;

				ICell cell = headerRow.GetCell(j);

				if (cell == null || string.IsNullOrWhiteSpace(cell.ToString()))
					continue;

				// От 100 г (срок 35 дн)
				PriceGroupValue groupValue = ParsePriceGroupValue(cell.ToString());

				// признак Дубликат
				if (priceGroupValueList.Any(x => x.GetHashCode() == groupValue.GetHashCode()))
					groupValue.IsDuplicate = true;

				priceGroupValueList.Add(groupValue);
			}

			// строки со значениями

			var startRow = sheet.FirstRowNum + 2;
			var lastRow = sheet.LastRowNum;

			for (int i = startRow; i <= lastRow; i++)
			{
				IRow row = sheet.GetRow(i);

				if (row == null) continue;

				if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

				var priceGroup = new PriceGroup();

				// значения в строке

				for (int j = row.FirstCellNum; j < cellCount; j++)
				{
					ICell cell = row.GetCell(j);

					int groupIndex = j < 2 ? 0 : (j - 2) / 2; // 2, 3, 4 ...

					PriceGroupValue priceGroupValue;
					if (priceGroup.PriceGroupValueList.Count == groupIndex)
					{
						priceGroupValue = priceGroupValueList[groupIndex].Clone();
						priceGroup.PriceGroupValueList.Add(priceGroupValue);
					}
					else
					{
						priceGroupValue = priceGroup.PriceGroupValueList[groupIndex];
					}

					// название группы
					if (j == 0)
					{
						priceGroup.Name = cell.ToString();
						if (string.IsNullOrWhiteSpace(priceGroup.Name))
							priceGroup.AppendError($"Не указано название группы");
						continue;
					}

					// % потерь
					if (j == 1)
					{
						decimal lossPercentage;
						if (decimal.TryParse(cell.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out lossPercentage))
							priceGroup.LossPercentage = lossPercentage;
						else
							priceGroup.AppendError($"Ошибка в формате % потерь '{cell}'");
						continue;
					}

					// стоимость с НДС/без НДС
					decimal price;
					if (!String.IsNullOrWhiteSpace(cell.ToString()))
					{
						if (decimal.TryParse(cell.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out price))
						{
							if ((j - 2) % 2 == 0)
								priceGroupValue.WithNdsPrice = price;
							else
								priceGroupValue.WithoutNdsPrice = price;
						}
					}
				}

				result.Add(priceGroup);
			}

			return result;
		}

		private PriceGroupValue ParsePriceGroupValue(string value)
		{
			var groupValue = new PriceGroupValue();

			// От 100 г. (срок 35 дн.)
			var parts = value.Trim().Split(" ");

			if (parts.Length != 6)
				throw new System.Exception($"Ошибка в формате вариант исполнения '{value}'");

			decimal orderMetalWeight;
			if (decimal.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out orderMetalWeight))
				groupValue.OrderMetalWeight = orderMetalWeight;
			else
				throw new System.Exception($"Ошибка в значении количество металла '{value}'");

			int productionTime;
			if (int.TryParse(parts[4], out productionTime))
				groupValue.ProductionTime = productionTime;
			else
				throw new System.Exception($"Ошибка в значении срок изготовления '{value}'");

			return groupValue;
		}

		private XSSFWorkbook GetWorkbook(string url)
		{
			var client = new WebClient();
			var stream = new MemoryStream(client.DownloadData(url));

			var contentType = client.ResponseHeaders["Content-Type"];

			// проверить контент-тип файла
			if (contentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
				throw new System.Exception($"Неизвестный тип файла '{contentType}'");

			return new XSSFWorkbook(stream);
		}

		class PriceGroupValue
		{

			public Guid Id { get; set; }

			public int ClusterSettingId { get; set; }
			public int PriceGroupValueId { get; set; }

			public decimal OrderMetalWeight { get; set; }
			public int ProductionTime { get; set; }

			public decimal? WithNdsPrice { get; set; }
			public decimal? WithoutNdsPrice { get; set; }
			
			public PriceGroupValue()
			{
				Id = Guid.NewGuid();
			}

			public PriceGroupValue Clone()
			{
				return (PriceGroupValue)this.MemberwiseClone();
			}

			public override int GetHashCode()
			{
				return $"{OrderMetalWeight:F2}|{ProductionTime}".GetHashCode();
			}

			public bool IsDuplicate { get; set; }
		}

		class PriceGroup
		{
			private List<string> _errors = new List<string>();

			public int PriceGroupId { get; set; }

			public string Name { get; set; }

			public decimal LossPercentage { get; set; }

			public List<PriceGroupValue> PriceGroupValueList { get; set; }

			public PriceGroup()
			{
				PriceGroupValueList = new List<PriceGroupValue>();
			}
			public void AppendError(string error)
			{
				_errors.Add(error);
			}

			public string Error { get { return string.Join(".", _errors); } }
		}

		class CompanySaleSchema
		{
			public bool WithNdsPriceRequired { get; set; }
			public bool WithoutNdsPriceRequired { get; set; }
		}

	}
}
