using Mr.Avalon.Common.Client;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Group
			{
				public class Export
				{
					public int CompanyId { get; set; }
					public int PriceClusterId { get; set; }

					public Response Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/group/export").Body(this);
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