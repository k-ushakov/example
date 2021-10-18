using Mr.Avalon.Common.Client;
using Mr.Avalon.MariPrice.Client.Portal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Cluster
			{
				public Info ManufactureSettings { get; set; }
				public BulkPublishInformationByCluster LastClusterPublishInfo { get; set; }
				public List<Group> PriceGroups { get; set; }

				public static Cluster ExecGet(int companyId, int clusterId, MariPriceApiClient api)
				{
					var request = api.GetRequest("portal/price/cluster");
					request.QueryParameters.Add(new KeyValuePair<string, object>("companyId", companyId));
					request.QueryParameters.Add(new KeyValuePair<string, object>("clusterId", clusterId));

					return api.Execute<Cluster>(request);
				}
			}
		}
	}
}
