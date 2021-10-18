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
				public class Create
				{
					public int VersionId { get; set; }

					public Guid TechnologyId { get; set; }
					public decimal? WithNdsPrice { get; set; }
					public decimal? WithoutNdsPrice { get; set; }

					public Technologies Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/technologies/create").Body(this);

						return api.Execute<Technologies>(request);
					}
				}
			}
		}
	}
}
