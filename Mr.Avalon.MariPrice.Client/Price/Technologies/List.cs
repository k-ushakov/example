using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Technologies
			{
				public class List
				{
					public List<int> VersionIds { get; set; } = new List<int>();

					public List<Technologies> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest($"price/technologies").Body(this);

						return api.Execute<List<Technologies>>(request);
					}

					public List ForVersions(params int[] versionIds)
					{
						if (versionIds != null)
							VersionIds = versionIds.ToList();

						return this;
					}
				}
			}
		}
	}
}
