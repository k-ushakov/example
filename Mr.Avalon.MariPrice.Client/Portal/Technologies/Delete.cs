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
				public class Delete
				{
					public int VersionId { get; set; }
					public Guid TechnologyId { get; set; }

					public TechnologiesAdditions Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/technologies/delete")
							.Body(this);

						return api.Execute<TechnologiesAdditions>(request);
					}
				}
			}
		}
	}
}
