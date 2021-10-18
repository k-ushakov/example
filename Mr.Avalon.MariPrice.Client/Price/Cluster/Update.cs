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
			public partial class Cluster
			{
				public class Update
				{
					public int Id { get; set; }

					public bool Enabled { get; set; }
					public bool InStock { get; set; }
					public bool InOrder { get; set; }

					public Guid Name { get; set; }
					public Guid? Metal { get; set; }
					public Guid? Quality { get; set; }
					public List<ClusterSetting> VariantsSettings { get; set; }

					public string[] UpdationList { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/cluster/update")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
