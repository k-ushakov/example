using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public int VersionId { get; set; }

			public bool WithNdsPriceRequired { get; set; }
			public bool WithoutNdsPriceRequired { get; set; }

			public List<Cluster> Clusters { get; set; } = new List<Cluster>();
			public List<Price.Technologies.TechnologyPortal> Technologies { get; set; } = new List<Price.Technologies.TechnologyPortal>();

			public static PortalPrice GetActive(MariPriceApiClient api, int companyId)
			{
				var request = api.GetRequest($"portal/price/{companyId}");

				return api.Execute<PortalPrice>(request);
			}

			public static PortalPrice GetDraft(MariPriceApiClient api, int companyId)
			{
				var request = api.GetRequest($"portal/price/draft/{companyId}");

				return api.Execute<PortalPrice>(request);
			}
		}
	}
}
