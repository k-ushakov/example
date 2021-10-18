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
				public class UpdateInStockFlag
				{
					public int ClusterId { get; set; }
					public bool InStock { get; set; }

					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster/updateinstockflag")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}

				public class UpdateInOrderFlag
				{
					public int ClusterId { get; set; }
					public bool InOrder { get; set; }

					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster/updateinorderflag")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}
			}
		}
	}
}
