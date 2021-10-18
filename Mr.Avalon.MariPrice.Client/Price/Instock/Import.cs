using Mr.Avalon.Common.Client;
using System.Collections.Generic;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Instock
			{
				public class Import
				{
					public class Item
					{
						public string Barcode { get; set; }
						public int ProductId { get; set; }
						public decimal Weight { get; set; }
					}

					public int CompanyId { get; set; }
					public List<Item> NewBarcodes { get; set; } = new List<Item>();

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/instock/import")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
