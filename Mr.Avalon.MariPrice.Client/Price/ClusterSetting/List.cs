using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class ClusterSetting
			{
				public class List
				{
					public List<int> Ids { get; set; } = new List<int>();
					public List<int> PriceClusterIds { get; set; }

					public List<ClusterSetting> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/clustersetting").Body(this);

						return api.Execute<List<ClusterSetting>>(request);
					}

					public List ForCluster(params int[] clasterIds)
					{
						if (clasterIds != null)
							PriceClusterIds = clasterIds.ToList();
						return this;
					}

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}
				}
			}
		}
	}
}
