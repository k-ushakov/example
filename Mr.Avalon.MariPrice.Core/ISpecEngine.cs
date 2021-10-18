using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.Spec.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	public interface ISpecEngine
	{
		SpecApi.VocValue.List GetVocValues(Guid vocId, params Guid[] valueIds);
		List<SpecApi.ImageContent> GetImageContents(params Guid[] productIds);
		List<SpecApi.PriceContent> GetPriceContents(params Guid[] productIds);
		SpecApi.Product GetProduct(Guid productId);
		SpecApi.Source GetSource(Guid sourceId);
		List<SpecApi.SpecContent> GetSpecContents(params Guid[] productIds);
		List<SpecApi.Source> GetAvailableSources(UserInfo user, List<int> companyIds);
		List<SpecApi.Product> GetProductsCompany(Guid sourceId);
		List<SpecApi.PriceGroup> GetPriceGroup(Guid priceGroupId);
		List<SpecApi.PriceSettings> GetPriceSettings(Guid sourceId);

		void UpdatePriceGroup(List<SpecApi.PriceGroup> priceGroups);
	}
}
