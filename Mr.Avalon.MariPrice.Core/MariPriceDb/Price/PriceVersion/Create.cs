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
			public partial class PriceVersion
			{
				[BindStruct]
				public class Create
				{
					[Bind("WithNdsPriceRequired")]
					public bool WithNdsPriceRequired { get; set; }
					[Bind("WithoutNdsPriceRequired")]
					public bool WithoutNdsPriceRequired { get; set; }

					[Bind("ResultId", Direction = System.Data.ParameterDirection.Output)]
					public int ResultId { get; set; }

					#region c_insertSql

					const string c_insertSql = @"
INSERT [MariPrice].[PriceCompanyVersion](
	[WithNdsPriceRequired], 
	[WithoutNdsPriceRequired])
SELECT
	@WithNdsPriceRequired, 
	@WithoutNdsPriceRequired
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
