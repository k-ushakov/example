using System;
using System.Collections.Generic;
using System.Text;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			[BindStruct]
			public partial class ClusterSetting
			{
				[Bind("PriceClusterSettingId")]
				public int Id { get; set; }

				[Bind("PriceClusterId")]
				public int ClusterId { get; set; }

				[Bind("OrderMetalWeight")]
				public decimal OrderMetalWeight { get; set; }

				[Bind("ProductionTime")]
				public int ProductionTime { get; set; }

				[Bind("Enabled")]
				public bool Enabled { get; set; }
			}
		}
	}
}
