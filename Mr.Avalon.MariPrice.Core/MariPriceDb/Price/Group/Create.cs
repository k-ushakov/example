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
			public partial class Group
			{
				[BindStruct]
				public class Create
				{
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

					[Bind("Enabled")]
					public bool Enabled { get; set; }

					[Bind("ResultId", Direction = System.Data.ParameterDirection.Output)]
					public int ResultId { get; set; }

					#region insertSql

					const string c_insertSql = @"
INSERT [MariPrice].[PriceGroup](
	[PriceClusterId], 
	[Name],  
	[DisplayName], 
	[ProductPublishInfo],
	[LossPercentage],
	[AdditionalLossPercentage],
	[Enabled])
SELECT
	@PriceClusterId, 
	@Name, 
	@DisplayName,
	@ProductPublishInfo,
	@LossPercentage,
	@AdditionalLossPercentage,
	@Enabled
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
