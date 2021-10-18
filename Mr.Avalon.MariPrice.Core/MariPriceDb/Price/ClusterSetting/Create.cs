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
			public partial class ClusterSetting
			{
				[BindStruct]
				public class Create
				{
					[Bind("PriceClusterId")]
					public int ClusterId { get; set; }

					[Bind("OrderMetalWeight")]
					public decimal OrderMetalWeight { get; set; }

					[Bind("ProductionTime")]
					public int ProductionTime { get; set; }

					[Bind("Enabled")]
					public bool Enabled { get; set; }

					[Bind("ResultId", Direction = System.Data.ParameterDirection.Output)]
					public int ResultId { get; set; }

					#region insertSql

					const string c_insertSql = @"
INSERT [MariPrice].[PriceClusterSetting]   (
	 [PriceClusterId]
	,[OrderMetalWeight]
	,[ProductionTime]
	,[Enabled])
SELECT
	@PriceClusterId, 
	@OrderMetalWeight, 
	@ProductionTime,
	1
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
