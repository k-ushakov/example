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
				public class Create
				{
					public int PriceGroupId { get; set; }
					public int PriceClusterVariantId { get; set; }

					public decimal? WithNdsPrice { get; set; }
					public decimal? WithNdsMarkup { get; set; }
					public decimal? WithoutNdsPrice { get; set; }
					public decimal? WithoutNdsMarkup { get; set; }

					public Group Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/value/create").Body(this);

						return api.Execute<Group>(request);
					}
				}
			}
		}
	}
}
