using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupLinkOtherGroupAndClusterException : Common.ServerErrorApiException
	{
		public PriceGroupLinkOtherGroupAndClusterException()
			: base()
		{
		}
		public PriceGroupLinkOtherGroupAndClusterException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
