using Mr.Avalon.Common.Client;
using System.Collections.Generic;
using System.Linq;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Instock
			{
				public class List
				{
					public List<int> CompanyIds { get; set; }

					public List<int> ProductIds { get; set; }

					public List<Instock> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/instock/available")
							.Body(this);

						return api.Execute<List<Instock>>(request);
					}

					public List ForCompanies(params int[] companyIds)
					{
						if (companyIds?.Any() == true)
							CompanyIds = companyIds.ToList();
						return this;
					}

					public List ForProducts(params int[] productIds)
					{
						if (productIds?.Any() == true)
							ProductIds = productIds.ToList();
						return this;
					}
				}
			}
		}
	}
}
