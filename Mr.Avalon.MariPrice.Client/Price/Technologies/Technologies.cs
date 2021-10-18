using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public partial class Technologies
			{
				public int VersionId { get; set; }
				public Guid TechnologyId { get; set; }
				public decimal? WithNdsPrice { get; set; }
				public decimal? WithoutNdsPrice { get; set; }

				public class TechnologyPortal
				{
					public Guid TechnologyId { get; set; }
					public string Name { get; set; }

					public decimal? WithNdsPrice { get; set; }
					public decimal? WithoutNdsPrice { get; set; }
				}

				public class TechnologyApi
				{
					public Guid TechnologyId { get; set; }
					public decimal? WithNdsPrice { get; set; }
					public decimal? WithoutNdsPrice { get; set; }
				}
			}
		}
	}
}
