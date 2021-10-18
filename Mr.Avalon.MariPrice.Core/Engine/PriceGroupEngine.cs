using d7k.Dto;
using Mr.Avalon.MariPrice.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class PriceGroupEngine
	{
		private ISqlFactory m_sql;
		private DtoComplex m_dto;
		private PriceClusterEngine m_cluster;

		public PriceGroupEngine(ISqlFactory sql, DtoComplex dto, PriceClusterEngine cluster)
		{
			m_sql = sql;
			m_dto = dto;
			m_cluster = cluster;
		}

		public List<MariPriceApi.Price.Group> Get(MariPriceApi.Price.Group.List request, bool withoutSettings)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbGroupRequest = new MariPriceDb.Price.Group.List()
				.ForIds(request.Ids.ToArray())
				.ForClusters(request.ClusterIds?.ToArray())
				.ForName(request.Name);

			var groups = dbGroupRequest.Exec(m_sql);

			var values = new List<MariPriceDb.Price.GroupValue>();
			var instockValues = new List<MariPriceDb.Price.InstockGroupValue>();
			if ((!withoutSettings) && groups.Any())
			{
				values = new MariPriceDb.Price.GroupValue.List()
					.ForGroupIds(groups.Select(i => i.Id).ToArray())
					.Exec(m_sql);
				instockValues = new MariPriceDb.Price.InstockGroupValue.List()
					.ForGroupIds(groups.Select(i => i.Id).ToArray())
					.Exec(m_sql);
			}

			var result = new List<MariPriceApi.Price.Group>();
			foreach (var item in groups)
			{
				var groupToAdd = new MariPriceApi.Price.Group().CopyFrom(item, m_dto);

				groupToAdd.TotalLossPercentage = GetTotalLost(groupToAdd.LossPercentage, groupToAdd.AdditionalLossPercentage);

				var currentValue = values.Where(x => x.PriceGroupId == groupToAdd.Id)?.ToList() ?? new List<MariPriceDb.Price.GroupValue>();
				if (currentValue.Any())
					groupToAdd.Values = currentValue.Select(x => new MariPriceApi.Price.GroupValue().CopyFrom(x, m_dto))?.ToList();

				var currentInstockValue = instockValues.FirstOrDefault(x => x.PriceGroupId == groupToAdd.Id);
				if (currentInstockValue != null)
					groupToAdd.InStockValues = new MariPriceApi.Price.InstockGroupValue().CopyFrom(currentInstockValue, m_dto);

				result.Add(groupToAdd);
			}

			return result;
		}


		public List<MariPriceApi.Price.GroupWithVersion> GetWithVersion(MariPriceApi.Price.Group.List request, bool withoutSettings)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbGroupRequest = new MariPriceDb.Price.Group.ListWithVersion()
				.ForIds(request.Ids.ToArray())
				.ForClusters(request.ClusterIds?.ToArray())
				.ForName(request.Name);


			var groups = dbGroupRequest.Exec(m_sql);

			// need to load values
			var settings = new List<MariPriceDb.Price.GroupValue>();
			if ((!withoutSettings) && groups.Any())
			{
				settings = new MariPriceDb.Price.GroupValue.List()
					.ForGroupIds(groups.Select(i => i.Id).ToArray())
					.Exec(m_sql);
			}

			// include name lostpercent clusteid
			var result = new List<MariPriceApi.Price.GroupWithVersion>();
			foreach (var item in groups)
			{
				var newItem = new MariPriceApi.Price.GroupWithVersion().CopyFrom(item, m_dto);

				newItem.TotalLossPercentage = GetTotalLost(newItem.LossPercentage, newItem.AdditionalLossPercentage);

				if (!withoutSettings)
				{
					var currentSettings = settings.Where(x => x.PriceGroupId == newItem.Id)?.ToList() ?? new List<MariPriceDb.Price.GroupValue>();
					if (currentSettings.Any())
						newItem.Values = currentSettings.Select(x => new MariPriceApi.Price.GroupValue().CopyFrom(x, m_dto))?.ToList();
				}

				result.Add(newItem);
			}

			return result;
		}

		private static decimal GetTotalLost(decimal lostPercentage, decimal? additionalLostPercentage)
		{
			return ((1 + (lostPercentage / 100.0M)) * (1 + ((additionalLostPercentage ?? 0) / 100.0M)) - 1) * 100;
		}

		public int Create(MariPriceApi.Price.Group.Create request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.Group.Create().CopyFrom(request, m_dto);
			dbRequest.Enabled = true;

			var id = dbRequest.Exec(m_sql);

			return id;
		}

		public void Update(MariPriceApi.Price.Group.Update request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			var updationList = request.UpdationList;

			var dbRequest = new MariPriceDb.Price.Group.Update().CopyFrom(request, m_dto);
			dbRequest.UpdationList = updationList;

			dbRequest.Exec(m_sql);
		}

		public void SetPrices(MariPriceApi.Price.Group.SetPrices request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.Group.BulkUpdate
			{
				VariantsForGroups = request.Groups.ToDictionary(
					x => x.Key,
					x => new MariPriceDb.Price.Group.BulkUpdate.Item
					{
						Enabled = x.Value?.Any() == true,
						Variants = JsonConvert.SerializeObject(x.Value)
					})
			};

			dbRequest.Exec(m_sql);
		}

		public void Delete(int priceGroupId)
		{
			var dbRequestValue = new MariPriceDb.Price.GroupValue.DeleteByGroup() { PriceGroupId = priceGroupId };
			var dbRequestInstockValue = new MariPriceDb.Price.InstockGroupValue.DeleteByGroup() { PriceGroupId = priceGroupId };
			var dbRequestGroup = new MariPriceDb.Price.Group.Delete() { PriceGroupId = priceGroupId };
			using (var transSql = m_sql.Transaction())
			{
				dbRequestValue.Exec(transSql);
				dbRequestInstockValue.Exec(transSql);
				dbRequestGroup.Exec(transSql);
				transSql.Commit();
			}
		}
	}
}
