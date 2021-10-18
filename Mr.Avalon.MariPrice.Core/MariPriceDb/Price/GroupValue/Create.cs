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
			public partial class GroupValue
			{
				[BindStruct]
				public class Create
				{
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

					[Bind("ResultId", Direction = System.Data.ParameterDirection.Output)]
					public int ResultId { get; set; }

					#region insertSql

					const string c_insertSql = @"
INSERT [MariPrice].[PriceGroupValue](
	[PriceGroupId], 
	[PriceClusterSettingId], 
	[WithNdsPrice],
	[WithoutNdsPrice],
	[WithNdsMarkup],
	[WithoutNdsMarkup])
SELECT
	@PriceGroupId, 
	@PriceClusterSettingId, 
	@WithNdsPrice,
	@WithoutNdsPrice,
	@WithNdsMarkup,
	@WithoutNdsMarkup
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
