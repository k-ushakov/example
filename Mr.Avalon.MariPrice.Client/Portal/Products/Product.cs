using Mr.Avalon.Common.Client;
using Mr.Avalon.Spec.Dto;
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
				public List<Item> Items { get; set; } = new List<Item>();
				public long Total { get; set; }

				public class Item
				{
					public int Id { get; set; }
					public Guid ProductUid { get; set; }

					public string Name { get; set; }
					//public int 
					public string Pn { get; set; }
					public string Metal { get; set; }

					public ProductState Status { get; set; }
					public string Title { get; set; }

					public int? ClusterId { get; set; }

					public string ImageUrl { get; set; }

					public bool Include { get; set; }

					public List<ProductSize> Sizes { get; set; } = new List<ProductSize>();

					public class ProductSize
					{
						public int Id { get; set; }
						public Guid SizeId { get; set; }
						public string SizeName { get; set; }
						public string SizePn { get; set; }
						public bool Include { get; set; }
						public int? ClusterId { get; set; }
					}
				}
			}
		}
	}
}
