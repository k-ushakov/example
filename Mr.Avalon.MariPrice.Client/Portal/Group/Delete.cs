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
			public partial class Group
			{
				public class Delete
				{
					public int PriceGroupId { get; set; }

					public MariPriceApi.PortalPrice.Cluster Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/group/delete")
								.Body(this);

						return api.Execute<MariPriceApi.PortalPrice.Cluster>(request);
					}
				}
			}
		}
	}
}
