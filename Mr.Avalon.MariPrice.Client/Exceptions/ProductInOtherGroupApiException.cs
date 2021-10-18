using Mr.Avalon.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public class ProductInOtherClusterApiException : BadRequestApiException
	{
		public ProductInOtherClusterApiException() : base() { }
		public ProductInOtherClusterApiException(string message) : base(message) { }
	}

	public class ProductInOtherGroupApiException : BadRequestApiException
	{
		public ProductInOtherGroupApiException() : base() { }
		public ProductInOtherGroupApiException(string message) : base(message) { }
	}

	public class ProductNotInDraftVersionApiException : BadRequestApiException
	{
		public ProductNotInDraftVersionApiException() : base() { }
		public ProductNotInDraftVersionApiException(string message) : base(message) { }
	}
}
