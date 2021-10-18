using Mr.Avalon.Spec.Dto;
using System;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Product
			{
				public int Id { get; set; }
				public Guid ProductUid { get; set; }
				public Guid SizeUid { get; set; }

				public string Name { get; set; }
				public string Pn { get; set; }

				public string Metal { get; set; }

				public string SizePn { get; set; }

				public int CompanyId { get; set; }

				public bool Enabled { get; set; }

				public int? PriceGroupId { get; set; }

				public ProductState Status { get; set; }
				
				public string SizeFullName { get; set; }

			}
		}
	}
}
