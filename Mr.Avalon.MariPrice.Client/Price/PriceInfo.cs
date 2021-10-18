using Mr.Avalon.Common.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public partial class Price
		{
			public Guid ProductId { get; set; }

			public List<ManufacturingOption> MainManufacturingOptions { get; set; } = new List<ManufacturingOption>();
			public Dictionary<Guid, List<ManufacturingOption>> SizeOwnManufacturingOptions { get; set; }
				= new Dictionary<Guid, List<ManufacturingOption>>();
			public Dictionary<Guid, Technologies.TechnologyApi> TechnologiesAdditionalPrices { get; set; }
				= new Dictionary<Guid, Technologies.TechnologyApi>();
			public EntityName<int> PriceCluster { get; set; }

			public InstockInfo ProductInstockInfo { get; set; } = new InstockInfo();

			public class InstockInfo
			{
				public InStockItems MainInstockItems { get; set; } = new InStockItems();
				public Dictionary<Guid, InStockItems> SizeInstockItems { get; set; } = new Dictionary<Guid, InStockItems>();
			}

			public class InStockItems
			{
				public decimal? WithNdsBuyPrice { get; set; }
				public decimal? WithNdsMarkup { get; set; }

				public decimal? WithoutNdsBuyPrice { get; set; }
				public decimal? WithoutNdsMarkup { get; set; }

				[Obsolete("Need to use Products field")]
				public List<string> Barcodes { get; set; }
				public int Total { get; set; }
				public List<InstockShort> Products { get; set; }
			}

			public class ManufacturingOption
			{
				public decimal? OrderMetalWeight { get; set; }				
				public int? ProductionTime { get; set; }
				public decimal? CostForGramm { get; set; }
				public decimal? CostForGrammWithMarkup { get; set; }
				public bool? WithNds { get; set; }

				public decimal? LossPercentage { get; set; }
				public decimal? AffinageLossPercentage { get; set; }
				public decimal? TotalLossPercentage { get; set; }

			}

			public class AggregatePrice
			{
				public Dictionary<Guid, Price> Items { get; set; } = new Dictionary<Guid, Price>();

				public class Request
				{
					public List<Guid> ProductUids { get; set; }

					public AggregatePrice Exec(MariPriceApiClient api)
					{
						var request = api.PostRequest("price/aggregate")
							.Body(this);

						return api.Execute<AggregatePrice>(request);
					}
				}
			}
		}
	}
}
