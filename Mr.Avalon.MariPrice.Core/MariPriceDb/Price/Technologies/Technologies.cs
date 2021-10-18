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
			public partial class CompanyTechnology
			{
				[Bind("VersionId")]
				public int VersionId { get; set; }

				[Bind("TechnologyUid")]
				public Guid TechnologyId { get; set; }
				[Bind("WithNdsPrice")]
				public decimal? WithNdsPrice { get; set; }
				[Bind("WithoutNdsPrice")]
				public decimal? WithoutNdsPrice { get; set; }
			}
		}
	}
}
