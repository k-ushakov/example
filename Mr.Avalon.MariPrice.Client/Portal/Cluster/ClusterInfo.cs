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
				public class Info
				{
					public int Id { get; set; }
					public int VersionId { get; set; }

					public bool Enabled { get; set; }
					public bool InStock { get; set; }
					public bool InOrder { get; set; }

					public EntityName<Guid> ClusterName { get; set; }
					public EntityName<Guid> ClusterMetal { get; set; }
					public EntityName<Guid> ClusterQuality { get; set; }

					public List<Price.ClusterSetting> Variants { get; set; }
				}
			}
		}
	}
}
