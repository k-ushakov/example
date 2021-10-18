using Mr.Avalon.Common.Client;
using Mr.Avalon.Common.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Product
			{
				public class List
				{
					public int CompanyId { get; set; }
					public int GroupId { get; set; }
					public FilterInfo Filter { get; set; } = new FilterInfo();
					public PageInfo PageInfo { get; set; } = new PageInfo();

					public Product Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/product/list")
							.Body(this);

						return api.Execute<Product>(request);
					}

					public class FilterInfo
					{
						public bool OnlyGroupProduct { get; set; }

						public string Name { get; set; }
					}
				}
			}
		}
	}
}
