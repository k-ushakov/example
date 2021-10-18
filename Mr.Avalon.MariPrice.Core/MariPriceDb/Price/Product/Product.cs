using Mr.Avalon.Spec.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
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
			public partial class Product
			{
				[Bind("ProductId")]
				public int Id { get; set; }

				[Bind("ProductUid")]
				public Guid ProductUid { get; set; }
				[Bind("SizeUid")]
				public Guid SizeUid { get; set; }
				[NVarChar("Name", 2000)]
				public string Name { get; set; }
				[NVarChar("Pn", 2000)]
				public string Pn { get; set; }
				[NVarChar("SizePn", 2000)]
				public string SizePn { get; set; }
				[Bind("CompanyId")]
				public int CompanyId { get; set; }
				[Bind("Status")]
				public int? Status { get; set; }
				[Bind("Enabled")]
				public bool Enabled { get; set; }
				[Bind("PriceGroupId")]
				public int? PriceGroupId { get; set; }
				[NVarChar("Metal", 200)]
				public string Metal { get; set; }

				[NVarChar("Size", 200)]
				public string Size { get; set; }
				[Decimal("WireThickness", 18, 2)]
				public decimal? WireThickness { get; set; }

				[NVarChar("SizeFullName", 1000)]
				public string SizeFullName { get; set; }
			}
		}
	}
}
