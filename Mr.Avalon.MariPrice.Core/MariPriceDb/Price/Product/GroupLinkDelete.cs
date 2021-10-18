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
				public class GroupLinkDelete
				{
					[Bind("PriceGroupId")]
					public int? PriceGroupId { get; set; }

					[Bind("ProductUid")]
					public Guid ProductUid { get; set; }

					[Bind("ProductUidForClear")]
					public Guid? ProductUidForClear { get; set; }

					[Bind("PriceCusterId")]
					public int PriceCusterId { get; set; }

					public bool? DeleteClusterProduct { get; set; }

					public bool? DeleteUnusedClusterProducts { get; set; }

					public List<int> ProductIds { get; set; }

					#region updateSql

					const string c_updateSql = @"
DELETE link
FROM [MariPrice].[PriceGroupLink] link
WHERE 
	
	
	--{ProductUidForClearSection - start}
	link.[ProductUid] = @ProductUidForClear AND 
	--{ProductUidForClearSection - end}

	--{PriceGroupIdSection - start}
	link.[PriceGroupId] = @PriceGroupId AND 
	--{PriceGroupIdSection - end}

	--{ProductIds - start}
	link.[ProductId] in ({ProductIds}) and
	--{ProductIds - end}

1=1

--{DeleteClusterProduct - start}
	DELETE link
	FROM [MariPrice].[ProductCluster] link
	WHERE 
		link.[ProductUid] = @ProductUid AND 

		link.[PriceClusterId]=@PriceCusterId AND
	1=1
--{DeleteClusterProduct - end}

--{DeleteUnusedClusterProducts - start}
	DELETE link
	FROM [MariPrice].[ProductCluster] link
	WHERE 
		link.[PriceClusterId]=@PriceCusterId AND
		link.[ProductUid] not in (SELECT pgl.[ProductUid] from [MariPrice].[PriceGroupLink] pgl WHERE pgl.PriceClusterId=@PriceCusterId) AND 
	1=1
--{DeleteUnusedClusterProducts - end}

";

					#endregion

					public void Exec(ISqlExecutor sql)
					{
						var query = c_updateSql;

						if (DeleteClusterProduct ?? false)
							query = SqlQueriesFormater.RemoveLabels(query, "DeleteClusterProduct");
						else
							query = SqlQueriesFormater.RemoveSubString(query, "DeleteClusterProduct");

						if (DeleteUnusedClusterProducts ?? false)
							query = SqlQueriesFormater.RemoveLabels(query, "DeleteUnusedClusterProducts");
						else
							query = SqlQueriesFormater.RemoveSubString(query, "DeleteUnusedClusterProducts");

						query = SqlQueriesFormater.RemoveOrReplace("ProductIds", ProductIds, x => string.Join(",", x)).Format(query);

						if (!PriceGroupId.HasValue)
							query = SqlQueriesFormater.RemoveSubString(query, "PriceGroupIdSection");
						if (!ProductUidForClear.HasValue)
							query = SqlQueriesFormater.RemoveSubString(query, "ProductUidForClearSection");


						query = SqlQueriesFormater.RemoveLabels(query, "PriceGroupIdSection", "ProductUidForClearSection");

						sql.Query(query, this);
					}
				}
			}
		}
	}
}
