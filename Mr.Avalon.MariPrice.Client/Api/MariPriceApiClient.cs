using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public class MariPriceApiClient : ApiClient
	{
		public MariPriceApiClient(string baseUrl)
			   : base(baseUrl + "/api/v1")
		{
		}
	}
}
