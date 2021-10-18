using Mr.Avalon.Common.Client;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Product
			{
				public class Import
				{
					public string Url { get; set; }
					public int CompanyId { get; set; }
					public int PriceClusterId { get; set; }

					public Response Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/product/import").Body(this);
						return api.Execute<Response>(request);
					}

					public class Response
					{
						public bool HasErrors { get; set; }
						public string Url { get; set; }
					}
				}
			}
		}
	}
}