using Mr.Avalon.Spec.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Core.Other
{
	public class EventDataBulkToProductWF
	{
		public EventDataBulkToProductWF() {
			BulkId = Guid.Empty;
			ProductAndOperations = new List<ProductAndOperation>();

		}

		public Guid BulkId { get; set; }
		public int ClusterId { get; set; }
		public List<ProductAndOperation> ProductAndOperations { get; set; }
	}
}
