using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupWithNdsPriceException : Common.ServerErrorApiException
	{
		public PriceGroupWithNdsPriceException()
			: base()
		{
		}
		public PriceGroupWithNdsPriceException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
