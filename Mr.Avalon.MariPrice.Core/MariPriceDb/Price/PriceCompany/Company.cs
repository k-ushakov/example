using Utilities.Sql.Data;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class MariPriceDb
	{
		public partial class Price
		{
			[BindStruct]
			public partial class Company
			{
				[Bind("CompanyId")]
				public int CompanyId { get; set; }

				[Bind("ActiveVersionId")]
				public int ActiveVersionId { get; set; }
				[Bind("DraftVersionId")]
				public int DraftVersionId { get; set; }
			}
		}
	}
}
