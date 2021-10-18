using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
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

					public Group Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/group/update")
							.Body(this);

						return api.Execute<Group>(request);
					}
				}
			}
		}
	}
}
