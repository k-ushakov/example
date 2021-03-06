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
			public partial class GroupValue
			{
				[Bind("PriceGroupValueId")]
				public int PriceGroupValueId { get; set; }

				[Bind("PriceGroupId")]
				public int PriceGroupId { get; set; }

				[Bind("PriceClusterSettingId")]
				public int PriceClusterVariantId { get; set; }

				[Bind("WithNdsPrice")]
				public decimal? WithNdsPrice { get; set; }

				[Bind("WithoutNdsPrice")]
				public decimal? WithoutNdsPrice { get; set; }

				[Bind("WithNdsMarkup")]
				public decimal? WithNdsMarkup { get; set; }

				[Bind("WithoutNdsMarkup")]
				public decimal? WithoutNdsMarkup { get; set; }

			}
		}
	}
}
