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
			public partial class Cluster
			{
				[BindStruct]
				public class Create
				{
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

					[Bind("ResultId", Direction = System.Data.ParameterDirection.Output)]
					public int ResultId { get; set; }

					#region insertSql

					const string c_insertSql = @"

INSERT [MariPrice].[PriceCluster] (
	[VersionId], 
	[Name],
	[Enabled],
	[InOrder],
	[InStock],
	[Metal],
	[Quality])
SELECT
	@VersionId, 
	@Name, 
	@Enabled,
	@InOrder,
	@InStock,
	@Metal,
	@Quality
-----

set @ResultId=@@identity
";

					#endregion

					public int Exec(ISqlExecutor sql)
					{
						sql.Query(c_insertSql, this);

						return ResultId;
					}
				}
			}
		}
	}
}
