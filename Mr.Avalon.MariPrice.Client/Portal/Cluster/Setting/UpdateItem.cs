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
				public class UpdateItem
				{
					public int ClusterId { get; set; }
					public int SettingsVariantId { get; set; }

					public decimal OrderMetalWeight { get; set; }
					public int ProductionTime { get; set; }

					public Price.ClusterSetting Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/settings/item/update")
							.Body(this);

						return api.Execute<Price.ClusterSetting>(request);
					}
				}
			}
		}
	}
}
