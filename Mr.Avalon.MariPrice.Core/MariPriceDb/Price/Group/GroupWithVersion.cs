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
			public partial class GroupWithVersion
			{
				[Bind("PriceGroupId")]
				public int Id { get; set; }
				[Bind("PriceClusterId")]
				public int ClusterId { get; set; }

				[NVarChar("Name", 2000)]
				public string Name { get; set; }
				[NVarChar("DisplayName", 200)]
				public string DisplayName { get; set; }
				[Bind("LossPercentage")]
				public decimal LossPercentage { get; set; }
				[Bind("AdditionalLossPercentage")]
				public decimal? AdditionalLossPercentage { get; set; }
				[NVarChar("ProductPublishInfo", 200)]
				public string ProductPublishInfo { get; set; }
				[Bind("VersionId")]
				public int VersionId { get; set; }				
				[Bind("ActiveVersionId")]
				public int ActiveVersionId { get; set; }				
				[Bind("DraftVersionId")]
				public int DraftVersionId { get; set; }
			}
		}
	}
}
