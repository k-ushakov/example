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
			public partial class Cluster
			{
				public class List
				{
					public List<int> Ids { get; set; } = new List<int>();
					public List<int> VersionIds { get; set; }

					public List<Cluster> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/cluster").Body(this);

						return api.Execute<List<Cluster>>(request);
					}

					public List ForVersion(params int[] versionIds)
					{
						if (versionIds != null)
							VersionIds = versionIds.ToList();
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
