using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class ClusterSetting
			{
				public class Create
				{
					public int ClusterId { get; set; }

					public decimal OrderMetalWeight { get; set; }
					public int ProductionTime { get; set; }

					public ClusterSetting Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/cluster/createsetting").Body(this);

						return api.Execute<ClusterSetting>(request);
					}
				}
			}
		}
	}
}
