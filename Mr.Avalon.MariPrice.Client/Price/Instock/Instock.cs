using System;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Instock
			{
				public string Barcode { get; set; }
				public int ProductId { get; set; }
				public decimal Weight { get; set; }
				public int CompanyId { get; set; }
				public Guid ProductUid { get; set; }
				public Guid SizeUid { get; set; }

				public InstockShort GetInfo()
				{
					return new InstockShort
					{
						Barcode = Barcode,
						Weight = Weight
					};
				}
			}

			public class InstockShort
			{
				public string Barcode { get; set; }
				public decimal Weight { get; set; }
			}
		}
	}
}
