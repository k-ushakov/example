using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	public class BulkPublishSettings
	{
		public int MaxCount { get; set; }

		public BulkPublishSettings Load(IConfiguration configuration)
		{
			if (!int.TryParse(configuration["MariPrice.BulkPublish.MaxCount"], out var val))
				val = 100;
			MaxCount = val;

			return this;
		}
	}
}
