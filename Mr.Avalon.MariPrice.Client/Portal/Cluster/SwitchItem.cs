using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{ 
	public partial class MariPriceApi
	{
		public partial class PortalPrice
		{
			public partial class Cluster
			{
				public class SwitchItem
				{
					public int ClusterId { get; set; }
					public Guid SettingsVariantId { get; set; }
					public bool Enable { get; set; }
				}
			}
		}
	}
}
