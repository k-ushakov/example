using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupWithNdsPriceInStockException : Common.ServerErrorApiException
	{
		public PriceGroupWithNdsPriceInStockException()
			: base()
		{
		}
		public PriceGroupWithNdsPriceInStockException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
