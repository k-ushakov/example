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
				public class ListProductSize
				{

					[Bind("ProductUid")]
					public Guid ProductUid { get; set; }

					[Bind("VersionId")]
					public int VersionId { get; set; }


					public ListProductSize ForProduct(Guid productUid)
					{
						ProductUid = productUid;
						return this;
					}
					public ListProductSize ForVersion(int versionId)
					{
						VersionId = versionId;
						return this;
					}
		

					#region c_sql
					const string c_sql = @"
SELECT 
	p.ProductId,
	p.ProductUid,
	p.SizeUid,
	p.Name,
	p.Pn,
	p.SizePn,
	p.CompanyId,
	p.Status,
	p.Enabled,
	p.SizeFullName,
	p.Size,
	p.WireThickness,
	p.Metal,
	link.PriceGroupId	
FROM
	[MariPrice].[Product] as [p] WITH (nolock)
LEFT JOIN 
	[MariPrice].[PriceGroupLink] link on p.ProductId = link.ProductId AND link.VersionId=@VersionId

WHERE
	p.ProductUid= @ProductUid	
";

					#endregion

					public List<Product> Exec(ISqlExecutor sql)
					{
						return sql.Query<Product>(GetQuery(),this).ToList();
					}

					string GetQuery()
					{
						var query = c_sql;
						return query;						
					}
				}
			}
		}
	}
}
