using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Cluster
			{
				public class Append
				{
					public decimal OrderMetalWeight { get; set; }
					public int ProductionTime { get; set; }

					public int ClusterId { get; set; }

					public MariPriceApi.PortalPrice.Cluster Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/settings/create")
							.Body(this);

						return api.Execute<MariPriceApi.PortalPrice.Cluster>(request);
					}
				}
			}
		}
	}
}
