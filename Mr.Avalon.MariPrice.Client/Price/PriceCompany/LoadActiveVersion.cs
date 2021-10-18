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
			public partial class PriceCompany
			{
				public class LoadActiveVersion
				{
					public int CompanyId { get; set; }

					public PortalPrice Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/loadactive")
							.Body(this);

						return api.Execute<PortalPrice>(request);
					}
				}
			}
		}
	}
}
