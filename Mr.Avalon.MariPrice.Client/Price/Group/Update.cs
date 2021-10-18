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
			public partial class Group
			{
				public class Update
				{
					public int Id { get; set; }

					public string Name { get; set; }
					public string DisplayName { get; set; }
					public decimal LossPercentage { get; set; }
					public decimal? AdditionalLossPercentage { get; set; }
					public string ProductPublishInfo { get; set; }

					public List<GroupValue> Values { get; set; }
					public InstockGroupValue InStockValues { get; set; }

					public string[] UpdationList { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/update")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
