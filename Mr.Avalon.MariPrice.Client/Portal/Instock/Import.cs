using Mr.Avalon.Common.Client;
using System;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Instock
			{
				public class ImportRequest
				{
					public Guid DownloadSessionId { get; set; }
					public int CompanyId { get; set; }

					public void Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("portal/price/instock/import").Body(this);

						api.Execute(request);
					}

					public class Result
					{
						public string ReportUrl { get; set; }
					}
				}
			}
		}
	}
}
