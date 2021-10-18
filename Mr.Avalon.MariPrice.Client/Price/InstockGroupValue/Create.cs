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
				public class Create
				{
					public int PriceGroupId { get; set; }

					public decimal? WithNdsPrice { get; set; }
					public decimal? WithNdsMarkup { get; set; }
					public decimal? WithoutNdsPrice { get; set; }
					public decimal? WithoutNdsMarkup { get; set; }

					public InstockGroupValue Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/instockvalue/create").Body(this);

						return api.Execute<InstockGroupValue>(request);
					}
				}
			}
		}
	}
}
