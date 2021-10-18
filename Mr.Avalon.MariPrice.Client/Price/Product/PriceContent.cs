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
			public partial class Product
			{
				public class PriceContent
				{
					public class List
					{
						public List<Guid> ProductIds { get; set; } = new List<Guid>();

						public List<Price> Exec(MariPriceApiClient api)
						{
							var request = api.PostRequest("price/pricecontent").Body(this);

							return api.Execute<List<Price>>(request);
						}

						public List ForProduct(Guid Id)
						{
							if (ProductIds == null)
								ProductIds = new List<Guid> { Id };
							else
								ProductIds.Add(Id);

							return this;
						}
					}

				}
			}
		}
	}
}
