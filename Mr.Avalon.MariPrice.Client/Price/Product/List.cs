using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Product
			{
				public class List
				{
					public List<int> Ids { get; set; }

					public List<Guid> ProductUids { get; set; }

					public List<Guid> SizeUids { get; set; }

					public List<int> PriceGroupIds { get; set; }

					public List<int> ClusterIds { get; set; }

					public List<string> Pns { get; set; }

					public List<int> CompanyIds { get; set; }

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
					}

					public List ForProducts(params Guid[] productUids)
					{
						if (productUids?.Any() == true)
							ProductUids = productUids.ToList();
						return this;
					}

					public List ForSizes(params Guid[] sizeUids)
					{
						if (sizeUids?.Any() == true)
							SizeUids = sizeUids.ToList();
						return this;
					}

					public List ForPriceGroups(params int[] priceGroups)
					{
						if (priceGroups?.Any() == true)
							PriceGroupIds = priceGroups.ToList();
						return this;
					}

					public List ForClusters(params int[] clusters)
					{
						if (clusters?.Any() == true)
							ClusterIds = clusters.ToList();
						return this;
					}

					public List ForPns(params string[] pns)
					{
						if (pns?.Any() == true)
							Pns = pns.ToList();
						return this;
					}

					public List ForCompanies(params int[] companyIds)
					{
						if (companyIds?.Any() == true)
							CompanyIds = companyIds.ToList();
						return this;
					}


					public List<Product> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/product/list")
							.Body(this);

						return api.Execute<List<Product>>(request);
					}
				}
			}
		}
	}
}
