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
				public class DeleteItem
				{
					public int ClusterId { get; set; }
					public int SettingsVariantId { get; set; }

					public Info Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/settings/item/delete")
							.Body(this);

						return api.Execute<Info>(request);
					}
				}
			}
		}
	}
}
