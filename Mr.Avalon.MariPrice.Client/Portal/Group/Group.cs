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
				public int Id { get; set; }
				public int ClusterId { get; set; }

				public string Name { get; set; }
				public string DisplayName { get; set; }
				public decimal LossPercentage { get; set; }
				public decimal? AdditionalLossPercentage { get; set; }
				public decimal? TotalLossPercentage { get; set; }
				public string ProductPublishInfo { get; set; }

				public List<Price.GroupValue> Values { get; set; }
				public Price.InstockGroupValue InStockValues { get; set; }

				public List<EntityName<int>> Products { get; set; }

				public static List<EntityName<int>> ExecGet(MariPriceApiClient api, int companyId, string groupName)
				{
					var request = api.GetRequest($"portal/price/group/list");
					request.QueryParameters.Add(new KeyValuePair<string, object>("companyId", companyId));
					request.QueryParameters.Add(new KeyValuePair<string, object>("groupName", groupName));

					return api.Execute<List<EntityName<int>>>(request);
				}

				public static Group Exec(MariPriceApiClient api, int groupId)
				{
					var request = api.GetRequest($"portal/price/group/{groupId}");

					return api.Execute<Group>(request);
				}
			}
		}
	}
}
