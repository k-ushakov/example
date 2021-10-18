using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupNdsInStockException : Common.ServerErrorApiException
	{
		public PriceGroupNdsInStockException()
			: base()
		{
		}
		public PriceGroupNdsInStockException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
