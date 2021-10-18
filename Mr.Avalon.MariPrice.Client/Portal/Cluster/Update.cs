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
				public class Update
				{
					public int ClusterId { get; set; }
					public bool Enabled { get; set; }
					public Guid ClusterName { get; set; }
					public Guid ClusterMetal { get; set; }
					public Guid ClusterQuality { get; set; }
					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster/update")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}

				public class UpdateStatus
				{
					public int ClusterId { get; set; }
					public bool Enabled { get; set; }

					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster/updatestatus")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}
			}
		}
	}
}
