using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Exception
{
	public class ClusterSaleSchemeException : Common.ServerErrorApiException
	{
		public ClusterSaleSchemeException()
			: base()
		{
		}
		public ClusterSaleSchemeException(string message, HttpStatusCode statusCode)
			: base(message, statusCode)
		{

		}


	}
}
