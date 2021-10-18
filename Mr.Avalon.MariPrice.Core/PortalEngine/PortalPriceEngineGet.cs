using d7k.Dto;
using d7k.Dto.Utilities;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{

	public class PortalPriceEngineGet
	{
		private PriceEngine m_engine;
		private DtoComplex m_dto;
		private ISpecEngine m_specEngine;
		private SpecSettings m_specSettings;
		private ISqlFactory m_sql;

		public PortalPriceEngineGet(PriceEngine engine, DtoComplex dto, ISpecEngine specEngine, SpecSettings specSettings, ISqlFactory sql)
		{
			m_engine = engine;
			m_dto = dto;
			m_specEngine = specEngine;
			m_specSettings = specSettings;
			m_sql = sql;
		}

		// TO DO
		/*public MariPriceApi.PortalPrice GetSummary(int companyId)
		{
			var cluster = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForVersion(companyId));
			var groups = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(cluster.Select(x => x.Id).ToArray()));
			var products = GetProductsAssignedForGroups(groups);
			var technologies = m_engine.GetTechnologyPrices(new MariPriceApi.Price.Technologies.List().ForOwners(companyId)).SingleOrDefault()
				?? new MariPriceApi.Price.Technologies();

			var groupsByClusters = groups.GroupBy(x => x.ClusterId)
				.ToDictionary(x => x.Key, x => x.OrderBy(g => g.Name).ToList());

			var names = m_specEngine.GetVocValues(m_specSettings.PriceClusters).Items.ToDictionary(x => x.Id, x => x.Value);

			var portalClusters = cluster
				.Select(x => FormIntoCluster(x, groupsByClusters.Get(x.Id) ?? Enumerable.Empty<MariPriceApi.Price.Group>(), names, products))
				.OrderBy(x => x.ManufactureSettings.ClusterName.Name)
				.ToList();

			var portalTechnologies = FillTechNames(technologies.TechnologyAdditions.Values.ToList());

			return new MariPriceApi.PortalPrice
			{
				WithNdsPriceRequired = cluster.FirstOrDefault()?.WithNdsPriceRequired ?? false,
				WithoutNdsPriceRequired = cluster.FirstOrDefault()?.WithoutNdsPriceRequired ?? false,
				Technologies = portalTechnologies,
				Clusters = portalClusters
			};
		}*/

		private Dictionary<int, List<MariPriceApi.Price.Product>> GetProductsAssignedForGroups(IEnumerable<MariPriceApi.Price.Group> groups)
		{
			if (groups.Any())
				return m_engine.GetProducts(new MariPriceApi.Price.Product.List().ForPriceGroups(groups.Select(x => x.Id).ToArray()))
								.GroupBy(x => x.PriceGroupId).ToDictionary(x => x.Key.Value, x => x.ToList());

			return new Dictionary<int, List<MariPriceApi.Price.Product>>();
		}

		private MariPriceApi.PortalPrice.Cluster FormIntoCluster(MariPriceApi.Price.Cluster apiCluster, IEnumerable<MariPriceApi.Price.Group> groups,
			Dictionary<Guid, string> names = null, Dictionary<int, List<MariPriceApi.Price.Product>> products = null)
		{
			names = names ?? m_specEngine.GetVocValues(m_specSettings.PriceClusters).Items.ToDictionary(x => x.Id, x => x.Value);
			products = products ?? GetProductsAssignedForGroups(groups);

			var portalGroups = new List<MariPriceApi.PortalPrice.Group>();

			foreach (var group in groups)
				portalGroups.Add(BuildGroup(group, apiCluster.VariantsSettings, products.Get(group.Id)));

			var clusterName = new MariPriceApi.EntityName<Guid>
			{
				Id = apiCluster.Name,
				Name = names.Get(apiCluster.Name)
			};
			var settings = apiCluster == null ? null :
				new MariPriceApi.PortalPrice.Cluster.Info { ClusterName = clusterName }.CopyFrom(apiCluster, m_dto);

			var result = new MariPriceApi.PortalPrice.Cluster
			{
				PriceGroups = portalGroups,
				ManufactureSettings = settings
			};

			return result;
		}

		public List<MariPriceApi.EntityName<int>> GetGroupsShortInfo(MariPriceApi.PortalPrice.Group.SasRequest.SasSource sasRequest, string groupName)
		{
			var request = new MariPriceDb.Price.Group.Search()
				   .WithNameLike(groupName)
				   .TakeOnly(sasRequest.MaxCount)
				   .ForOwners(sasRequest.CompanyIds.ToArray());

			var dbResult = request.Exec(m_sql);

			var result = dbResult
				.Select(x => new MariPriceApi.EntityName<int> { Id = x.Id, Name = x.Name })
				.ToList();

			return result;
		}

		public MariPriceApi.PortalPrice.Group GetGroup(int groupId)
		{
			var apiResult = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(groupId))?.FirstOrDefault();

			if (apiResult == null)
				throw new RecordNotFoundApiException($"Group with Id {groupId} cannot found");

			var result =  new MariPriceApi.PortalPrice.Group().CopyFrom(apiResult, m_dto);

			return result;
		}

		public MariPriceApi.PortalPrice.Cluster GetOneCluster(int clusterId)
		{
			var cluster = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(clusterId)).Single();
			var groups = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(clusterId));

			return FormIntoCluster(cluster, groups);
		}

		public MariPriceApi.PortalPrice.Cluster GetOneClusterOnlyInfo(int clusterId)
		{
			var cluster = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(clusterId))?.Single();

			if (cluster == null)
				throw new RecordNotFoundApiException($"Cannot find cluster with Id {clusterId}");

			return new MariPriceApi.PortalPrice.Cluster()
			{
				ManufactureSettings = new MariPriceApi.PortalPrice.Cluster.Info().CopyFrom(cluster, m_dto)
			};
		}


		private MariPriceApi.PortalPrice.Group BuildGroup(
			MariPriceApi.Price.Group group,
			List<MariPriceApi.Price.ClusterSetting> variantsSettings,
			List<MariPriceApi.Price.Product> products)
		{
			var result = new MariPriceApi.PortalPrice.Group().CopyFrom(group, m_dto);
			result.Products = products?.Select(x => new MariPriceApi.EntityName<int> { Id = x.Id, Name = x.Name }).ToList();

			return result;
		}

		private List<MariPriceApi.Price.Technologies.TechnologyPortal> FillTechNames(List<MariPriceApi.Price.Technologies.TechnologyApi> techs)
		{
			var techNames = m_specEngine.GetVocValues(m_specSettings.Technologies, techs.Select(x => x.TechnologyId).ToArray())
				.Items.ToDictionary(x => x.Id, x => x.Value);

			var result = techs.Select(x => new MariPriceApi.Price.Technologies.TechnologyPortal
			{
				Name = techNames.Get(x.TechnologyId)
			}.CopyFrom(x, m_dto)).ToList();

			return result;
		}
	}
}
