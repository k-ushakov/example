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
			public partial class InstockGroupValue
			{
				public class List
				{
					public List<int> GroupIds { get; set; } = new List<int>();

					public List<InstockGroupValue> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/instockvalue").Body(this);

						return api.Execute<List<InstockGroupValue>>(request);
					}

					public List ForGroups(params int[] groupIds)
					{
						if (groupIds?.Any() == true)
							GroupIds = groupIds.ToList();
						return this;
					}
				}
			}
		}
	}
}
