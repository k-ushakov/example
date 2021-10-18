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
				public int CompanyId { get; set; }
				public int ActiveVersionId { get; set; }
				public int DraftVersionId { get; set; }

				public static PriceCompany GetPrice(int companyId, MariPriceApiClient api)
				{
					var request = api.GetRequest($"price/company/{companyId}");

					return api.Execute<PriceCompany>(request);
				}
			}
		}
	}
}
