using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class InstockGroupValue
			{
				public int PriceGroupId { get; set; }

				public decimal? WithNdsPrice { get; set; }
				public decimal? WithNdsMarkup { get; set; }
				public decimal? WithoutNdsPrice { get; set; }
				public decimal? WithoutNdsMarkup { get; set; }
			}
		}
	}
}
