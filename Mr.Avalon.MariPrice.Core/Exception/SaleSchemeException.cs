using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class SaleSchemeException : Common.ServerErrorApiException
	{
		public SaleSchemeException()
			: base()
		{
		}
		public SaleSchemeException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
