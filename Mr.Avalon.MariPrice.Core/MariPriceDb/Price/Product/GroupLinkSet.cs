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
				public class GroupLinkSet
				{
					[Bind("PriceGroupId")]
					public int PriceGroupId { get; set; }
					[Bind("ProductId")]
					public int ProductId { get; set; }
					[Bind("PriceClusterId")]
					public int PriceClusterId { get; set; }
					[Bind("VersionId")]
					public int VersionId { get; set; }
					[NVarChar("ProductUid", 200)]
					public string ProductUid { get; set; }

					#region updateSql

					const string c_updateSql = @"
Update link SET 
	link.PriceClusterId = @PriceClusterId,
	link.PriceGroupId = @PriceGroupId,
	link.UpdateTs = getutcdate()
FROM  [MariPrice].[PriceGroupLink] as link
WHERE
	link.ProductUid = @ProductUid
	AND
	link.[PriceClusterId] <>  @PriceClusterId
	AND link.VersionId=@VersionId

IF NOT EXISTS (SELECT PriceGroupId FROM [MariPrice].[PriceGroupLink] l WHERE l.PriceGroupId = @PriceGroupId AND l.ProductId = @ProductId)
BEGIN
	INSERT INTO [MariPrice].[PriceGroupLink]
		([PriceGroupId], [ProductId], [ProductUid], [PriceClusterId],[VersionId], [UpdateTs])
	SELECT @PriceGroupId,@ProductId,@ProductUid,@PriceClusterId,@VersionId,getutcdate()
END

UPDATE productCluster SET 
	productCluster.PriceClusterId = @PriceClusterId
FROM [MariPrice].[ProductCluster] as productCluster  
WHERE
	productCluster.[ProductUid] =@ProductUid
	AND productCluster.VersionId=@VersionId

IF NOT EXISTS
(
    SELECT *
    FROM [MariPrice].[ProductCluster]
    WHERE ProductUid=@ProductUid AND VersionId=@VersionId)
	BEGIN	
		INSERT INTO [MariPrice].[ProductCluster]
		([ProductUid],[PriceClusterId],[VersionId])
			SELECT @ProductUid,@PriceClusterId, @VersionId
	END
";

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						var query = c_updateSql;
						sql.Query(query, this);
					}
				}


				[BindStruct]
				public class GroupLinkGet
				{
					[BindStruct]
					public class Item
					{
						[Bind("PriceGroupId")]
						public int PriceGroupId { get; set; }
						[Bind("ProductId")]
						public int ProductId { get; set; }
						[Bind("PriceClusterId")]
						public int PriceClusterId { get; set; }
						[Bind("VersionId")]
						public int VersionId { get; set; }

						[Bind("ProductUid")]
						public Guid ProductUid { get; set; }
					}

					[Bind("ProductUid")]
					public Guid ProductUid { get; set; }

					#region SelectSql

					const string c_SelectSql = @"

SELECT pg.PriceGroupId,
       pg.ProductId,
       pg.ProductUid,
       pg.PriceClusterId,
       pg.VersionId
FROM   MariPrice.PriceGroupLink pg

WHERE  
pg.ProductUid = @ProductUid
";

					#endregion

					public List<Item>  Exec(ISqlExecutor sql)
					{
						var query = c_SelectSql;				
						return sql.Query<Item>(query,this).ToList();
					}
				}
			}
		}
	}
}
