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
			public partial class InstockGroupValue
			{
				public class Delete
				{
					public int PriceGroupId { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/instockvalue/delete")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
