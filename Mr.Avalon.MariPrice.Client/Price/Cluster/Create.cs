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
			public partial class Cluster
			{
				public class Create
				{
					public int VersionId { get; set; }
					public Guid Name { get; set; }

					public bool Enabled { get; set; }
					public bool InStock { get; set; }
					public bool InOrder { get; set; }
					public Guid? Metal { get; set; }
					public Guid? Quality { get; set; }
					public Cluster Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/cluster/create").Body(this);

						return api.Execute<Cluster>(request);
					}
				}
			}
		}
	}
}
