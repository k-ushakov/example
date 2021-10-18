using Mr.Avalon.Common.Client;
using System;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Instock
			{
				public class Export
				{
					public int CompanyId { get; set; }
					public int VersionId { get; set; }

					public Result Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/instock/export").Body(this);

						return api.Execute<Result>(request);
					}

					public class Result
					{
						public string Url { get; set; }
					}
				}
			}
		}
	}
}
