using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupWithOutNdsInStockPriceException : Common.ServerErrorApiException
	{
		public PriceGroupWithOutNdsInStockPriceException()
			: base()
		{
		}
		public PriceGroupWithOutNdsInStockPriceException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
