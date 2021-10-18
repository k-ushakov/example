using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace Mr.Avalon.MariPrice.Core
{
	public class BarcodeTableEntity : TableEntity
	{
		public Guid DownloadSessionId { get; set; }
		public int SaleCompany { get; set; }
		public string ProductPn { get; set; }
		public string SizePn { get; set; }
		public string Size { get; set; }
		public decimal? WireThickness { get; set; }
		public string Barcode { get; set; }
		public decimal Weight { get; set; }
		
		public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
		{
			base.ReadEntity(properties, operationContext);

			if (properties.TryGetValue(nameof(WireThickness), out EntityProperty property) && !string.IsNullOrWhiteSpace(property.StringValue))
			{
				if (decimal.TryParse(property.StringValue, out decimal azureDecimal))
				{
					WireThickness = azureDecimal;
				}
			}

			if (properties.TryGetValue(nameof(Weight), out property) && !string.IsNullOrWhiteSpace(property.StringValue))
			{
				if (decimal.TryParse(property.StringValue, out decimal azureDecimal))
				{
					Weight = azureDecimal;
				}
			}
		}

		public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
		{
			throw new NotImplementedException();
		}
	}
}
