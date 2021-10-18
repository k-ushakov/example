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
			public partial class GroupValue
			{
				public class Delete
				{
					public int PriceGroupValueId { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/value/delete")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
