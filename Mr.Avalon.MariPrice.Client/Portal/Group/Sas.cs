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
				public class SasRequest
				{
					public string Sas { get; set; }
					public string Name { get; set; }

					public class SasSource
					{
						public List<int> CompanyIds { get; set; }
						public int MaxCount { get; set; }
					}

					public List<EntityName<int>> ExecGet(MariPriceApiClient api)
					{
						var request = api.PostRequest($"portal/price/group/list")
							.Body(this);

						return api.Execute<List<EntityName<int>>>(request);
					}
				}
			}
		}
	}
}
