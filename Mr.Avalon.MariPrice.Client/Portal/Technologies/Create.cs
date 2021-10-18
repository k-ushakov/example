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
				public class Create
				{
					public int VersionId { get; set; }
					public Guid TechnologyId { get; set; }
					public decimal? WithNdsPrice { get; set; }
					public decimal? WithoutNdsPrice { get; set; }

					public Price.Technologies.TechnologyPortal Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/technologies/create")
							.Body(this);

						return api.Execute<Price.Technologies.TechnologyPortal>(request);
					}
				}
			}
		}
	}
}
