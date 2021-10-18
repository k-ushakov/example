using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class PriceCompanyVersion
			{
				public int VersionId { get; set; }
				public bool WithNdsPriceRequired { get; set; }
				public bool WithoutNdsPriceRequired { get; set; }

				public static PriceCompanyVersion GetPrice(int id, MariPriceApiClient api)
				{
					var request = api.GetRequest($"price/companyversion/{id}");

					return api.Execute<PriceCompanyVersion>(request);
				}
			}
		}
	}
}
