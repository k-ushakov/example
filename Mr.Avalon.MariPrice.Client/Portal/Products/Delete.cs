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
				public class Delete
				{
					public int PriceGroupId { get; set; }

					public int Id { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/product/delete")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
