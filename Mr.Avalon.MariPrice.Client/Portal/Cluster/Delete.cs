﻿using Mr.Avalon.Common.Client;
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
				public class Delete
				{
					public int ClusterId { get; set; }

					public MariPriceApi.PortalPrice Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/cluster/delete")
							.Body(this);

						return api.Execute<MariPriceApi.PortalPrice>(request);
					}
				}
			}
		}
	}
}
