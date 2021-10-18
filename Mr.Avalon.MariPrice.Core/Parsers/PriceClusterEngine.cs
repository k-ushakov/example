//using d7k.Dto;
//using Mr.Avalon.Common;
//using Mr.Avalon.MariPrice.Client;
//using Mr.Avalon.Spec.Client;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Utilities.Sql;

//namespace Mr.Avalon.MariPrice.Core
//{
//	public partial class PriceClusterEngine
//	{
//		private ISqlFactory m_sql;
//		private DtoComplex m_dto;
//		private ISpecEngine m_specEngine;
//		private SpecSettings m_specSettings;

//		public PriceClusterEngine(ISqlFactory sql, DtoComplex dto, ISpecEngine specEngine, SpecSettings specSettings)
//		{
//			m_sql = sql;
//			m_dto = dto;
//			m_specEngine = specEngine;
//			m_specSettings = specSettings;
//		}

//		public int Create(MariPriceApi.Price.Cluster.Create request)
//		{
//			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

//			ValidateName(request.Name);
//			// TO DO
//			//ValidateCrc(request.VariantsSettings);

//			var dbRequest = new MariPriceDb.Price.Cluster.Create().CopyFrom(request, m_dto);
//			dbRequest.Enabled = request.Enabled;

//			var id = dbRequest.Exec(m_sql);

//			return id;
//		}

//		public void Update(MariPriceApi.Price.Cluster.Update request, MariPriceDb.Price.Cluster originCluster = null)
//		{
//			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

//			originCluster = originCluster ?? new MariPriceDb.Price.Cluster.List().ForClusterIds(request.Id).Exec(m_sql).SingleOrDefault();
//			if (originCluster == null)
//				throw new RecordNotFoundApiException("There is no specific cluster");

//			if (request.Name != originCluster.Name)
//				ValidateName(request.Name);

//			var dbRequest = new MariPriceDb.Price.Cluster.Update()
//				.CopyFrom(request, m_dto);

//			dbRequest.UpdationList = request.UpdationList;

//			dbRequest.Exec(m_sql);
//		}

//		public void Delete(int clusterId)
//		{
//			var dbClusterSettings = new MariPriceDb.Price.ClusterSetting.Delete() { ClusterId = clusterId };
//			var dbCluster = new MariPriceDb.Price.Cluster.Delete() { PriceClusterId = clusterId };
//			var dbCusterProdut = new MariPriceDb.Price.Product.ProductClusterDelete() { PriceClusterId = clusterId };

//			using (var transSql = m_sql.Transaction())
//			{
//				dbCusterProdut.Exec(transSql);
//				dbClusterSettings.Exec(transSql);
//				dbCluster.Exec(transSql);
//				transSql.Commit();
//			}
//		}

//		public void CusterSwap(MariPriceApi.Price.PriceCompany.Swap request)
//		{
//			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

//			var dbSwap = new MariPriceDb.Price.Company.Swap() { CompanyId = request.CompanyId };
//			dbSwap.Exec(m_sql);
//		}

//		public List<MariPriceApi.Price.Cluster> Get(MariPriceApi.Price.Cluster.List request)
//		{
//			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

//			var dbRequest = new MariPriceDb.Price.Cluster.List()
//				.ForClusterIds(request.Ids?.ToArray())
//				.ForVersionIds(request.VersionIds?.ToArray());

//			var dbResult = dbRequest.Exec(m_sql);
//			var settings = new List<MariPriceDb.Price.ClusterSetting>();
//			if (dbResult.Any())
//			{
//				settings = new MariPriceDb.Price.ClusterSetting.List()
//					.ForClusters(dbResult.Select(i => i.Id).ToArray())
//					.Exec(m_sql);
//			}

//			var result = new List<MariPriceApi.Price.Cluster>();
//			foreach (var item in dbResult)
//			{
//				var newItem = new MariPriceApi.Price.Cluster().CopyFrom(item, m_dto);
//				var currentSettings = settings.Where(x => x.ClusterId == newItem.Id)?.ToList() ?? new List<MariPriceDb.Price.ClusterSetting>();
//				if (currentSettings.Any())
//					newItem.VariantsSettings = currentSettings.Select(x => new MariPriceApi.Price.ClusterSetting().CopyFrom(x, m_dto)).ToList();

//				result.Add(newItem);
//			}

//			return result;
//		}

//		public List<MariPriceApi.Price.Cluster> GetOnlyClusterInfo(MariPriceApi.Price.Cluster.List request)
//		{
//			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

//			var dbRequest = new MariPriceDb.Price.Cluster.List()
//				.ForClusterIds(request.Ids?.ToArray())
//				.ForVersionIds(request.VersionIds?.ToArray());

//			var dbResult = dbRequest.Exec(m_sql);
//			var result = new List<MariPriceApi.Price.Cluster>();
//			foreach (var item in dbResult)
//			{
//				var newItem = new MariPriceApi.Price.Cluster().CopyFrom(item, m_dto);

//				result.Add(newItem);
//			}

//			return result;
//		}

//		// TO DO
//		//public void UpdateCompany(MariPriceApi.Price.CompanySettingsUpdate request)
//		//{
//		//	request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

//		//	var dbRequest = new MariPriceDb.Price.Company.Update
//		//	{
//		//		CompanyId = request.Id,
//		//		WithNdsPriceRequired = request.WithNdsSaleRequired,
//		//		WithoutNdsPriceRequired = request.WithoutNdsSaleRequired,
//		//	}.DefaultUpdationList();

//		//	dbRequest.Exec(m_sql);
//		//}

//		private void ValidateCrc(List<MariPriceApi.Price.ClusterSetting> variants)
//		{
//			HashSet<string> crcSet = new HashSet<string>();

//			foreach (var variant in variants)
//				if (!crcSet.Add(GetVariantCrc(variant)))
//					throw new RecordDuplicateApiException("There are non-unique variants");
//		}

//		private string GetVariantCrc(MariPriceApi.Price.ClusterSetting variant)
//		{
//			return $"{variant.OrderMetalWeight}#%#{variant.ProductionTime}";
//		}

//		private void ValidateName(Guid clusterName)
//		{
//			var name = m_specEngine.GetVocValues(m_specSettings.PriceClusters, clusterName).Items.SingleOrDefault();

//			if (name == null)
//				throw new RecordNotFoundApiException("There is no such Cluster name");

//			if (name.State == SpecApi.VocValueState.Deleted)
//				throw new ConflictApiException("Specific Cluster name is not available now");
//		}
//	}
//}
