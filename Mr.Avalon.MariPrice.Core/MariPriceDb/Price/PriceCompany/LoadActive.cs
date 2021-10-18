using Mr.Avalon.Common;
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
			public partial class Company
			{
				[BindStruct]
				public class LoadActive
				{
					[Bind("CompanyId")]
					public int CompanyId { get; set; }

					#region updateSql

					const string c_updateSql = @"Exec dbo.LoadActiveVersion  @CompanyId";

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						sql.Query(c_updateSql, this);
					}
				}
			}
		}
	}
}
