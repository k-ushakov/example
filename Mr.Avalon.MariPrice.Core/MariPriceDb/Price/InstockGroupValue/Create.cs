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
			public partial class InstockGroupValue
			{
				[BindStruct]
				public class Create
				{
					[Bind("PriceGroupId")]
					public int PriceGroupId { get; set; }

					[Bind("WithNdsPrice")]
					public decimal? WithNdsPrice { get; set; }

					[Bind("WithoutNdsPrice")]
					public decimal? WithoutNdsPrice { get; set; }

					[Bind("WithNdsMarkup")]
					public decimal? WithNdsMarkup { get; set; }

					[Bind("WithoutNdsMarkup")]
					public decimal? WithoutNdsMarkup { get; set; }

					#region insertSql

					const string c_insertSql = @"
INSERT [MariPrice].[PriceGroupInstockValue](
	[PriceGroupId],
	[WithNdsPrice],
	[WithoutNdsPrice],
	[WithNdsMarkup],
	[WithoutNdsMarkup])
SELECT
	@PriceGroupId,
	@WithNdsPrice,
	@WithoutNdsPrice,
	@WithNdsMarkup,
	@WithoutNdsMarkup
";

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(c_insertSql, this);
					}
				}
			}
		}
	}
}
