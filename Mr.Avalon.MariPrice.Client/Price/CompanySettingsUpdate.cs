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
			[Obsolete("Need to Remove")]
			public class CompanySettingsUpdate
			{
				public int VersionId { get; set; }
				public bool WithNdsSaleRequired { get; set; }
				public bool WithoutNdsSaleRequired { get; set; }

				public void Exec(MariPriceApiClient api)
				{
					var request = api.PostRequest("price/company/update").Body(this);

					api.Execute(request);
				}
			}
		}
	}
}
