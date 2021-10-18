using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Group
			{
				public class SetPrices
				{
					public Dictionary<int, List<GroupValue>> Groups { get; set; }
				}
			}
		}
	}
}
