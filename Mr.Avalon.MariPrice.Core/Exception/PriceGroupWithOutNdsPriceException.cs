using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupWithOutNdsPriceException : Common.ServerErrorApiException
	{
		public PriceGroupWithOutNdsPriceException()
			: base()
		{
		}
		public PriceGroupWithOutNdsPriceException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
