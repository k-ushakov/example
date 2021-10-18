using d7k.Dto;
using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.Event.Client;
using Mr.Avalon.File.Client;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core.Exception;
using Mr.Avalon.MariPrice.Core.Other;
using Mr.Avalon.Print.Client;
using Mr.Avalon.ProfileMari.Client;
using Mr.Avalon.Spec.Client;
using Mr.Avalon.Spec.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public class PortalPriceEngine
	{
		DtoComplex m_dto;
		ProfileApiClient m_profile;
		SpecApiClient m_spec;
		SearchSettings m_searchSettings;
		ISpecEngine m_specEngine;
		SpecSettings m_specSettings;

		PriceEngine m_apiEngine;
		PortalPriceEngineGet m_get;
		PortalPriceBulkSetEngine m_bulkSetEngine;
		PortalPriceClusterEngine m_cluster;
		PortalPriceGroupEngine m_group;

		PortalPriceGroupImportEngine m_groupImport;
		PortalPriceGroupExportEngine m_groupExport;
		PortalPriceProductExportEngine m_productExport;
		PortalPriceProductImportEngine m_productImport;

		PortalPriceTechnologiesEngine m_technologies;


		PortalPriceInstockEngine m_instockEngine;

		IEventSenderClient m_event;
		BulkPublishSettings m_bulkPublishSettings;

		public PortalPriceEngine(
			DtoComplex dto,
			PriceEngine engine,
			ProfileApiClient profile,
			SpecApiClient spec,
			ISpecEngine specEngine,
			SpecSettings specSettings,
			SearchSettings searchSettings,
			ISqlFactory sql,
			IBarcodeStorage barcodeStorage,
			String defaultImageUrl,
			FileApiClient files,
			PrintApiClient printApi,
			IEventSenderClient eventSenderClient,
			BulkPublishSettings bulkPublishSettings)

		{
			m_apiEngine = engine;
			m_profile = profile;
			m_spec = spec;
			m_searchSettings = searchSettings;
			m_specEngine = specEngine;
			m_specSettings = specSettings;
			m_event = eventSenderClient;
			m_bulkPublishSettings = bulkPublishSettings;

			m_get = new PortalPriceEngineGet(engine, dto, specEngine, specSettings, sql);
			m_cluster = new PortalPriceClusterEngine(engine, dto, specEngine, specSettings);
			m_group = new PortalPriceGroupEngine(engine, dto, sql, searchSettings, defaultImageUrl);
			m_technologies = new PortalPriceTechnologiesEngine(engine, dto, specEngine, specSettings);
			m_bulkSetEngine = new PortalPriceBulkSetEngine(engine, dto, m_apiEngine, m_group, m_cluster, m_technologies);

			m_instockEngine = new PortalPriceInstockEngine(dto, barcodeStorage, printApi, engine);

			m_groupImport = new PortalPriceGroupImportEngine(sql, files, engine);
			m_groupExport = new PortalPriceGroupExportEngine(sql, files);
			m_productImport = new PortalPriceProductImportEngine(sql, engine, m_group, files);
			m_productExport = new PortalPriceProductExportEngine(sql, files);

			m_dto = dto;
		}

		public MariPriceApi.PortalPrice GetActivePrice(int companyId, UserInfo userInfo)
		{
			ValidateCompanyAccess(companyId, userInfo.UserId);

			var compPrice = m_apiEngine.GetCompanyPrice(companyId);
			return GetPriceByVersion(compPrice.ActiveVersionId);
		}

		private MariPriceApi.PortalPrice GetPriceByVersion(int versionId)
		{
			var result = new MariPriceApi.PortalPrice();

			result.VersionId = versionId;
			var version = m_apiEngine.GetVersion(versionId);

			result.WithNdsPriceRequired = version.WithNdsPriceRequired;
			result.WithoutNdsPriceRequired = version.WithoutNdsPriceRequired;

			result.Technologies = m_technologies.Get(versionId)?.Technologies ?? new List<MariPriceApi.Price.Technologies.TechnologyPortal>();

			var clusters = m_apiEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForVersion(version.VersionId));

			if (clusters.Any())
			{
				var groups = m_apiEngine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(clusters.Select(x => x.Id).ToArray()));

				var accumClusters = new List<MariPriceApi.PortalPrice.Cluster>();

				var clusterNames = m_specEngine.GetVocValues(m_specSettings.PriceClusters, clusters.Select(x => x.Name).ToArray()).Items.ToDictionary(x => x.Id, x => x.Value);
				var metalNames = m_specEngine.GetVocValues(m_specSettings.Metal, clusters.Where(x => x.Metal.HasValue).Select(x => x.Metal.Value).ToArray()).Items.ToDictionary(x => x.Id, x => x.Value);
				var qualityNames = m_specEngine.GetVocValues(m_specSettings.Quality, clusters.Where(x => x.Metal.HasValue).Select(x => x.Quality.Value).ToArray()).Items.ToDictionary(x => x.Id, x => x.Value);

				var lastBulkResultByClusters = new SpecApi.Product.LastBulkResultByClusters() { ClusterIds = clusters.Select(x => x.Id).ToList() }.Exec(m_spec);

				foreach (var cluster in clusters)
				{
					var newItem = new MariPriceApi.PortalPrice.Cluster();
					newItem.ManufactureSettings = new MariPriceApi.PortalPrice.Cluster.Info().CopyFrom(cluster, m_dto);

					if (lastBulkResultByClusters.BulkInformationByClusters.Where(x => x.ClusterId == cluster.Id).FirstOrDefault() != null)
						newItem.LastClusterPublishInfo = new MariPrice.Client.Portal.BulkPublishInformationByCluster().CopyFrom(lastBulkResultByClusters.BulkInformationByClusters.Where(x => x.ClusterId == cluster.Id).FirstOrDefault(), m_dto);

					newItem.ManufactureSettings.ClusterName = new MariPriceApi.EntityName<Guid>() { Id = cluster.Name, Name = clusterNames.ContainsKey(cluster.Name) ? clusterNames[cluster.Name] : null };
					newItem.ManufactureSettings.ClusterMetal = new MariPriceApi.EntityName<Guid> { Id = cluster.Metal ?? Guid.Empty, Name = metalNames.ContainsKey(cluster.Metal ?? Guid.Empty) ? metalNames[cluster.Metal ?? Guid.Empty] : null };
					newItem.ManufactureSettings.ClusterQuality = new MariPriceApi.EntityName<Guid> { Id = cluster.Quality ?? Guid.Empty, Name = qualityNames.ContainsKey(cluster.Quality ?? Guid.Empty) ? qualityNames[cluster.Quality ?? Guid.Empty] : null };

					var currentgroups = groups.Where(x => x.ClusterId == cluster.Id)?.ToList() ?? new List<MariPriceApi.Price.Group>();
					if (currentgroups.Any())
						newItem.PriceGroups = currentgroups.Select(x => new MariPriceApi.PortalPrice.Group().CopyFrom(x, m_dto))?.ToList();
					accumClusters.Add(newItem);
				}
				result.Clusters = accumClusters;
			}

			return result;
		}

		public MariPriceApi.PortalPrice LoadActiveVerson(MariPriceApi.PortalPrice.PriceCompany.LoadActiveVersion request, UserInfo userInfo)
		{
			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			m_apiEngine.LoadActiveVersion(request.CompanyId);

			var compPrice = m_apiEngine.GetCompanyPrice(request.CompanyId);
			return GetPriceByVersion(compPrice.DraftVersionId);
		}

		public MariPriceApi.PortalPrice.InstockGroupValue UpdateGroupInstockValue(MariPriceApi.PortalPrice.InstockGroupValue.Update request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var versionInfo = m_apiEngine.GetCompanyInfoByGroupId(request.PriceGroupId);

			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_group.UpdateGroupInstockVaue(request);
		}

		public void PublishProduct(MariPriceApi.PortalPrice.Cluster.PublishProduct request, UserInfo userInfo)
		{
			var productsWithActiveCluster = m_apiEngine.GetProductsActiveCluster(new MariPriceApi.Price.Product.List().ForClusters(request.ClusterId), true);

			string clusterMetal = GetClusterMetal(request.ClusterId);
			var notSameClusterMetal = productsWithActiveCluster.Where(x => x.Metal != clusterMetal)?.Select(x => x.ProductUid)?.ToList() ?? new List<Guid>();
			productsWithActiveCluster = productsWithActiveCluster.Where(x => x.Metal == clusterMetal).ToList();

			var lstToPublish = new List<Guid>();
			var lstToArchive = new List<Guid>();
			var withoutSizeGroup = productsWithActiveCluster?.Where(x => x.SizeUid == Guid.Empty)?.Select(x => x.ProductUid)?.ToList() ?? new List<Guid>();
			var sizeGroup = productsWithActiveCluster?.Where(x => x.SizeUid != Guid.Empty)?.Select(x => x.ProductUid)?.ToList() ?? new List<Guid>();

			if (notSameClusterMetal.Any())
				lstToArchive.AddRange(notSameClusterMetal);

			if (withoutSizeGroup.Any())
				lstToPublish.AddRange(withoutSizeGroup);

			if (sizeGroup.Any())
				foreach (var item in sizeGroup)
					if (!withoutSizeGroup.Contains(item))
						lstToArchive.Add(item);

			var productsWithNotActiveCluster = m_apiEngine.GetProductsActiveCluster(new MariPriceApi.Price.Product.List().ForClusters(request.ClusterId), false);

			if (productsWithNotActiveCluster.Any() || lstToArchive.Any())
			{
				lstToArchive.AddRange(productsWithNotActiveCluster.Select(x => x.ProductUid).ToList());
			}

			EventDataBulkToProductWF eventData = new EventDataBulkToProductWF();
			eventData.ClusterId = request.ClusterId;
			eventData.BulkId = Guid.NewGuid();

			List<ProductAndOperation> produstToWF = new List<ProductAndOperation>();

			foreach (var item in lstToPublish)
			{
				ProductAndOperation newItem = new ProductAndOperation() { Action = ProductAction.Publish, ProductId = item };
				produstToWF.Add(newItem);
			}

			foreach (var item in lstToArchive)
			{
				ProductAndOperation newItem = new ProductAndOperation() { Action = ProductAction.ToArchive, ProductId = item };
				produstToWF.Add(newItem);
			}

			int productSend = 0;
			do
			{
				eventData.ProductAndOperations.Clear();
				eventData.ProductAndOperations.AddRange(produstToWF.Skip(productSend).Take(m_bulkPublishSettings.MaxCount));
				productSend = productSend + eventData.ProductAndOperations.Count();
				var eventAggregate = new EventApi.Send().SpecProduct("Mr-Avalon-Product-Bulk-To-WF", body: eventData);
				eventAggregate.Exec(m_event);
			} while (produstToWF.Count > productSend);
		}

		private string GetClusterMetal(int clusterId)
		{
			var clusters = m_apiEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(clusterId), true);
			var cluster = clusters?.FirstOrDefault();

			// загрузить из vocs метал и пробу, затем сформировать название металла
			var metalNames = m_specEngine.GetVocValues(m_specSettings.Metal, clusters?.Where(x => x.Metal.HasValue)?.Select(x => x.Metal.Value)?.ToArray())?.Items?.ToDictionary(x => x.Id, x => x.Value);
			var qualityNames = m_specEngine.GetVocValues(m_specSettings.Quality, clusters?.Where(x => x.Metal.HasValue)?.Select(x => x.Quality.Value)?.ToArray())?.Items?.ToDictionary(x => x.Id, x => x.Value);

			var metal = metalNames.ContainsKey(cluster?.Metal ?? Guid.Empty) ? metalNames[cluster?.Metal ?? Guid.Empty] : null;
			var quality = qualityNames.ContainsKey(cluster?.Quality ?? Guid.Empty) ? qualityNames[cluster?.Quality ?? Guid.Empty] : null;

			return $"{metal} {quality}".Trim();
		}

		public MariPriceApi.PortalPrice DeleteCluster(MariPriceApi.PortalPrice.Cluster.Delete request, UserInfo userInfo)
		{
			var cluster = m_get.GetOneCluster(request.ClusterId);
			var version = cluster.ManufactureSettings.VersionId;

			var versionInfo = m_apiEngine.GetCompanyInfoByVersion(version);
			if (versionInfo.ActiveVersionId == version)
				throw new ValidationApiException("Cannot change the active claster");

			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			if (cluster.PriceGroups?.Any() == true)
				foreach (var group in cluster.PriceGroups)
				{

					m_apiEngine.RemoveFromProductLinkByGroup(group.Id);
					m_group.DeleteGroup(group.Id);
				}

			m_cluster.DeleteCluster(request, userInfo);

			return GetPriceByVersion(version);
		}

		public MariPriceApi.PortalPrice.Cluster DeleteGroup(MariPriceApi.PortalPrice.Group.Delete request, UserInfo userInfo)
		{
			var group = m_group.GetById(request.PriceGroupId);
			var cluster = m_get.GetOneClusterOnlyInfo(group.ClusterId);
			var version = cluster.ManufactureSettings.VersionId;

			var versionInfo = m_apiEngine.GetCompanyInfoByVersion(version);
			if (versionInfo.ActiveVersionId == version)
				throw new ValidationApiException("Cannot change the active claster");

			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			m_apiEngine.RemoveFromProductLinkByGroup(request.PriceGroupId);
			m_group.DeleteGroup(request.PriceGroupId);

			return m_get.GetOneCluster(group.ClusterId);
		}

		public MariPriceApi.PortalPrice GetDraftPrice(int companyId, UserInfo userInfo)
		{
			ValidateCompanyAccess(companyId, userInfo.UserId);

			var compPrice = m_apiEngine.GetCompanyPrice(companyId);

			return GetPriceByVersion(compPrice.DraftVersionId);
		}

		public MariPriceApi.PortalPrice.Instock.ImportRequest.Result ImportBarcodes(MariPriceApi.PortalPrice.Instock.ImportRequest request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_instockEngine.ImportBarcodes(request, userInfo);
		}

		public MariPriceApi.PortalPrice.Instock.Export.Result ExportBarcodes(MariPriceApi.PortalPrice.Instock.Export request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_instockEngine.ExportBarcodes(request, userInfo);
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateCluster(MariPriceApi.PortalPrice.Cluster.Update request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var cluster = m_get.GetOneClusterOnlyInfo(request.ClusterId);
			var versionInfo = m_apiEngine.GetCompanyInfoByClusterId(request.ClusterId);
			if (versionInfo.ActiveVersionId == cluster.ManufactureSettings.VersionId)
				throw new ValidationApiException("Cannot change the active claster");
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_cluster.UpdateCluster(request);
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateInOrderFlag(MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var cluster = m_get.GetOneClusterOnlyInfo(request.ClusterId);
			var versionInfo = m_apiEngine.GetCompanyInfoByClusterId(request.ClusterId);
			if (versionInfo.ActiveVersionId == cluster.ManufactureSettings.VersionId)
				throw new ValidationApiException("Cannot change the active claster");
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_cluster.UpdateInOrderFlag(request);
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateClusterInStockFlug(MariPriceApi.PortalPrice.Cluster.UpdateInStockFlag request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var cluster = m_get.GetOneClusterOnlyInfo(request.ClusterId);
			var versionInfo = m_apiEngine.GetCompanyInfoByClusterId(request.ClusterId);
			if (versionInfo.ActiveVersionId == cluster.ManufactureSettings.VersionId)
				throw new ValidationApiException("Cannot change the active claster");
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_cluster.UpdateInstockFlag(request);
		}

		public MariPriceApi.Price.Technologies.TechnologyPortal LoadActiveTechnology(MariPriceApi.PortalPrice.TechnologiesAdditions.LoadActiveVersion request, UserInfo userInfo)
		{

			return new MariPriceApi.Price.Technologies.TechnologyPortal();
		}

		public MariPriceApi.PortalPrice.Cluster.Info UpdateClusterStatus(MariPriceApi.PortalPrice.Cluster.UpdateStatus request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var cluster = m_get.GetOneClusterOnlyInfo(request.ClusterId);
			var versionInfo = m_apiEngine.GetCompanyInfoByClusterId(request.ClusterId);
			if (versionInfo.ActiveVersionId == cluster.ManufactureSettings.VersionId)
				throw new ValidationApiException("Cannot change the active claster");
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_cluster.UpdateClusterStatsus(request);
		}

		public MariPriceApi.PortalPrice.Cluster.Info NewCluster(MariPriceApi.PortalPrice.Cluster.Create request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var versionInfo = m_apiEngine.GetCompanyInfoByVersion(request.VersionId);
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);
			if (versionInfo.ActiveVersionId == request.VersionId)
				throw new ValidationApiException("Cannot change the active claster");

			return m_cluster.Create(request);
		}

		public MariPriceApi.PortalPrice ClusterSwap(MariPriceApi.PortalPrice.PriceCompany.Swap request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			var versionInfo = m_apiEngine.GetCompanyPrice(request.CompanyId);
			var versionDetails = m_apiEngine.GetVersion(versionInfo.DraftVersionId);
			var clusters = m_apiEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForVersion(versionInfo.ActiveVersionId, versionInfo.DraftVersionId), true);

			var draftClasters = clusters.Where(x => x.VersionId == versionInfo.DraftVersionId)?.ToList() ?? new List<MariPriceApi.Price.Cluster>();
			var draftGroups = m_apiEngine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(draftClasters.Select(x => x.Id)?.ToArray()));

			ValidateGroups(versionDetails, draftGroups, draftClasters);

			List<Guid> toArchive = new List<Guid>();
			var activeClasters = clusters.Where(x => x.VersionId == versionInfo.ActiveVersionId)?.Select(x => x.Id)?.ToList() ?? new List<int>();
			if (activeClasters.Any())
			{
				var activeGroups = m_apiEngine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(activeClasters.ToArray()), true);
				if (activeGroups.Any())
				{
					var activeGroupIds = activeGroups.Select(x => x.Id).ToList();
					var draftGroupIds = draftGroups.Select(x => x.Id).ToList();

					var productReq = new MariPriceApi.Price.Product.List().ForPriceGroups(activeGroupIds.Union(draftGroupIds).ToArray());
					var products = m_apiEngine.GetProducts(productReq);

					var fromActive = products.Where(s => activeGroupIds.Contains(s.PriceGroupId.Value)).ToList();
					var fromDraft = products.Where(s => draftGroupIds.Contains(s.PriceGroupId.Value)).ToList();

					var activeNotInDraft = (from active in fromActive
											join draft in fromDraft on active.Id equals draft.Id into gj
											from subpet in gj.DefaultIfEmpty()
											select new { active.Id, active.ProductUid, subpet }).Where(x => x.subpet == null);

					if (activeNotInDraft.Any())
					{
						if (!request.Force)
							throw new ProductNotInDraftVersionApiException("There are products in active version which are not incude in draft");

						toArchive.AddRange(activeNotInDraft.Select(x => x.ProductUid).ToList());
					}
				}
			}

			m_cluster.Swap(request);

			if (toArchive.Any())
			{
				new SpecApi.Product.UpdateStatusAction()
				{
					Ids = toArchive,
					Action = SpecApi.Product.ProductAction.ToArchive
				}.Exec(m_spec);
			}

			var compPrice = m_apiEngine.GetCompanyPrice(request.CompanyId);
			return GetPriceByVersion(compPrice.ActiveVersionId);
		}

		private void ValidateGroups(MariPriceApi.Price.PriceCompanyVersion versionInfo,
			List<MariPriceApi.Price.Group> draftGroups,
			List<MariPriceApi.Price.Cluster> draftClasters)
		{
			if (versionInfo.WithNdsPriceRequired == false && versionInfo.WithoutNdsPriceRequired == false)
				throw new SaleSchemeException($"Для компании не выбрана ни одна схема продажи.", System.Net.HttpStatusCode.PreconditionFailed);

			if (!draftGroups.Any() && draftClasters.All(x => x.Enabled == true))
				throw new ValidationApiException($"Cluster is empty {versionInfo.VersionId}");

			var clasterhaveGroup = draftGroups.Select(x => x.ClusterId).ToList();
			foreach (var item in draftClasters.Where(x => x.Enabled == true))
			{
				if (!item.InOrder && !item.InStock)
					throw new ClusterSaleSchemeException($"В активном кластере не выбран тип продажи", System.Net.HttpStatusCode.PreconditionFailed);

				if (!(clasterhaveGroup.Contains(item.Id)))
					throw new ValidationApiException($"Cluster {item} is empty");
			}

			foreach (var item in draftGroups)
			{
				if (!item.Values.Any())
					throw new ValidationApiException($"Group has empty value (cluster {item.ClusterId}, Group {item.Id}");

				var currentCluster = draftClasters.FirstOrDefault(x => x.Id == item.ClusterId);

				if (currentCluster.InOrder)
					foreach (var value in item.Values)
					{
						if (versionInfo.WithNdsPriceRequired && versionInfo.WithoutNdsPriceRequired)
							if (!((value.WithNdsPrice ?? 0) > 0) && !((value.WithoutNdsPrice ?? 0) > 0))
								throw new PriceGroupNdsException("Не заполнены базовые стоимости с НДС и без НДС", System.Net.HttpStatusCode.PreconditionFailed);

						if (versionInfo.WithNdsPriceRequired)
							if (!((value.WithNdsPrice ?? 0) > 0))
								throw new PriceGroupWithNdsPriceException("Не заполнена базовая стоимость с НДС", System.Net.HttpStatusCode.PreconditionFailed);

						if (versionInfo.WithoutNdsPriceRequired)
							if (!((value.WithoutNdsPrice ?? 0) > 0))
								throw new PriceGroupWithOutNdsPriceException("Не заполнена базовая стоимость без НДС", System.Net.HttpStatusCode.PreconditionFailed);
					}

				if (currentCluster.InStock)
				{
					if (versionInfo.WithNdsPriceRequired && versionInfo.WithoutNdsPriceRequired)
						if (!((item.InStockValues?.WithNdsPrice ?? 0) > 0) && !((item.InStockValues?.WithoutNdsPrice ?? 0) > 0))
							throw new PriceGroupNdsInStockException("Не заполнены закупочные стоимости с НДС и без НДС", System.Net.HttpStatusCode.PreconditionFailed);

					if (versionInfo.WithNdsPriceRequired)
						if (!((item.InStockValues?.WithNdsPrice ?? 0) > 0))
							throw new PriceGroupWithNdsPriceInStockException("Не заполнена закупочная стоимость с НДС", System.Net.HttpStatusCode.PreconditionFailed);

					if (versionInfo.WithoutNdsPriceRequired)
						if (!((item.InStockValues?.WithoutNdsPrice ?? 0) > 0))
							throw new PriceGroupWithOutNdsInStockPriceException("Не заполнена закупочная стоимость без НДС", System.Net.HttpStatusCode.PreconditionFailed);

				}
			}
		}
		public MariPriceApi.PortalPrice.Cluster GetCluster(int companyId, int clusterId, UserInfo userInfo)
		{
			ValidateCompanyAccess(companyId, userInfo.UserId);
			return m_get.GetOneCluster(clusterId);
		}

		public List<MariPriceApi.EntityName<int>> GetGroupsForCompany(int companyId, string groupName, UserInfo userInfo)
		{
			ValidateCompanyAccess(companyId, userInfo.UserId);
			return m_get.GetGroupsShortInfo(new MariPriceApi.PortalPrice.Group.SasRequest.SasSource
			{
				CompanyIds = new List<int> { companyId },
				MaxCount = m_searchSettings.MaxPriceGroupsReturnItems,
			}, groupName);
		}

		public MariPriceApi.PortalPrice.Group GetGroupById(int groupId, UserInfo userInfo)
		{
			var versionInfo = m_apiEngine.GetCompanyInfoByGroupId(groupId);
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_get.GetGroup(groupId);
		}

		public List<MariPriceApi.EntityName<int>> GetGroupsBySas(MariPriceApi.PortalPrice.Group.SasRequest.SasSource sasRequest, string groupName)
		{
			return m_get.GetGroupsShortInfo(sasRequest, groupName);
		}

		public MariPriceApi.PortalPrice UpdateVersion(MariPriceApi.PortalPrice.PriceCompanyVersion.Update request, UserInfo userInfo)
		{
			var versionInfo = m_apiEngine.GetCompanyInfoByVersion(request.VersionId);
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			var update = new MariPriceApi.Price.PriceCompanyVersion.Update().CopyFrom(request, m_dto);
			m_apiEngine.UpdateVersion(update);

			return GetPriceByVersion(request.VersionId);
		}

		public MariPriceApi.PortalPrice.Group NewGroup(MariPriceApi.PortalPrice.Group.Create request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var versionInfo = m_apiEngine.GetCompanyInfoByClusterId(request.ClusterId);
			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_group.Create(request);
		}

		public MariPriceApi.PortalPrice.Group UpdateGroup(MariPriceApi.PortalPrice.Group.Update request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var versionInfo = m_apiEngine.GetCompanyInfoByGroupId(request.Id);

			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_group.Update(request);
		}

		public MariPriceApi.PortalPrice.GroupValue UpdateGroupValue(MariPriceApi.PortalPrice.GroupValue.Update request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var originValueInfo = m_apiEngine.GetGroupsValues(new MariPriceApi.Price.GroupValue.List().ForIds(request.PriceGroupValueId)).FirstOrDefault();
			if (originValueInfo == null)
				throw new RecordNotFoundApiException($"Cannot found value with Id {request.PriceGroupValueId}");

			var versionInfo = m_apiEngine.GetCompanyInfoByGroupId(originValueInfo.PriceGroupId);

			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			return m_group.UpdateGroupVaue(request);
		}

		public MariPriceApi.PortalPrice.Cluster AddSettingsItem(MariPriceApi.PortalPrice.Cluster.Append request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var versionInfo = m_apiEngine.GetCompanyInfoByClusterId(request.ClusterId);

			ValidateCompanyAccess(versionInfo.CompanyId, userInfo.UserId);

			var originSettings = m_apiEngine.GetSetting(new MariPriceApi.Price.ClusterSetting.List().ForCluster(request.ClusterId));

			var dublicate = originSettings.FirstOrDefault(x => x.OrderMetalWeight == request.OrderMetalWeight);
			if (dublicate != null)
				throw new RecordDuplicateApiException("Cluster has the same variants");

			m_cluster.Append(request);

			return m_get.GetOneCluster(request.ClusterId);
		}

		public MariPriceApi.Price.ClusterSetting UpdateSettingsItem(MariPriceApi.PortalPrice.Cluster.UpdateItem request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			var originCluster = m_apiEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(request.ClusterId)).SingleOrDefault();

			// TO DO
			//CheckAccessToCluster(userInfo, originCluster);

			return m_cluster.UpdateItem(request, originCluster);
		}

		public MariPriceApi.PortalPrice.Cluster.Info DeleteSettingsItem(MariPriceApi.PortalPrice.Cluster.DeleteItem request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			var originCluster = m_apiEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(request.ClusterId)).SingleOrDefault();
			// TO DO
			//CheckAccessToCluster(userInfo, originCluster);

			var groups = m_apiEngine.GetGroups(new MariPriceApi.Price.Group.List().ForClusters(originCluster.Id));

			foreach (var item in groups)
			{
				var needToRemove = item.Values.Where(x => x.PriceClusterVariantId == request.SettingsVariantId).FirstOrDefault();

				if (needToRemove != null)
					m_apiEngine.DeleteGroupValue(new MariPriceApi.Price.GroupValue.Delete() { PriceGroupValueId = needToRemove.PriceGroupValueId });
			}

			var mergePricesRequest = new MariPriceApi.Price.Group.SetPrices()
			{
				Groups = groups.ToDictionary(x => x.Id, x => x.Values.Where(v => v.PriceGroupValueId != request.SettingsVariantId).ToList())
			};

			var clusterInfo = m_cluster.DeleteItem(request, originCluster);

			return clusterInfo;
		}

		public MariPriceApi.PortalPrice BulkSetPrice(MariPriceApi.PortalPrice.BulkSet request, UserInfo userInfo)
		{
			// TO DO
			/*request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			m_bulkSetEngine.Exec(request, userInfo);

			return m_get.GetSummary(request.CompanyId);*/
			return new MariPriceApi.PortalPrice();
		}

		public void DeleteGroupLink1(MariPriceApi.PortalPrice.Product.Delete request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			m_group.DeleteProductLink1(request);
		}

		public void CreateGroupLink1(MariPriceApi.PortalPrice.Product.Create request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			m_group.CreateProductLink1(request);
		}

		public MariPriceApi.PortalPrice.Product GetProductNamesForLinks(MariPriceApi.PortalPrice.Product.List request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());
			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_group.GetProductNames(request);
		}

		private ProfileApi.Company ValidateCompanyAccess(int companyId, int userId)
		{
			var reader = new PlayersReader.Request().AddCompanies(companyId).AddUsers(userId)
							.Load(m_profile);

			var company = reader.GetCompany(companyId);
			if (company == null)
				throw new RecordNotFoundApiException($"There is no such company with id '{companyId}'");
			var user = reader.GetUser(userId);
			if (!(user.Roles.Contains(UserRoles.MariManager) ||
				user.Roles.Contains(UserRoles.ContractAdmin) && company.ContractId == user.ContractId))
				throw new ForbiddenApiException("You have no access to the company");
			return company;
		}

		public MariPriceApi.PortalPrice.TechnologiesAdditions GetTechnologiesPricesActive(int companyId, UserInfo userInfo)
		{
			ValidateCompanyAccess(companyId, userInfo.UserId);
			return m_technologies.Get(companyId);
		}

		public MariPriceApi.PortalPrice.TechnologiesAdditions GetTechnologiesPricesDraft(int companyId, UserInfo userInfo)
		{
			ValidateCompanyAccess(companyId, userInfo.UserId);
			return m_technologies.Get(companyId);
		}


		public MariPriceApi.Price.Technologies.TechnologyPortal UpdateTechnologyPrices(MariPriceApi.PortalPrice.TechnologiesAdditions.Update request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			// TO Do
			//ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_technologies.Update(request);
		}

		public MariPriceApi.Price.Technologies.TechnologyPortal AppendTechnologyPrices(MariPriceApi.PortalPrice.TechnologiesAdditions.Create request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			// TO DO
			//ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_technologies.Create(request);
		}

		public MariPriceApi.PortalPrice.TechnologiesAdditions DeleteTechnologyPrices(MariPriceApi.PortalPrice.TechnologiesAdditions.Delete request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			//TO DO
			//ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_technologies.Delete(request);
		}

		public MariPriceApi.PortalPrice.Group.Import.Response ImportPriceGroup(MariPriceApi.PortalPrice.Group.Import request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_groupImport.Exec(request);
		}

		public MariPriceApi.PortalPrice.Group.Export.Response ExportPriceGroup(MariPriceApi.PortalPrice.Group.Export request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_groupExport.Exec(request);
		}

		public MariPriceApi.PortalPrice.Product.Import.Response ImportPriceProduct(MariPriceApi.PortalPrice.Product.Import request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_productImport.Exec(request);
		}

		public MariPriceApi.PortalPrice.Product.Export.Response ExportPriceProduct(MariPriceApi.PortalPrice.Product.Export request, UserInfo userInfo)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			ValidateCompanyAccess(request.CompanyId, userInfo.UserId);

			return m_productExport.Exec(request);
		}
	}
}
