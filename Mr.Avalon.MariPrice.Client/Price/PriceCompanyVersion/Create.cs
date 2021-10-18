using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class PriceCompanyVersion
			{
				public class Create
				{
					public bool WithNdsPriceRequired { get; set; }
					public bool WithoutNdsPriceRequired { get; set; }
				}
			}
		}
	}
}
