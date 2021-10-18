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
			public partial class ClusterSetting
			{
				public class Delete
				{
					public int ClusterId { get; set; }
					public int SettingsVariantId { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/cluster/deletesetting")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
