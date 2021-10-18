using Mr.Avalon.Common.Client;
using Mr.Avalon.Description.Dto;
using Mr.Avalon.Spec.Dto;
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
				public class Sync
				{
					public List<Item> Products { get; set; }
					public ProductAction? Action { get; set; }

					public class Item
					{
						public Guid ProductUid { get; set; }

						public List<Guid> Technologys { get; set; }
						public ProductSync DescriptionProductSyncInfo { get; set; }
					}

					public List<Product> Exec(MariPriceApiClient client)
					{
						var request = client.PostRequest("price/product")
							.Body(this);

						return client.Execute<List<Product>>(request);
					}
				}
			}
		}
	}
}
