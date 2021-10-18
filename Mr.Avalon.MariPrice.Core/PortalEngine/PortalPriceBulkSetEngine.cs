using d7k.Dto;
using d7k.Dto.Utilities;
using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{

	public class PortalPriceBulkSetEngine
	{
		private PriceEngine m_engine;
		private DtoComplex m_dto;

		private PriceEngine m_apiEngine;
		private PortalPriceGroupEngine m_group;
		private PortalPriceClusterEngine m_cluster;
		private PortalPriceTechnologiesEngine m_technologies;

		public PortalPriceBulkSetEngine(PriceEngine engine, DtoComplex dto, PriceEngine apiEngine, PortalPriceGroupEngine group, PortalPriceClusterEngine cluster, PortalPriceTechnologiesEngine technologies)
		{
			m_engine = engine;
			m_dto = dto;
			m_apiEngine = apiEngine;

			m_group = group;
			m_cluster = cluster;
			m_technologies = technologies;
		}

		private void UpdateSwitchers(MariPriceApi.PortalPrice.BulkSet request, Dictionary<int, MariPriceApi.Price.Group> originGroups, Dictionary<int, MariPriceApi.Price.Cluster> originClusters)
		{
			if (request.Switchers == null)
				return;

			var requestByClusters = request.Switchers.GroupBy(x => x.ClusterId).ToDictionary(x => x.Key, x => x.ToArray());

			foreach (var item in requestByClusters)
			{
				m_cluster.SwitchItemsOfOneCluster(item.Value, originClusters[item.Key],
					originGroups.Values.Where(x => x.ClusterId == item.Key).ToList());
			}
		}

		private static void ValidateUpdate(MariPriceApi.PortalPrice.BulkSet request,
			Dictionary<int, MariPriceApi.Price.Group> originGroups, Dictionary<int, MariPriceApi.Price.Cluster> originClusters)
		{
			if (!request.WithNdsPriceRequired && !request.WithoutNdsPriceRequired)
				throw new ConflictApiException("Cannot save source without any price schema");
			if (request.Groups.Any(x => !originGroups.ContainsKey(x.Id)))
				throw new ConflictApiException("There is no access to some groups");

			ValidateSwitchers(request, originClusters);
			ValidateGroups(request, originGroups, originClusters);
		}

		private static void ValidateSwitchers(MariPriceApi.PortalPrice.BulkSet request, Dictionary<int, MariPriceApi.Price.Cluster> originClusters)
		{
		}

		private static void ValidateGroups(MariPriceApi.PortalPrice.BulkSet request, Dictionary<int, MariPriceApi.Price.Group> originGroups, Dictionary<int, MariPriceApi.Price.Cluster> originClusters)
		{
			foreach (var requestedGroup in request.Groups)
			{
				var originGroup = originGroups[requestedGroup.Id];
				var groupCluster = originClusters.Get(originGroup.ClusterId);
				if (groupCluster == null)
					throw new RecordNotFoundApiException($"There is no cluster with id '{originGroup.ClusterId}'");

				if (groupCluster.VariantsSettings.Count != requestedGroup.Variants.Count)
					throw new ConflictApiException("There is different quantity of variants in originCluster and requested switchers");

				foreach (var variant in requestedGroup.Variants)
				{
					var originSettingsVariant = groupCluster.VariantsSettings.SingleOrDefault(x => x.Id == variant.PriceGroupValueId);
					if (originSettingsVariant == null)
						throw new RecordNotFoundApiException($"There is no variant with id '{variant.PriceGroupValueId}'");

					if ((!variant.WithNdsPrice.HasValue && request.WithNdsPriceRequired) ||
						(!variant.WithoutNdsPrice.HasValue && request.WithoutNdsPriceRequired))
						throw new ConflictApiException("Cannot save price groups because empty prices exist");

				}
			}
		}
	}
}
