using d7k.Dto;
using Mr.Avalon.Description.Client;
using Mr.Avalon.Description.Dto;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core.Other;
using Mr.Avalon.Spec.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public class PriceProductEngine
	{
		private ISqlFactory m_sql;
		private DtoComplex m_dto;
		DescriptionApiClient m_descriptionApi;
		private PriceEngine m_engine;

		public PriceProductEngine(ISqlFactory sql, DtoComplex dto, Description.Client.DescriptionApiClient descriptionApi, PriceEngine priceEngine)
		{
			m_sql = sql;
			m_dto = dto;
			m_descriptionApi = descriptionApi;
			m_engine = priceEngine;
		}

		public void Sync(MariPriceApi.Price.Product.Sync request)
		{
			// need title status companyId pn metal  imageurl
			ProductSync cachedDescriptionResult = null;
			ProductDataJson pdj = new ProductDataJson();
			if (request.Products.Count == 1)
			{
				cachedDescriptionResult = request.Products.First().DescriptionProductSyncInfo;
				if (request.Products.First().Technologys != null)
					pdj.TechnologyUids = JsonConvert.SerializeObject(request.Products.First().Technologys);
			}

			var descriptionRequest = new ProductDescription();
			descriptionRequest.Products = request.Products.Select(x => new ProductDescription.Item()
			{
				Id = x.ProductUid,
				Type = Description.Dto.DescriptionType.ProductSync
			}).ToList();
			descriptionRequest.SessionExpired = DateTime.UtcNow.AddYears(10);

			var items = new List<MariPriceDb.Price.Product.Merge.Item>();
			// TODO нужен рефакторинг

			if (cachedDescriptionResult != null)
			{
				var info = cachedDescriptionResult;//  item.Value.ProductSyncInfo;

				foreach (var item1 in info.Items)
				{
					var accum = new MariPriceDb.Price.Product.Merge.Item();

					accum.ProductUid = item1.ProductUid;
					accum.SizeUid = item1.SizeUid;

					accum.Title = item1.Title;
					accum.Pn = item1.ProductPn;
					accum.SizePn = item1.SizePn;
					accum.Metal = item1.Metal;

					accum.Size = item1.Size;
					accum.WireThickness = item1.WireThikness;

					switch (request.Action)
					{
						case ProductAction.Save:
							accum.Status = (Spec.Dto.ProductState)item1.Status;
							break;
						case ProductAction.Publish:
							accum.Status = Spec.Dto.ProductState.Active;
							break;
						case ProductAction.ToArchive:
							accum.Status = Spec.Dto.ProductState.Archive;
							break;
						default:
							accum.Status = (Spec.Dto.ProductState)item1.Status;
							break;
					}

					accum.ImageUrl = item1.ImageUrl;
					accum.DataJson = JsonConvert.SerializeObject(pdj);
					accum.CompanyId = item1.CompanyId;
					accum.Name = item1.Title;
					accum.Enabled = true;

					accum.SizeFullName = GetSizeFullName(item1.Size, item1.WireThikness);

					accum.SearchField = GetSearchField(item1, accum);

					items.Add(accum);
				}
			}
			else
			{
				var res = descriptionRequest.ExecApi(m_descriptionApi);
				request.Action = request.Action == null ? ProductAction.Save : request.Action;
				foreach (var item in res.Products)
				{
					var info = item.Value.ProductSyncInfo;
					foreach (var item1 in info.Items)
					{
						var accum = new MariPriceDb.Price.Product.Merge.Item();

						accum.ProductUid = item1.ProductUid;
						accum.SizeUid = item1.SizeUid;

						accum.Title = item1.Title;
						accum.Pn = item1.ProductPn;
						accum.SizePn = item1.SizePn;
						accum.Metal = item1.Metal;

						accum.Size = item1.Size;
						accum.WireThickness = item1.WireThikness;

						switch (request.Action)
						{
							case ProductAction.Save:
								accum.Status = (Spec.Dto.ProductState)item1.Status;
								break;
							case ProductAction.Publish:
								accum.Status = Spec.Dto.ProductState.Active;
								break;
							case ProductAction.ToArchive:
								accum.Status = Spec.Dto.ProductState.Archive;
								break;
							default:
								accum.Status = (Spec.Dto.ProductState)item1.Status;
								break;
						}

						accum.ImageUrl = item1.ImageUrl;
						accum.DataJson = JsonConvert.SerializeObject(pdj);
						accum.CompanyId = item1.CompanyId;
						accum.Name = item1.Title;
						accum.Enabled = true;
						accum.SearchField = GetSearchField(item1, accum);
						accum.SizeFullName = GetSizeFullName(item1.Size, item1.WireThikness);

						items.Add(accum);
					}
				}
			}

			var mergeDb = new MariPriceDb.Price.Product.Merge() { Products = items.ToArray() };
			mergeDb.Exec(m_sql);
			m_engine.UpdateProdustsDisplayName(items.Select(x => x.ProductUid).ToArray());
		}

		private decimal? ToDecimal(string size)
		{
			decimal result;
			if (decimal.TryParse(size, out result))
				return result;
			return (decimal?)null;
		}

		public static string GetSizeFullName(string sizeName, decimal? wireThikness)
		{
			var wireThiknessKey = wireThikness > 0 ? wireThikness?.ToString("0.00") : "";
			return $"{sizeName}-*-{wireThiknessKey}";
		}

		public string GetSearchField(ProductSync.Item item1, MariPriceDb.Price.Product.Merge.Item accum)
		{
			var res = new List<string>();

			if (!string.IsNullOrWhiteSpace(item1.AllPns))
				res.Add(item1.AllPns);

			if (!string.IsNullOrWhiteSpace(accum.Title))
				res.Add(accum.Title);

			var productId = accum.ProductUid.ToString();
			if (!string.IsNullOrWhiteSpace(productId))
				res.Add(productId);

			return String.Join("#", res.ToArray());
		}

		public List<MariPriceApi.Price.Product> Get(MariPriceApi.Price.Product.List request)
		{
			var getDb = new MariPriceDb.Price.Product.List()
				.ForIds(request.Ids?.ToArray())
				.ForProducts(request.ProductUids?.ToArray())
				.ForSizes(request.SizeUids?.ToArray())
				.ForPriceGroups(request.PriceGroupIds?.ToArray());

			var dbResult = getDb.Exec(m_sql);

			var result = dbResult.Select(x => new MariPriceApi.Price.Product().CopyFrom(x, m_dto)).ToList();

			return result;
		}

		public List<MariPriceApi.Price.Product> GetOnlyInActiveCluster(MariPriceApi.Price.Product.List request, bool onlyActiveCluster)
		{
			var getDb = new MariPriceDb.Price.Product.ListOnlyActive() { OnlyActiveCluster = onlyActiveCluster }
				.ForIds(request.Ids?.ToArray())
				.ForProducts(request.ProductUids?.ToArray())
				.ForSizes(request.SizeUids?.ToArray())
				.ForClusters(request.ClusterIds?.ToArray())
				.ForPriceGroups(request.PriceGroupIds?.ToArray())
				.ForPns(request.Pns?.ToArray())
				.ForCompanies(request.CompanyIds?.ToArray());

			var dbResult = getDb.Exec(m_sql);

			var result = dbResult.Select(x => new MariPriceApi.Price.Product().CopyFrom(x, m_dto)).ToList();

			return result;
		}

		public void RemoveFromProductLinkByGroup(int groupId)
		{
			var deleteDb = new MariPriceDb.Price.Product.GroupLinkDelete();
			deleteDb.PriceGroupId = groupId;

			deleteDb.Exec(m_sql);
		}
		public void RemoveFromProductLinkAndCluster(int clusterId, int groupId)
		{
			var deleteDb = new MariPriceDb.Price.Product.GroupLinkDelete();
			deleteDb.PriceGroupId = groupId;
			deleteDb.PriceCusterId = clusterId;
			deleteDb.DeleteUnusedClusterProducts = true;

			deleteDb.Exec(m_sql);
		}

		public void SetLinks(MariPriceApi.Price.Product.Link request)
		{
			if (request.ForClear != null)
			{
				var deleteDb = new MariPriceDb.Price.Product.GroupLinkDelete();
				deleteDb.ProductUidForClear = request.ForClear.ProductUid;
				deleteDb.ProductUid = request.ForClear.ProductUid;
				deleteDb.PriceCusterId = request.ForClear.PriceClusterId;
				deleteDb.DeleteClusterProduct = true;

				deleteDb.Exec(m_sql);
			}
			if (request.ForDelete != null)
			{
				var currentLinks = new MariPriceDb.Price.Product.GroupLinkList()
				{
					PriceGroupId = request.ForDelete.PriceGroupId,
					ProductId = request.ForDelete.ProductId
				}.Exec(m_sql);

				var deleteDb = new MariPriceDb.Price.Product.GroupLinkDelete();
				deleteDb.ProductIds = new List<int>() { request.ForDelete.ProductId };
				deleteDb.PriceGroupId = request.ForDelete.PriceGroupId;

				if (currentLinks.Any() && currentLinks.Count == 1)
				{
					deleteDb.ProductUid = currentLinks.First().ProductUid;
					deleteDb.PriceCusterId = currentLinks.First().PriceClusterId;
					deleteDb.DeleteClusterProduct = true;
				}

				deleteDb.Exec(m_sql);
			}
			if (request.ForSet != null)
			{
				var setDb = new MariPriceDb.Price.Product.GroupLinkSet();
				setDb.PriceGroupId = request.ForSet.PriceGroupId;
				setDb.ProductId = request.ForSet.ProductId;
				setDb.PriceClusterId = request.ForSet.PriceClusterId;
				setDb.VersionId = request.ForSet.VersionId;
				setDb.ProductUid = request.ForSet.ProductUid.ToString();

				setDb.Exec(m_sql);
			}
		}

		public void UpdateStatus(MariPriceApi.Price.Product.UpdateStatus request)
		{
			var update = new MariPriceDb.Price.Product.UpdateStatus();
			update.ProductUid = request.ProductUid.ToString();
			update.Status = request.Status;

			update.Exec(m_sql);
		}

		public List<MariPriceApi.Price.Product> GetWithoutAdditionalInfo(MariPriceApi.Price.Product.List request)
		{
			var getDb = new MariPriceDb.Price.Product.ListOnlyProducts()
				.ForCompanies(request.CompanyIds?.ToArray())
				.ForProducts(request.ProductUids?.ToArray());

			var dbResult = getDb.Exec(m_sql);

			var result = dbResult.Select(x => new MariPriceApi.Price.Product().CopyFrom(x, m_dto)).ToList();

			return result;
		}
	}
}
