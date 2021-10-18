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
			public partial class TechnologiesAdditions
			{
				public class LoadActiveVersion
				{
					public int CompanyId { get; set; }

					public Price.Technologies.TechnologyPortal Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/technologies/loadactive")
							.Body(this);

						return api.Execute<Price.Technologies.TechnologyPortal>(request);
					}
				}
			}
		}
	}
}
