using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.Spec.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	public class SpecEngine : ISpecEngine
	{
		SpecApiClient m_specClient;

		public SpecEngine(SpecApiClient specClient)
		{
			m_specClient = specClient;
		}

		public SpecApi.Product GetProduct(Guid productId)
		{
			return SpecApi.Product.ExecGet(m_specClient, productId);
		}

		public SpecApi.Source GetSource(Guid sourceId)
		{
			return new SpecApi.Source.List { Ids = new List<Guid> { sourceId } }.Exec(m_specClient).Single();
		}

		public List<SpecApi.SpecContent> GetSpecContents(params Guid[] productIds)
		{
			return new SpecApi.SpecContent.List { ProductIds = productIds.ToList() }.Exec(m_specClient);
		}

		public List<SpecApi.ImageContent> GetImageContents(params Guid[] productIds)
		{
			return new SpecApi.ImageContent.List { ProductIds = productIds.ToList() }.Exec(m_specClient);
		}

		public List<SpecApi.PriceContent> GetPriceContents(params Guid[] productIds)
		{
			return new SpecApi.PriceContent.List { ProductIds = productIds.ToList() }.Exec(m_specClient);
		}

		public List<SpecApi.Source> GetAvailableSources(UserInfo user, List<int> companyIds)
		{
			var request = new SpecApi.Source.Available { CompanyIds = companyIds };
			if (user != null)
				request.ForUser = new SpecApi.Source.Available.Requestor
				{
					Id = user.UserId,
					IsManager = user.Roles.Contains(UserRoles.MariManager)
				};

			return request.Exec(m_specClient);
		}

		public SpecApi.VocValue.List GetVocValues(Guid vocId, params Guid[] valueIds)
		{
			var request = new SpecApi.VocValue.List.Request { VocId = vocId };
			if (valueIds?.Any() == true)
				request.Ids = valueIds.ToList();
			return request.Exec(m_specClient);
		}

		public List<SpecApi.Product> GetProductsCompany(Guid sourceId)
		{
			var request = new SpecApi.Product.BySource() { SourceId = sourceId, State = Spec.Dto.ProductState.Active }.Exec(m_specClient);
			return request.Items;
		}

		public List<SpecApi.PriceGroup> GetPriceGroup(Guid priceGroupId)
		{
			var request = new SpecApi.PriceGroup.List().ForIds(priceGroupId).Exec(m_specClient);
			return request;
		}

		public List<SpecApi.PriceSettings> GetPriceSettings(Guid sourceId)
		{
			var request = new SpecApi.PriceSettings.List() { SourceIds = new List<Guid>() { sourceId } }.Exec(m_specClient);
			return request;
		}

		public void UpdatePriceGroup(List<SpecApi.PriceGroup> priceGroups)
		{
			foreach (var item in priceGroups)
			{
				new SpecApi.PriceGroup.Update() { Id = item.Id, Variants = item.Variants, UpdationList = new string[1] { "Variants" } }.Exec(m_specClient);
			}

		}
	}
}
