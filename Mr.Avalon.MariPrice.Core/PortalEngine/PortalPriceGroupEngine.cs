using d7k.Dto;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{

	public class PortalPriceGroupEngine
	{
		private PriceEngine m_engine;
		private DtoComplex m_dto;
		private ISqlFactory m_sql;
		private SearchSettings m_searchSettings;
		private String m_defaultImageUrl;

		public PortalPriceGroupEngine(PriceEngine engine, DtoComplex dto, ISqlFactory sql, SearchSettings searchSettings, String defaultImageUrl)
		{
			m_engine = engine;
			m_dto = dto;
			m_sql = sql;
			m_searchSettings = searchSettings;
			m_defaultImageUrl = defaultImageUrl;
		}

		public MariPriceApi.PortalPrice.Group Create(MariPriceApi.PortalPrice.Group.Create request)
		{
			var apiRequest = new MariPriceApi.Price.Group.Create().CopyFrom(request, m_dto);

			var originCluster = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(request.ClusterId))?.FirstOrDefault();
			if (originCluster == null)
				throw new RecordNotFoundApiException($"Cannot found cluster {request.ClusterId}");

			apiRequest.DisplayName = apiRequest.Name;
			var groupId = m_engine.CreateGroup(apiRequest);

			foreach (var item in originCluster.VariantsSettings)
			{
				var createValue = new MariPriceApi.Price.GroupValue.Create()
				{
					PriceClusterVariantId = item.Id,
					PriceGroupId = groupId
				};
				m_engine.CreateGroupValue(createValue);
			}

			var createInstockValue = new MariPriceApi.Price.InstockGroupValue.Create() { PriceGroupId = groupId };
			m_engine.CreateInstockGroupValue(createInstockValue);

			var group = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(groupId)).FirstOrDefault();

			return new MariPriceApi.PortalPrice.Group().CopyFrom(group, m_dto);
		}



		public MariPriceApi.PortalPrice.Group Update(MariPriceApi.PortalPrice.Group.Update request, MariPriceApi.Price.Group origin = null)
		{
			var apiRequest = new MariPriceApi.Price.Group.Update().CopyFrom(request, m_dto);

			// need to check
			var originGroup = origin ?? m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(request.Id)).FirstOrDefault();
			if (originGroup == null)
				throw new RecordNotFoundApiException($"Cannot found the group {request.Id}");

			apiRequest.DisplayName = m_engine.GenerateDisplayName(apiRequest.Name, request.Id);
			apiRequest.UpdationList = new[]
			{
				nameof(apiRequest.Name),
				nameof(apiRequest.LossPercentage),
				nameof(apiRequest.AdditionalLossPercentage),
				nameof(apiRequest.DisplayName)
			};
			m_engine.UpdateGroup(apiRequest);

			return GetById(request.Id);
		}

		public MariPriceApi.PortalPrice.GroupValue UpdateGroupVaue(MariPriceApi.PortalPrice.GroupValue.Update request)
		{
			var apiRequest = new MariPriceApi.Price.GroupValue.Update().CopyFrom(request, m_dto);

			apiRequest.UpdationList = new[] { nameof(apiRequest.WithNdsPrice), nameof(apiRequest.WithoutNdsPrice) };

			m_engine.UpdateGroupValue(apiRequest);

			var accum = m_engine.GetGroupsValues(new MariPriceApi.Price.GroupValue.List().ForIds(request.PriceGroupValueId)).FirstOrDefault();

			return new MariPriceApi.PortalPrice.GroupValue().CopyFrom(accum, m_dto);
		}

		public MariPriceApi.PortalPrice.InstockGroupValue UpdateGroupInstockVaue(MariPriceApi.PortalPrice.InstockGroupValue.Update request)
		{
			var apiRequest = new MariPriceApi.Price.InstockGroupValue.Update().CopyFrom(request, m_dto);

			var origin = m_engine.GetGroupsInstockValues(new MariPriceApi.Price.InstockGroupValue.List().ForGroups(request.PriceGroupId))
				.FirstOrDefault();
			if (origin == null)
			{
				m_engine.CreateInstockGroupValue(new MariPriceApi.Price.InstockGroupValue.Create().CopyFrom(apiRequest, m_dto));
			}
			else
			{
				apiRequest.UpdationList = new[] {
					nameof(apiRequest.WithNdsPrice),
					nameof(apiRequest.WithoutNdsPrice),
					nameof(apiRequest.WithNdsMarkup),
					nameof(apiRequest.WithoutNdsMarkup)
				};

				m_engine.UpdateGroupInstockValue(apiRequest);
			}
			var accum = m_engine.GetGroupsInstockValues(new MariPriceApi.Price.InstockGroupValue.List().ForGroups(request.PriceGroupId))
				.FirstOrDefault();

			return new MariPriceApi.PortalPrice.InstockGroupValue().CopyFrom(accum, m_dto);
		}

		public void CreateProductLink1(MariPriceApi.PortalPrice.Product.Create request)
		{

			var product = m_engine.GetProducts(new MariPriceApi.Price.Product.List().ForIds(request.Id)).FirstOrDefault();
			var newGroupInfo = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(request.PriceGroupId)).FirstOrDefault();
			if (newGroupInfo == null)
				throw new RecordNotFoundApiException($"Cannot find group {request.PriceGroupId}");

			var cluster = m_engine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(newGroupInfo.ClusterId), true).FirstOrDefault();
			if (cluster == null)
				throw new RecordNotFoundApiException($"Cannot find cluster {newGroupInfo.ClusterId}");

			if (product == null)
				throw new RecordNotFoundApiException($"Product {request.Id} was not found");

			var productGroupLinks = new MariPriceDb.Price.Product.GroupLinkGet() { ProductUid = product.ProductUid }.Exec(m_sql);
			if (productGroupLinks.Any())
			{

				if (productGroupLinks.Any(x => x.PriceClusterId != cluster.Id))
				{
					if (!request.RemoveOldLink)
						throw new PriceGroupLinkOtherClusterException($"Изделие добавлено в другой кластер. При переносе изделия все его размеры будут удалены из кластера. Продолжить?", System.Net.HttpStatusCode.PreconditionFailed);

					var apiSetRequestDelete = new MariPriceApi.Price.Product.Link
					{
						ForClear = new MariPriceApi.Price.Product.Link.Clear
						{
							ProductUid = product.ProductUid,
							PriceClusterId = productGroupLinks.First().PriceClusterId
						}
					};

					//Temporary remove need to checkout why not found company
					//SetLinksAndUpdateDisplayName(apiSetRequestDelete, productGroupLinks.Select(x => x.PriceGroupId).Distinct().ToArray());
					m_engine.SetLinks(apiSetRequestDelete);
				}
				else if (productGroupLinks.Any(x => x.ProductId == request.Id && request.PriceGroupId != x.PriceGroupId))
				{
					if (!request.RemoveOldLink)
						throw new PriceGroupLinkOtherGroupException($"Размер изделия выбран в другой ценовой группе. Перенести размер в текущую ценовую группу?", System.Net.HttpStatusCode.PreconditionFailed);

					var apiSetRequestDelete = new MariPriceApi.Price.Product.Link
					{
						ForDelete = new MariPriceApi.Price.Product.Link.Delete
						{
							PriceGroupId = productGroupLinks.First(x => x.ProductId == request.Id).PriceGroupId,
							ProductId = request.Id
						}
					};

					SetLinksAndUpdateDisplayName(apiSetRequestDelete, productGroupLinks.First(x => x.ProductId == request.Id).PriceGroupId);
				}
			}

			var apiSetRequest = new MariPriceApi.Price.Product.Link
			{
				ForSet = new MariPriceApi.Price.Product.Link.Set
				{
					PriceGroupId = request.PriceGroupId,
					ProductId = request.Id,
					PriceClusterId = newGroupInfo.ClusterId,
					ProductUid = product.ProductUid,
					VersionId = cluster.VersionId
				}
			};

			SetLinksAndUpdateDisplayName(apiSetRequest, request.PriceGroupId);
		}

		private void SetLinksAndUpdateDisplayName(MariPriceApi.Price.Product.Link apiSetRequest, params int[] groupIds)
		{
			m_engine.SetLinks(apiSetRequest);

			foreach (var id in groupIds)
			{
				m_engine.UpdateDisplayNamePriceGroup(id);
			}

		}

		public void DeleteProductLink1(MariPriceApi.PortalPrice.Product.Delete request)
		{
			var newGroupInfo = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(request.PriceGroupId)).FirstOrDefault();
			if (newGroupInfo == null)
				throw new RecordNotFoundApiException($"Cannot find group {request.PriceGroupId}");

			var apiSetRequest = new MariPriceApi.Price.Product.Link
			{
				ForDelete = new MariPriceApi.Price.Product.Link.Delete
				{
					PriceGroupId = request.PriceGroupId,
					ProductId = request.Id
				}
			};

			SetLinksAndUpdateDisplayName(apiSetRequest, request.PriceGroupId);
		}

		public void DeleteGroup(int priceGroupId)
		{
			m_engine.DeleteGroup(priceGroupId);
		}

		public MariPriceApi.PortalPrice.Group GetById(int Id)
		{
			var originGroup = m_engine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(Id)).Single();

			var result = new MariPriceApi.PortalPrice.Group().CopyFrom(originGroup, m_dto);

			return result;
		}

		public MariPriceApi.PortalPrice.Product GetProductNames(MariPriceApi.PortalPrice.Product.List request)
		{
			var price = m_engine.GetCompanyPrice(request.CompanyId);
			//price.DraftVersionId

			var dbRequest = new MariPriceDb.Price.Product.Search() { VersionId = price.DraftVersionId }.CopyFrom(request, m_dto)
				.ForCompany(request.CompanyId)
				.WithNameLike(request.Filter.Name);

			var groupsForSearch = new List<int>();

			if (request.Filter.OnlyGroupProduct)
				dbRequest.PriceGroupIds = new List<int>() { request.GroupId };

			/*if (groupsForSearch.Any())
				dbRequest.PriceGroupIds = groupsForSearch;*/

			var dbProductUids = dbRequest.Exec(m_sql);

			var result = new MariPriceApi.PortalPrice.Product();
			if (!dbProductUids.Any())
				return result;

			var dbProductRequest = new MariPriceDb.Price.Product.SearchProduct();
			result.Total = dbProductUids.First().TotalCount;

			dbProductRequest.ProductUids = dbProductUids.Select(i => i.ProductUid).ToArray();
			dbProductRequest.VersionId = price.DraftVersionId;
			var dbResult = dbProductRequest.Exec(m_sql);

			if (!dbResult.Any())
				return new MariPriceApi.PortalPrice.Product();


			foreach (var item in dbResult)
			{
				var current = result.Items.FirstOrDefault(x => x.ProductUid == item.ProductUid);
				if (current == null)
				{
					var forAdd = new MariPriceApi.PortalPrice.Product.Item()
					{
						ProductUid = item.ProductUid,
						Id = item.Id,
						Metal = item.Metal,
						Name = item.Name,
						Pn = item.Pn,
						Status = (item.Status.HasValue) ? (Spec.Dto.ProductState)item.Status.Value : Spec.Dto.ProductState.Active,
						Title = item.Title,
						ImageUrl = item.ImageUrl
					};
					NormalizeProduct(request.GroupId, forAdd, item);
					result.Items.Add(forAdd);
				}
				else
					NormalizeProduct(request.GroupId, current, item);
			}


			foreach (var item in result.Items)
			{
				if (item.Sizes.Any())
					item.Sizes = item.Sizes.OrderBy(x => x.SizeName).ToList();

				item.ImageUrl = String.IsNullOrEmpty(item.ImageUrl) ? m_defaultImageUrl : item.ImageUrl;
			}

			return result;
		}

		void NormalizeProduct(int currentGroup, MariPriceApi.PortalPrice.Product.Item product, MariPriceDb.Price.Product.SearchProduct.Item item)
		{
			if (item.SizeUid != Guid.Empty)
			{
				product.Sizes.Add(new MariPriceApi.PortalPrice.Product.Item.ProductSize()
				{
					Id = item.Id,
					SizeId = item.SizeUid,
					SizeName = item.Title + (item.WireThickness == null ? "" : $" Диаметр {item.WireThickness.ToString()}"),
					SizePn = item.Pn,
					ClusterId = item.PriceGroupId == null ? null : item.ClusterId,
					Include = (item.PriceGroupId == currentGroup)
				});
			}
			else
			{
				product.ClusterId = item.ClusterId;
				product.Name = item.Name;
				product.Title = item.Title;
				product.Pn = item.Pn;
				product.Metal = item.Metal;
				product.Status = (item.Status.HasValue) ? (Spec.Dto.ProductState)item.Status.Value : Spec.Dto.ProductState.Active;
				product.Include = (item.PriceGroupId == currentGroup);
				product.ImageUrl = item.ImageUrl;
			}
		}
	}
}
