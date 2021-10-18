using Mr.Avalon.Common.Client;
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
				public class Update
				{
					public int VersionId { get; set; }
					public bool WithNdsPriceRequired { get; set; }
					public bool WithoutNdsPriceRequired { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/companyversion/update")
							.Body(this);

						api.Execute(request);
					}
				}
			}
		}
	}
}
