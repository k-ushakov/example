using d7k.Dto;
using d7k.Dto.Utilities;
using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.Description.Client;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.Print.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class PriceEngine
	{
		PriceTechnologiesEngine m_technologies;
		PriceGroupEngine m_group;
		PriceClusterEngine m_cluster;
		PriceProductEngine m_product;
		PriceInstockEngine m_instock;

		ISpecEngine m_specEngine;
		SpecSettings m_specSettings;
		DescriptionApiClient m_descriptionApi;
		PrintApiClient m_print;

		ISqlFactory m_sql;
		DtoComplex m_dto;

		public PriceEngine(ISqlFactory sql, DtoComplex dto, ISpecEngine specEngine, SpecSettings specSettings, DescriptionApiClient descriptionApi, PrintApiClient print)
		{
			m_cluster = new PriceClusterEngine(sql, dto, specEngine, specSettings);
			m_group = new PriceGroupEngine(sql, dto, m_cluster);
			m_technologies = new PriceTechnologiesEngine(sql, dto, specEngine, specSettings);
			m_product = new PriceProductEngine(sql, dto, descriptionApi, this);
			m_instock = new PriceInstockEngine(sql, print, dto, this);

			m_specEngine = specEngine;
			m_specSettings = specSettings;

			m_sql = sql;
			m_dto = dto;

			m_descriptionApi = descriptionApi;
			m_print = print;
		}

		public void UpdateVersion(MariPriceApi.Price.PriceCompanyVersion.Update request)
		{
			var update = new MariPriceDb.Price.PriceVersion.Update().CopyFrom(request, m_dto);
			update.UpdationList = new string[] { nameof(update.WithNdsPriceRequired), nameof(update.WithoutNdsPriceRequired) };
			update.Exec(m_sql);
		}

		public void LoadActiveVersion(int companyId)
		{
			var dbLoadActive = new MariPriceDb.Price.Company.LoadActive() { CompanyId = companyId };
			dbLoadActive.Exec(m_sql);
		}

		public MariPriceApi.Price.PriceCompany GetCompanyPrice(int companyId)
		{
			var priceCompany = new MariPriceDb.Price.Company.List(companyId).Exec(m_sql)?.FirstOrDefault();

			if (priceCompany != null)
				return new MariPriceApi.Price.PriceCompany().CopyFrom(priceCompany, m_dto);
			else
			{
				// need to create
				using (var trans = m_sql.Transaction())
				{
					var versionCreate = new MariPriceDb.Price.PriceVersion.Create() { WithNdsPriceRequired = false, WithoutNdsPriceRequired = false };
					var activeId = versionCreate.Exec(trans);
					var draftId = versionCreate.Exec(trans);

					var create = new MariPriceDb.Price.Company.Create() { CompanyId = companyId, ActiveVersionId = activeId, DraftVersionId = draftId };
					create.Exec(trans);

					trans.Commit();

					return new MariPriceApi.Price.PriceCompany().CopyFrom(create, m_dto);
				}
			}
		}

		public List<MariPriceApi.Price> GetProductPriceContent(MariPriceApi.Price.Product.PriceContent.List request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var result = new List<MariPriceApi.Price>();

			var priceRequest = new MariPriceApi.Price.AggregatePrice.Request { ProductUids = request.ProductIds };
			var newPriceContents = GetAggregatePriceInfoForProducts(priceRequest).Items;

			if (newPriceContents?.Any() == true)
			{
				/*var techsNames = m_vocValue.Get(new SpecApi.VocValue.List.Request().ForVoc(m_vocSettings.Ids.Technologies))
					.Items.ToDictionary(x => x.Id, x => x.Value);*/

				foreach (var productId in request.ProductIds)
				{
					var priceContentFromMari = newPriceContents.Get(productId);

					if (priceContentFromMari == null)
						continue;

					var priceContent = new MariPriceApi.Price
					{
						ProductId = productId,
						TechnologiesAdditionalPrices = priceContentFromMari.TechnologiesAdditionalPrices.ToDictionary(
								x => x.Key,
								x => new MariPriceApi.Price.Technologies.TechnologyApi
								{
									WithNdsPrice = x.Value.WithNdsPrice,
									WithoutNdsPrice = x.Value.WithoutNdsPrice,
									TechnologyId = x.Value.TechnologyId
								}),
						MainManufacturingOptions = priceContentFromMari.MainManufacturingOptions,
						SizeOwnManufacturingOptions = priceContentFromMari.SizeOwnManufacturingOptions.ToDictionary(
								x => x.Key,
								x => x.Value?.ToList()),
						PriceCluster = new MariPriceApi.EntityName<int>()
						{
							Id = priceContentFromMari.PriceCluster.Id,
							Name = priceContentFromMari.PriceCluster.Name
						}
					};

					result.Add(priceContent);
				}
			}

			return result;
		}

		public MariPriceApi.Price.PriceCompany CreateCompanyVersion(MariPriceApi.Price.PriceCompany.Create request)
		{
			using (var trans = m_sql.Transaction())
			{
				var versionCreate = new MariPriceDb.Price.PriceVersion.Create() { WithNdsPriceRequired = false, WithoutNdsPriceRequired = false };
				var activeId = versionCreate.Exec(trans);
				var draftId = versionCreate.Exec(trans);

				var create = new MariPriceDb.Price.Company.Create() { CompanyId = request.CompanyId, ActiveVersionId = activeId, DraftVersionId = draftId };
				create.Exec(trans);

				trans.Commit();

				return new MariPriceApi.Price.PriceCompany().CopyFrom(create, m_dto);
			}
		}

		public MariPriceApi.Price.PriceCompanyVersion GetVersion(int versionId)
		{
			var version = new MariPriceDb.Price.PriceVersion.List(versionId).Exec(m_sql)?.FirstOrDefault();

			if (version == null)
				throw new RecordNotFoundApiException($"Cannot find version with versionId {versionId}");

			return new MariPriceApi.Price.PriceCompanyVersion().CopyFrom(version, m_dto);
		}

		public List<MariPriceApi.Price.PriceCompanyVersion> GetVersions(int[] versionIds)
		{
			var versions = new MariPriceDb.Price.PriceVersion.List() { VersionIds = versionIds }
			.Exec(m_sql);

			if (!versions.Any())
				throw new RecordNotFoundApiException($"Cannot find version with versionId");

			return versions.Select(x => new MariPriceApi.Price.PriceCompanyVersion().CopyFrom(x, m_dto)).ToList();
		}

		public MariPriceApi.Price.PriceCompany GetCompanyInfoByVersion(int versionId)
		{
			var version = new MariPriceDb.Price.Company.ListByVersion(versionId).Exec(m_sql)?.FirstOrDefault();

			if (version == null)
				throw new RecordNotFoundApiException($"Cannot find version with versionId {versionId}");

			return new MariPriceApi.Price.PriceCompany().CopyFrom(version, m_dto);
		}

		public MariPriceApi.Price.PriceCompany GetCompanyInfoByClusterId(int clasterId)
		{
			var cluster = new MariPriceDb.Price.Cluster.List().ForClusterIds(clasterId).Exec(m_sql)?.FirstOrDefault();

			if (cluster == null)
				throw new RecordNotFoundApiException($"Cannot find claster {clasterId}");

			return GetCompanyInfoByVersion(cluster.VersionId);
		}

		public MariPriceApi.Price.PriceCompany GetCompanyInfoByGroupId(int groupId)
		{
			var group = new MariPriceDb.Price.Group.List().ForIds(groupId).Exec(m_sql)?.FirstOrDefault();

			if (group == null)
				throw new RecordNotFoundApiException($"Cannot find group {groupId}");

			return GetCompanyInfoByClusterId(group.ClusterId);
		}

		public List<MariPriceApi.Price.Cluster> GetCluster(MariPriceApi.Price.Cluster.List request, bool withoutSettings = false)
		{
			return withoutSettings ? m_cluster.GetOnlyClusterInfo(request) : m_cluster.Get(request);
		}

		public int CreateCluster(MariPriceApi.Price.Cluster.Create request)
		{
			return m_cluster.Create(request);
		}

		public void UpdateProdutStatus(MariPriceApi.Price.Product.UpdateStatus request)
		{
			m_product.UpdateStatus(request);
		}

		public void UpdateCluster(MariPriceApi.Price.Cluster.Update request)
		{
			m_cluster.Update(request);
		}

		public List<MariPriceApi.Price.ClusterSetting> GetSetting(MariPriceApi.Price.ClusterSetting.List request)
		{
			return m_cluster.GetSetting(request);
		}

		public void UpdateSetting(MariPriceApi.Price.ClusterSetting.Update request)
		{
			m_cluster.UpdateSetting(request);
		}

		public void DeleteSetting(MariPriceApi.Price.ClusterSetting.Delete request)
		{
			m_cluster.DeleteSetting(request);
		}

		public void DeleteGroup(int priceGroupId)
		{
			m_group.Delete(priceGroupId);
		}

		public int CreateSetting(MariPriceApi.Price.ClusterSetting.Create request)
		{
			return m_cluster.CreateSetting(request);
		}

		public List<MariPriceApi.Price.Group> GetGroups(MariPriceApi.Price.Group.List request, bool withoutValues = false)
		{
			var accum = m_group.Get(request, withoutValues);

			if (accum.Any())
			{
				var products = m_product.Get(new MariPriceApi.Price.Product.List().ForPriceGroups(accum.Select(x => x.Id).ToArray()));

				foreach (var current in accum)
				{
					if (products.Any())
					{
						var currentProducts = products.Where(x => x.PriceGroupId.Value == current.Id)?.ToList() ?? new List<MariPriceApi.Price.Product>();

						var productGroupBy = currentProducts?.GroupBy(x => x.ProductUid)?.Select(x => new { ProductUid = x.Key, Status = x.Max(s => s.Status) })?.ToList();

						var allProduct = productGroupBy?.Count ?? 0;
						var publish = productGroupBy?.Where(x => x.Status == Spec.Dto.ProductState.Active)?.Count() ?? 0;

						current.ProductPublishInfo = $"Опубликовано {publish} из {allProduct}";
					}
				}
			}

			return accum;
		}

		public List<MariPriceApi.Price.GroupWithVersion> GetGroupsWithVersion(MariPriceApi.Price.Group.List request, bool withoutValues = false)
		{
			var accum = m_group.GetWithVersion(request, withoutValues);

			if (accum.Any())
			{
				var products = m_product.Get(new MariPriceApi.Price.Product.List().ForPriceGroups(accum.Select(x => x.Id).ToArray()));

				if (products.Any())
				{
					foreach (var current in accum)
					{
						var currentProducts = products.Where(x => x.PriceGroupId.Value == current.Id)?.ToList() ?? new List<MariPriceApi.Price.Product>();

						var productGroupBy = currentProducts?.GroupBy(x => x.ProductUid)?.Select(x => new { ProductUid = x.Key, Status = x.Max(s => s.Status) })?.ToList();

						var allProduct = productGroupBy?.Count ?? 0;
						var publish = productGroupBy?.Where(x => x.Status == Spec.Dto.ProductState.Active)?.Count() ?? 0;

						current.ProductPublishInfo = $"Опубликовано {publish} из {allProduct}";
					}
				}
			}

			return accum;
		}

		public void DeleteTechnology(int verstionId, Guid technologyId)
		{
			m_technologies.Delete(verstionId, technologyId);
		}

		public void DeleteCluster(int clusterId)
		{
			m_cluster.Delete(clusterId);
		}

		public int CreateGroup(MariPriceApi.Price.Group.Create request)
		{
			return m_group.Create(request);
		}

		public void UpdateGroup(MariPriceApi.Price.Group.Update request)
		{
			m_group.Update(request);
		}

		public List<MariPriceApi.Price.GroupValue> GetGroupsValues(MariPriceApi.Price.GroupValue.List request)
		{
			return m_group.GetValues(request);
		}

		public int CreateGroupValue(MariPriceApi.Price.GroupValue.Create request)
		{
			return m_group.CreateValue(request);
		}

		public void UpdateGroupValue(MariPriceApi.Price.GroupValue.Update request)
		{
			m_group.UpdateValue(request);
		}

		public void DeleteGroupValue(MariPriceApi.Price.GroupValue.Delete request)
		{
			m_group.DeleteVaue(request);
		}

		public List<MariPriceApi.Price.Technologies> GetTechnologyPrices(MariPriceApi.Price.Technologies.List request)
		{
			if (request.VersionIds.Any())
				return m_technologies.Get(new MariPriceApi.Price.Technologies.List().ForVersions(request.VersionIds.ToArray()));

			else return new List<MariPriceApi.Price.Technologies>();
		}

		public MariPriceApi.Price.Technologies CreateTechnologiesPrice(MariPriceApi.Price.Technologies.Create request)
		{
			m_technologies.Create(request);

			return m_technologies.Get(new MariPriceApi.Price.Technologies.List().ForVersions(request.VersionId)).FirstOrDefault();
		}

		public MariPriceApi.Price.Technologies SetTechnologiesPrice(MariPriceApi.Price.Technologies.Set request)
		{
			m_technologies.Set(request);

			return m_technologies.Get(new MariPriceApi.Price.Technologies.List().ForVersions(request.VersionId)).FirstOrDefault();
		}

		public void ClusterSwap(MariPriceApi.Price.PriceCompany.Swap apiRequest)
		{
			m_cluster.CusterSwap(apiRequest);
		}

		public void UpdateCompany(MariPriceApi.Price.CompanySettingsUpdate request)
		{
			//m_cluster.UpdateCompany(request);
		}

		public void SetPrices(MariPriceApi.Price.Group.SetPrices request)
		{
			m_group.SetPrices(request);
		}

		public void SyncProducts(MariPriceApi.Price.Product.Sync request)
		{
			m_product.Sync(request);
		}

		public void UpdateProdustsDisplayName(Guid[] ProductUids)
		{
			var products = new MariPriceDb.Price.Product.List().ForProducts(ProductUids).Exec(m_sql).ToList();
			foreach (var productItem in products)
			{
				if (productItem.PriceGroupId != null)
					UpdateDisplayNamePriceGroup((int)productItem.PriceGroupId);
			}
		}

		public List<MariPriceApi.Price.Product> GetOnlyProductsWithoutAdditionalInfo(MariPriceApi.Price.Product.List request)
		{
			return m_product.GetWithoutAdditionalInfo(request);
		}

		public List<MariPriceApi.Price.Product> GetProducts(MariPriceApi.Price.Product.List request)
		{
			return m_product.Get(request);
		}

		public List<MariPriceApi.Price.Product> GetProductsActiveCluster(MariPriceApi.Price.Product.List request, bool onlyActiveCluster)
		{
			return m_product.GetOnlyInActiveCluster(request, onlyActiveCluster);
		}

		public void SetLinks(MariPriceApi.Price.Product.Link request)
		{
			m_product.SetLinks(request);
		}

		public void SetBarcodes(MariPriceApi.Price.Instock.Import request)
		{
			m_instock.SetBarcodes(request);
		}

		public MariPriceApi.Price.Instock.Export.Result ExportBarcodes(MariPriceApi.Price.Instock.Export request, UserInfo user)
		{
			return m_instock.ExportBarcode(request, user);
		}

		public List<MariPriceApi.Price.Instock> GetBarcodes(MariPriceApi.Price.Instock.List request)
		{
			return m_instock.GetBarcodes(request);
		}

		public MariPriceApi.Price.AggregatePrice GetAggregatePriceInfoForProducts(MariPriceApi.Price.AggregatePrice.Request request)
		{
			var result = new MariPriceApi.Price.AggregatePrice();
			if (request.ProductUids == null || !request.ProductUids.Any())
				return result;

			var productsWithGroups = GetProductsActiveCluster(new MariPriceApi.Price.Product.List().ForProducts(request.ProductUids.ToArray()), true).ToDictionary(x => x.Id);
			var products = GetOnlyProductsWithoutAdditionalInfo(new MariPriceApi.Price.Product.List().ForProducts(request.ProductUids.ToArray()));
			var mainPriceGroupIds = products.Where(x => x.SizeUid == Guid.Empty && productsWithGroups.Get(x.Id)?.PriceGroupId != null)
				.ToDictionary(x => x.ProductUid, x => productsWithGroups.Get(x.Id).PriceGroupId.Value);

			foreach (var product in products)
			{
				product.PriceGroupId = productsWithGroups.Get(product.Id)?.PriceGroupId;
			}

			var sizes = products.Where(x => x.SizeUid != Guid.Empty)
				.GroupBy(x => x.ProductUid)
				.ToDictionary(x => x.Key, x => x.ToList());

			var priceGroupIds = productsWithGroups.Values.Where(x => x.PriceGroupId.HasValue).Select(x => x.PriceGroupId.Value).Distinct().ToArray();
			var priceGroups = priceGroupIds.Any() ?
					GetGroups(new MariPriceApi.Price.Group.List().ForIds(priceGroupIds)).ToDictionary(x => x.Id) :
					new Dictionary<int, MariPriceApi.Price.Group>();

			var mainPriceGroups = mainPriceGroupIds.ToDictionary(x => x.Key, x => priceGroups.Get(x.Value));
			var priceClusterIds = priceGroups.Select(x => x.Value.ClusterId).Distinct().ToArray();
			var priceClusters = priceClusterIds.Any() ?
				GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(priceClusterIds)).ToDictionary(x => x.Id) :
				new Dictionary<int, MariPriceApi.Price.Cluster>();

			var clusterNameIds = priceClusters.Select(x => x.Value.Name).Distinct().ToArray();
			var versionIds = priceClusters.Select(x => x.Value.VersionId).Distinct().ToArray();
			var clusterNames = clusterNameIds.Any() ?
				m_specEngine.GetVocValues(m_specSettings.PriceClusters, clusterNameIds).Items.ToDictionary(x => x.Id, x => x.Value) :
				new Dictionary<Guid, string>();

			var versionInfo = versionIds.Any() ? GetVersions(versionIds).ToDictionary(x => x.VersionId) :
				new Dictionary<int, MariPriceApi.Price.PriceCompanyVersion>();

			var companyIds = products.Select(x => x.CompanyId).Distinct().ToArray();
			var technologies = versionIds.Any() ?
				GetTechnologyPrices(new MariPriceApi.Price.Technologies.List().ForVersions(versionIds)) :
				new List<MariPriceApi.Price.Technologies>();

			var instockBarcodes = GetBarcodes(new MariPriceApi.Price.Instock.List().ForCompanies(companyIds));
			var withoutSizesBarcodes = instockBarcodes.Where(x => x.SizeUid == Guid.Empty).GroupBy(x => x.ProductUid)
				.ToDictionary(x => x.Key, x => x.ToList());
			var sizeBarcodes = instockBarcodes.Where(x => x.SizeUid != Guid.Empty).GroupBy(x => x.ProductUid)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach (var productUid in request.ProductUids)
			{
				var mainGroup = mainPriceGroups.Get(productUid);
				var productSizes = sizes.Get(productUid);
				var withoutSizeInstockInfo = withoutSizesBarcodes.Get(productUid);
				var sizeInstockInfo = sizeBarcodes.Get(productUid);
				var cluster = mainGroup == null ? null : priceClusters.Get(mainGroup.ClusterId);
				// TO DO
				var priceContent = GetAggregatePriceInfo(priceGroups, mainGroup, productSizes, cluster, technologies, versionInfo, clusterNames, withoutSizeInstockInfo, sizeInstockInfo, priceClusters);
				priceContent.ProductId = productUid;
				result.Items[productUid] = priceContent;

			}

			return result;
		}

		public void RemoveFromProductLinkByGroup(int groupId)
		{
			m_product.RemoveFromProductLinkByGroup(groupId);
		}

		public void UpdateDisplayNamePriceGroup(int id)
		{
			var priceGroup = GetGroups(new MariPriceApi.Price.Group.List().ForIds(id)).FirstOrDefault();

			if (string.IsNullOrWhiteSpace(priceGroup?.Name))
			{
				var apiRequest = new MariPriceApi.Price.Group.Update()
				{
					Id = id,
					DisplayName = GenerateDisplayName(priceGroup?.Name, id)
				};

				apiRequest.UpdationList = new[]
				{nameof(apiRequest.DisplayName)};

				UpdateGroup(apiRequest);
			}
		}

		public string GenerateDisplayName(string nameGroup, int id_priceGroup)
		{
			string result = nameGroup;

			if (String.IsNullOrWhiteSpace(nameGroup))
			{
				var companyInfo = GetCompanyInfoByGroupId(id_priceGroup);
				Dictionary<Guid, string> acuumproducts = new Dictionary<Guid, string>();
				Dictionary<Guid, List<string>> acuumsize = new Dictionary<Guid, List<string>>();
				var products = GetProducts(new MariPriceApi.Price.Product.List()
					.ForPriceGroups(id_priceGroup))
					.Where(x => x.Enabled == true).ToList();
				foreach (var product in products)
				{
					if (product.SizeUid == Guid.Empty)
					{
						// Все активные размеры
						var items = new MariPriceDb.Price.Product.ListProductSize()
							.ForProduct(product.ProductUid)
							.ForVersion(companyInfo.DraftVersionId)
							.Exec(m_sql)
							.Where(x => x.Enabled == true && (x.PriceGroupId == null || x.PriceGroupId == id_priceGroup) && x.SizeUid != Guid.Empty)
							.ToList();

						if (items.Count == 0)
						{
							acuumproducts[product.ProductUid] = product.SizePn;
						}
						else
						{
							if (!acuumsize.ContainsKey(product.ProductUid))
								acuumsize[product.ProductUid] = new List<string>();
							foreach (var itm in items)
							{
								acuumsize[product.ProductUid].Add(itm.SizePn);
							}
						}
					}
					else
					{
						if (!acuumsize.ContainsKey(product.ProductUid))
							acuumsize[product.ProductUid] = new List<string>();

						acuumsize[product.ProductUid].Add(product.SizePn);
					}
				}

				List<string> Pns = new List<string>();
				foreach (var item in acuumproducts)
				{
					if (!acuumsize.ContainsKey(item.Key))
						Pns.Add(item.Value);
					else
					{
						Pns.AddRange(acuumsize[item.Key]);
						acuumsize.Remove(item.Key);
					}
				}

				foreach (var item in acuumsize)
					Pns.AddRange(item.Value);

				if (Pns.Any())
					result = String.Join(", ", Pns.Distinct().Where(x=>!String.IsNullOrWhiteSpace(x)).ToArray());
			}

			return result;
		}



		public void RemoveFromProductLinkAndCluster(int clusterId, int groupId)
		{
			m_product.RemoveFromProductLinkAndCluster(clusterId, groupId);
		}

		// TO DO
		public MariPriceApi.Price GetAggregatePriceInfo(
			Dictionary<int, MariPriceApi.Price.Group> allGroups,
			MariPriceApi.Price.Group productPriceGroup,
			List<MariPriceApi.Price.Product> productSizes,
			MariPriceApi.Price.Cluster cluster,
			List<MariPriceApi.Price.Technologies> technologies,
			Dictionary<int, MariPriceApi.Price.PriceCompanyVersion> versionInfo,
			Dictionary<Guid, string> clusterNames,
			List<MariPriceApi.Price.Instock> withoutSizeInstockInfo,
			List<MariPriceApi.Price.Instock> sizeInstockInfo,
			Dictionary<int, MariPriceApi.Price.Cluster> priceClusters)
		{
			productSizes = productSizes ?? new List<MariPriceApi.Price.Product>();

			var result = new MariPriceApi.Price();

			if (cluster == null)
			{
				var firstSizeWithGroup = productSizes.FirstOrDefault(x => x.PriceGroupId.HasValue);
				if (firstSizeWithGroup == null ||
					!(allGroups.TryGetValue(firstSizeWithGroup.PriceGroupId.Value, out var sizeGroup) &&
					priceClusters.TryGetValue(sizeGroup.ClusterId, out cluster)))
				{
					return result;
				}
			}

			technologies = technologies.Where(x => x.VersionId == cluster.VersionId)?.ToList() ?? new List<MariPriceApi.Price.Technologies>();

			if (technologies?.Any() == true)
				result.TechnologiesAdditionalPrices = technologies.ToDictionary(x => x.TechnologyId, y => new MariPriceApi.Price.Technologies.TechnologyApi()
				{
					TechnologyId = y.TechnologyId,
					WithNdsPrice = y.WithNdsPrice,
					WithoutNdsPrice = y.WithoutNdsPrice
				});

			result.PriceCluster = new MariPriceApi.EntityName<int>
			{
				Id = cluster.Id,
				Name = clusterNames.Get(cluster.Name)
			};

			var version = versionInfo.Get(cluster.VersionId);

			result.MainManufacturingOptions = ExtractOptionsFromGroup(version, cluster, productPriceGroup).ToList();

			foreach (var size in productSizes)
			{
				if (size.PriceGroupId.HasValue && allGroups.TryGetValue(size.PriceGroupId.Value, out var sizeGroup))
				{
					result.SizeOwnManufacturingOptions[size.SizeUid] = ExtractOptionsFromGroup(version, cluster, sizeGroup).ToList();

					result.ProductInstockInfo.SizeInstockItems[size.ProductUid] = new MariPriceApi.Price.InStockItems()
					{
						WithNdsBuyPrice = sizeGroup?.InStockValues?.WithNdsPrice,
						WithNdsMarkup = sizeGroup?.InStockValues?.WithNdsMarkup,
						WithoutNdsBuyPrice = sizeGroup?.InStockValues?.WithoutNdsPrice,
						WithoutNdsMarkup = sizeGroup?.InStockValues?.WithoutNdsMarkup,
					};
				}
			}

			result.ProductInstockInfo = new MariPriceApi.Price.InstockInfo();
			result.ProductInstockInfo.MainInstockItems = BuildInstockItems(withoutSizeInstockInfo, productPriceGroup, version);


			if (sizeInstockInfo != null)
			{
				result.ProductInstockInfo.SizeInstockItems = new Dictionary<Guid, MariPriceApi.Price.InStockItems>();
				var barcodesGroupBySizes = sizeInstockInfo.GroupBy(x => x.SizeUid);
				foreach (var barcodes in barcodesGroupBySizes)
				{
					var sizeProduct = productSizes.FirstOrDefault(x => x.SizeUid == barcodes.Key);

					var currentGroup = productPriceGroup ?? new MariPriceApi.Price.Group();
					if (sizeProduct != null && sizeProduct.PriceGroupId.HasValue && allGroups.ContainsKey(sizeProduct.PriceGroupId.Value))
						currentGroup = allGroups[sizeProduct.PriceGroupId.Value];


					result.ProductInstockInfo.SizeInstockItems[barcodes.Key] = BuildInstockItems(barcodes.ToList(), currentGroup, version);
				}
			}

			return result;
		}

		private MariPriceApi.Price.InStockItems BuildInstockItems(List<MariPriceApi.Price.Instock> src, MariPriceApi.Price.Group group, MariPriceApi.Price.PriceCompanyVersion version)
		{
			var barcodes = src?.Select(x => x.Barcode)?.ToList() ?? new List<string>();
			var instockProducts = src?.Select(x => x.GetInfo())?.ToList() ?? new List<MariPriceApi.Price.InstockShort>();
			return new MariPriceApi.Price.InStockItems
			{
				Products = instockProducts,
				Barcodes = barcodes,
				Total = barcodes.Count,
				WithNdsBuyPrice = version.WithNdsPriceRequired ? group?.InStockValues?.WithNdsPrice : null,
				WithNdsMarkup = group?.InStockValues?.WithNdsMarkup,
				WithoutNdsBuyPrice = version.WithoutNdsPriceRequired ? group?.InStockValues?.WithoutNdsPrice : null,
				WithoutNdsMarkup = group?.InStockValues?.WithoutNdsMarkup
			};
		}

		private IEnumerable<MariPriceApi.Price.ManufacturingOption> ExtractOptionsFromGroup(
MariPriceApi.Price.PriceCompanyVersion version, MariPriceApi.Price.Cluster cluster, MariPriceApi.Price.Group group)
		{
			foreach (var variant in cluster.VariantsSettings)
			{
				var groupValue = group?.Values?.SingleOrDefault(x => x.PriceClusterVariantId == variant.Id);
				if (groupValue == null)
					continue;

				if (version.WithNdsPriceRequired)
					yield return new MariPriceApi.Price.ManufacturingOption
					{
						CostForGramm = groupValue.WithNdsPrice,
						LossPercentage = group.LossPercentage,
						AffinageLossPercentage = group.AdditionalLossPercentage,
						TotalLossPercentage = group.TotalLossPercentage,
						OrderMetalWeight = variant.OrderMetalWeight,
						ProductionTime = variant.ProductionTime,
						CostForGrammWithMarkup = groupValue.WithNdsMarkup,
						WithNds = true
					};
				if (version.WithoutNdsPriceRequired)
					yield return new MariPriceApi.Price.ManufacturingOption
					{
						CostForGramm = groupValue.WithoutNdsPrice,
						LossPercentage = group.LossPercentage,
						AffinageLossPercentage = group.AdditionalLossPercentage,
						TotalLossPercentage = group.TotalLossPercentage,
						OrderMetalWeight = variant.OrderMetalWeight,
						ProductionTime = variant.ProductionTime,
						CostForGrammWithMarkup = groupValue.WithoutNdsMarkup,
						WithNds = false
					};
			}
		}
	}
}
