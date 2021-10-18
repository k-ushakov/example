using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupLinkOtherGroupException : Common.ServerErrorApiException
	{
		public PriceGroupLinkOtherGroupException()
			: base()
		{
		}
		public PriceGroupLinkOtherGroupException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
