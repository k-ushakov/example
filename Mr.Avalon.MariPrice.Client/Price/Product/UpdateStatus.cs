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
				public class UpdateStatus
				{
					public Guid ProductUid { get; set; }
					public int Status { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/product/updatestatus")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
