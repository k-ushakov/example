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
			public class BulkSet
			{
				public int VersionId { get; set; }

				public bool WithNdsPriceRequired { get; set; }
				public bool WithoutNdsPriceRequired { get; set; }

				public List<Group.VariantSet> Groups { get; set; } = new List<Group.VariantSet>();
				public List<Cluster.SwitchItem> Switchers { get; set; } = new List<Cluster.SwitchItem>();
			
				public PortalPrice Exec(MariPriceApiClient api)
				{
					var request = api.PostRequest("portal/price/set")
						.Body(this);

					return api.Execute<PortalPrice>(request);
				}
			}
		}
	}
}
