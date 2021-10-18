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
			public partial class Cluster
			{
				[Bind("PriceClusterId")]
				public int Id { get; set; }

				[Bind("VersionId")]
				public int VersionId { get; set; }

				[Bind("Name")]
				public Guid Name { get; set; }

				[Bind("Enabled")]
				public bool Enabled { get; set; }

				[Bind("InStock")]
				public bool InStock { get; set; }

				[Bind("InOrder")]
				public bool InOrder { get; set; }

				[Bind("Metal")]
				public Guid? Metal { get; set; }

				[Bind("Quality")]
				public Guid? Quality { get; set; }
			}
		}
	}
}
