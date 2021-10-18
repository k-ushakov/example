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
				public class VariantSet
				{
					public int Id { get; set; }
					public List<Price.GroupValue> Variants { get; set; }
				}
			}
		}
	}
}
