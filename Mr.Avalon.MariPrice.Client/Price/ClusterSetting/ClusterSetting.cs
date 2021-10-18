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
				public int Id { get; set; }

				public decimal OrderMetalWeight { get; set; }
				public int ProductionTime { get; set; }
			}
		}
	}
}
