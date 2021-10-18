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

	public class PortalPriceClusterEngine
	{
		private PriceEngine m_engine;
		private DtoComplex m_dto;
		private ISpecEngine m_specEngine;
		private SpecSettings m_specSettings;

		public PortalPriceClusterEngine(PriceEngine engine, DtoComplex dto, ISpecEngine specEngine, SpecSettings specSettings)
		{
			m_engine = engine;
			m_dto = dto;
			m_specEngine = specEngine;
			m_specSettings = specSettings;
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateCluster(MariPriceApi.PortalPrice.Cluster.Update request)
		{
			var updateRequest = new MariPriceApi.Price.Cluster.Update();
			updateRequest.Id = request.ClusterId;
			updateRequest.Name = request.ClusterName;
			updateRequest.Enabled = request.Enabled;
			updateRequest.Metal = request.ClusterMetal;
			updateRequest.Quality = request.ClusterQuality;
			updateRequest.UpdationList = new string[] { 
				nameof(updateRequest.Enabled), 
				nameof(updateRequest.Name),
				nameof(updateRequest.Metal),
				nameof(updateRequest.Quality)
			};

			m_engine.UpdateCluster(updateRequest);

			return GetClusterInfos(request.ClusterId).Single();
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateInOrderFlag(MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag request)
		{
			var updateRequest = new MariPriceApi.Price.Cluster.Update();
			updateRequest.Id = request.ClusterId;
			updateRequest.InOrder = request.InOrder;
			updateRequest.UpdationList = new string[] { nameof(updateRequest.InOrder) };

			m_engine.UpdateCluster(updateRequest);

			return GetClusterInfos(request.ClusterId).Single();
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateInstockFlag(MariPriceApi.PortalPrice.Cluster.UpdateInStockFlag request)
		{
			var updateRequest = new MariPriceApi.Price.Cluster.Update();
			updateRequest.Id = request.ClusterId;
			updateRequest.InStock = request.InStock;
			updateRequest.UpdationList = new string[] { nameof(updateRequest.InStock) };

			m_engine.UpdateCluster(updateRequest);

			return GetClusterInfos(request.ClusterId).Single();
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateClusterStatsus(MariPriceApi.PortalPrice.Cluster.UpdateStatus request)
		{
			var updateRequest = new MariPriceApi.Price.Cluster.Update();
			updateRequest.Enabled = request.Enabled;

			updateRequest.UpdationList = new string[] { nameof(updateRequest.Enabled) };

			m_engine.UpdateCluster(updateRequest);

			return GetClusterInfos(request.ClusterId).Single();
		}

		public MariPriceApi.PortalPrice.Cluster.Info Create(MariPriceApi.PortalPrice.Cluster.Create request)
		{
			var apiRequest = new MariPriceApi.Price.Cluster.Create()
			{
				Name = request.ClusterName,
				VersionId = request.VersionId,
				Enabled = true,
				InOrder = false,
				InStock = false,
				Metal = request.ClusterMetal,
				Quality = request.ClusterQuality
			};

			var clusterId = m_engine.CreateCluster(apiRequest);
			var cluster = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(clusterId)).Single();

			var names = m_specEngine.GetVocValues(m_specSettings.PriceClusters).Items.ToDictionary(x => x.Id, x => x.Value);

			var result = new MariPriceApi.PortalPrice.Cluster.Info()
			{
				ClusterName = new MariPriceApi.EntityName<Guid>()
				{
					Id = cluster.Name,
					Name = names.Get(cluster.Name)
				}
			}.CopyFrom(cluster, m_dto);

			return result;
		}

		public List<MariPriceApi.PortalPrice.Cluster.Info> GetClusterInfos(params int[] Ids)
		{
			var clusters = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(Ids));
			if (!clusters.Any())
				return new List<MariPriceApi.PortalPrice.Cluster.Info>();

			var names = m_specEngine.GetVocValues(m_specSettings.PriceClusters).Items.ToDictionary(x => x.Id, x => x.Value);

			var result = clusters.Select(x => new MariPriceApi.PortalPrice.Cluster.Info()
			{
				ClusterName = new MariPriceApi.EntityName<Guid>
				{
					Id = x.Name,
					Name = names.Get(x.Name)
				}
			}.CopyFrom(x, m_dto)).ToList();
			return result;
		}

		// TO DO
		public void SwitchItemsOfOneCluster(MariPriceApi.PortalPrice.Cluster.SwitchItem[] switchers, MariPriceApi.Price.Cluster cluster, List<MariPriceApi.Price.Group> clusterGroups)
		{
			var groups = clusterGroups.Select(x => x.Values.ToDictionary(v => v.PriceGroupValueId)).ToList();
			var switchersDict = switchers.ToDictionary(x => x.SettingsVariantId);

			var updateRequest = new MariPriceApi.Price.Cluster.Update().CopyFrom(cluster, m_dto);

			//foreach (var updatingVariant in updateRequest.VariantsSettings)
			//{
			//	if (switchersDict.ContainsKey(updatingVariant.Id))
			//		updatingVariant.Enabled = switchersDict[updatingVariant.Id].Enable;
			//}

			//m_engine.UpdateCluster(updateRequest);
		}

		public void Append(MariPriceApi.PortalPrice.Cluster.Append request)
		{
			var apiGetRequest = new MariPriceApi.Price.Cluster.List().ForIds(request.ClusterId);
			var originCluster = m_engine.GetCluster(apiGetRequest, true).SingleOrDefault();

			if (originCluster == null)
				throw new RecordNotFoundApiException($"There is no cluster with id '{request.ClusterId}'");

			var create = new MariPriceApi.Price.ClusterSetting.Create().CopyFrom(request, m_dto);
			var id = m_engine.CreateSetting(create);

			var groups = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(request.ClusterId));
			foreach (var item in groups)
			{
				m_engine.CreateGroupValue(new MariPriceApi.Price.GroupValue.Create() { PriceGroupId = item.Id, PriceClusterVariantId = id });
			}
		}

		public void DeleteCluster(MariPriceApi.PortalPrice.Cluster.Delete request, UserInfo userInfo)
		{
			//TO DO ADD VALIDATION
			m_engine.DeleteCluster(request.ClusterId);
		}

		public MariPriceApi.Price.ClusterSetting UpdateItem(MariPriceApi.PortalPrice.Cluster.UpdateItem request, MariPriceApi.Price.Cluster originCluster)
		{
			var apiRequest = new MariPriceApi.Price.ClusterSetting.Update()
			{
				Id = request.SettingsVariantId,
				OrderMetalWeight = request.OrderMetalWeight,
				ProductionTime = request.ProductionTime
			};

			m_engine.UpdateSetting(apiRequest);

			return GetClusterInfos(request.ClusterId).Single().Variants.Single(x => x.Id == request.SettingsVariantId);
		}

		public MariPriceApi.PortalPrice.Cluster.Info DeleteItem(MariPriceApi.PortalPrice.Cluster.DeleteItem request, MariPriceApi.Price.Cluster originCluster)
		{
			var apiRequest = new MariPriceApi.Price.ClusterSetting.Delete();
			apiRequest.ClusterId = request.ClusterId;
			apiRequest.SettingsVariantId = request.SettingsVariantId;

			m_engine.DeleteSetting(apiRequest);

			return GetClusterInfos(request.ClusterId).Single();
		}

		public void Swap(MariPriceApi.PortalPrice.PriceCompany.Swap request)
		{
			var apiRequest = new MariPriceApi.Price.PriceCompany.Swap().CopyFrom(request, m_dto);
			m_engine.ClusterSwap(apiRequest);
		}
	}
}
