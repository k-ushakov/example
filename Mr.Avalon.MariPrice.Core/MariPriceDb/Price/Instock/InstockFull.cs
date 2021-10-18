using System;
using Utilities.Sql;
using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			[BindStruct]
			public partial class InstockFull
			{
				[Bind("ProductId")]
				public int ProductId { get; set; }

				[Bind("Barcode")]
				public string Barcode { get; set; }

				[Decimal("Weight", 18, 5)]
				public decimal Weight { get; set; }

				[Bind("CompanyId")]
				public int CompanyId { get; set; }

				[Bind("ProductUid")]
				public Guid ProductUid { get; set; }

				[Bind("SizeUid")]
				public Guid SizeUid { get; set; }

				[Bind("Size")]
				public string Size { get; set; }

				[Bind("Pn")]
				public string Pn { get; set; }

				[Bind("SizePn")]
				public string SizePn { get; set; }

				[Bind("PriceGroupId")]
				public int? PriceGroupId { get; set; }

				[Bind("Name")]
				public string GroupName { get; set; }

				[Decimal("WireThickness", 18, 5)]
				public decimal? WireThickness { get; set; }
			}
		}
	}
}
