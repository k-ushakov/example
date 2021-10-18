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
			public partial class PriceCompanyVersion
			{
				public class Update
				{
					public int VersionId { get; set; }
					public bool WithNdsPriceRequired { get; set; }
					public bool WithoutNdsPriceRequired { get; set; }

					public PortalPrice Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/version/update")
							.Body(this);

						return api.Execute<PortalPrice>(request);
					}
				}
			}
		}
	}
}
