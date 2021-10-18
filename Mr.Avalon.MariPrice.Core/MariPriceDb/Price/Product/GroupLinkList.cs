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
			public partial class Product
			{
				[BindStruct]
				public class GroupLinkList
				{
					[Bind("ProductId")]
					public int ProductId { get; set; }
					[Bind("PriceGroupId")]
					public int PriceGroupId { get; set; }

					#region c_sql
					const string c_sql = @"
SELECT 
	link.PriceGroupId,
	link.ProductId,
	link.ProductUid,
	link.PriceClusterId,
	link.VersionId
FROM 
	MariPrice.PriceGroupLink as link inner join (
SELECT * FROM [MariPrice].[PriceGroupLink] WHERE ProductId=@ProductId AND PriceGroupId=@PriceGroupId)
as temp on link.ProductUid= temp.ProductUid and link.VersionId=temp.VersionId
";

					#endregion

					public List<Item> Exec(ISqlExecutor sql)
					{
						var query = c_sql;

						return sql.Query<Item>(query, this).ToList();
					}

					[BindStruct]
					public class Item
					{
						[Bind("PriceGroupId")]
						public int PriceGroupId { get; set; }
						[Bind("ProductId")]
						public int ProductId { get; set; }
						[Bind("ProductUid")]
						public Guid ProductUid { get; set; }
						[Bind("PriceClusterId")]
						public int PriceClusterId { get; set; }
						[Bind("VersionId")]
						public int VersionId { get; set; }
					}
				}
			}
		}
	}
}
