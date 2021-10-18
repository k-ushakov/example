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
			public partial class GroupValue
			{
				public class List
				{
					public List<int> Ids { get; set; } = new List<int>();
					public List<int> GroupIds { get; set; } = new List<int>();

					public List<GroupValue> Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/group/value").Body(this);

						return api.Execute<List<GroupValue>>(request);
					}

					public List ForIds(params int[] ids)
					{
						if (ids?.Any() == true)
							Ids = ids.ToList();
						return this;
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
