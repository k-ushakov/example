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
				public class List
				{
					public List<int> Ids { get; set; } = new List<int>();
					public List<int> ClusterIds { get; set; } = new List<int>();
					public string Name { get; set; }

					public bool? OnlyActive { get; set; }

					public List<Group> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group").Body(this);

						return api.Execute<List<Group>>(request);
					}

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public List ForClusters(params int[] clusterIds)
					{
						if (clusterIds?.Any() == true)
							ClusterIds = clusterIds.ToList();
						return this;
					}

					public List ForName(string name)
					{
						Name = name;
						return this;
					}
				}
			}
		}
	}
}
