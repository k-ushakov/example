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
				public List<Price.Technologies.TechnologyPortal> Technologies { get; set; }

				public static TechnologiesAdditions ExecGet(int versionId, MariPriceApiClient api)
				{
					var request = api.GetRequest($"portal/price/technologies/active/{versionId}");

					return api.Execute<TechnologiesAdditions>(request);
				}
			}
		}
	}
}
