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
			public partial class CompanyTechnology
			{
				[BindStruct]
				public class Create
				{
					[Bind("VersionId")]
					public int VersionId { get; set; }

					[Bind("TechnologyId")]
					public Guid TechnologyId { get; set; }
					[Bind("WithNdsPrice")]
					public decimal? WithNdsPrice { get; set; }
					[Bind("WithoutNdsPrice")]
					public decimal? WithoutNdsPrice { get; set; }

					#region insertSql

					const string c_insertSql = @"

INSERT [MariPrice].[CompanyTechnology] (VersionId,TechnologyUid,WithNdsPrice,WithoutNdsPrice) 
	SELECT @VersionId,@TechnologyId,@WithNdsPrice,@WithoutNdsPrice
";
					#endregion

					public void Exec(ISqlExecutor sql)
					{
						var query = c_insertSql;

						sql.Query(query, this);
					}
				}
			}
		}
	}
}
