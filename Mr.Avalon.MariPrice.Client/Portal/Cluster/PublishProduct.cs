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
				public class PublishProduct
				{
					public int ClusterId { get; set; }

					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster/pubishproduct")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}
			}
		}
	}
}
