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
			public partial class Group
			{
				public class ListWithVersion
				{
					public List<int> Ids { get; set; } = new List<int>();
					public List<int> ClusterIds { get; set; } = new List<int>();
					public string Name { get; set; }

					public List<GroupWithVersion> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group-with-version").Body(this);

						return api.Execute<List<GroupWithVersion>>(request);
					}

					public ListWithVersion ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public ListWithVersion ForClusters(params int[] clusterIds)
					{
						if (clusterIds?.Any() == true)
							ClusterIds = clusterIds.ToList();
						return this;
					}

					public ListWithVersion ForName(string name)
					{
						Name = name;
						return this;
					}
				}
			}
		}
	}
}
