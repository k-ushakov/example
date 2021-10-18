using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client.Portal
{
	public class BulkPublishInformationByCluster
	{
		public int ClusterId { get; set; }
		public int AllProduct { get; set; }
		public int CompleteProduct { get; set; }
		public int ErrorProduct { get; set; }
		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
	}
}
