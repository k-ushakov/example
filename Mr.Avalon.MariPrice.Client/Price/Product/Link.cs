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
				public class Link
				{
					public Set ForSet { get; set; }
					public Delete ForDelete { get; set; }
					public Clear ForClear { get; set; }

					public class Clear
					{
						public Guid ProductUid { get; set; }
						public int PriceClusterId { get; set; }
					}

					public class Delete
					{
						public int PriceGroupId { get; set; }
						public int ProductId { get; set; }
					}

					public class Set
					{
						public int PriceGroupId { get; set; }
						public int ProductId { get; set; }
						public int PriceClusterId { get; set; }
						public int VersionId { get; set; }
						public Guid ProductUid { get; set; }
					}

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/product/link")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
