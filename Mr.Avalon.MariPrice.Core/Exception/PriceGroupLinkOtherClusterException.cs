using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class PriceGroupLinkOtherClusterException : Common.ServerErrorApiException
	{
		public PriceGroupLinkOtherClusterException()
			: base()
		{
		}
		public PriceGroupLinkOtherClusterException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
