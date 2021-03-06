using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class GroupWithVersion
			{
				public int Id { get; set; }
				public int ClusterId { get; set; }

				public string Name { get; set; }
				public string DisplayName { get; set; }
				public decimal LossPercentage { get; set; }
				public decimal? AdditionalLossPercentage { get; set; }
				public decimal? TotalLossPercentage { get; set; }
				public string ProductPublishInfo { get; set; }

				public List<GroupValue> Values { get; set; } = new List<GroupValue>();
				public InstockGroupValue InStockValues { get; set; }

				public int VersionId { get; set; }
				public int ActiveVersionId { get; set; }
				public int DraftVersionId { get; set; }
			}
		}
	}
}
