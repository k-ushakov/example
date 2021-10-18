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
				public class Create
				{
					public int VersionId { get; set; }
					public Guid ClusterName { get; set; }
					public Guid ClusterMetal { get; set; }
					public Guid ClusterQuality { get; set; }
					
					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}
			}
		}
	}
}
