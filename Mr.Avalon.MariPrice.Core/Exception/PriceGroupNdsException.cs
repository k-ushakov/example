using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupNdsException : Common.ServerErrorApiException
	{
		public PriceGroupNdsException()
			: base()
		{
		}
		public PriceGroupNdsException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
