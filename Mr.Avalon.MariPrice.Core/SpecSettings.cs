using Microsoft.Extensions.Configuration;
using Mr.Avalon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	public class SpecSettings
	{
		const string c_prefix = "Spec.Voc.";

		public Dictionary<string, Guid> VocIds { get; set; } = new Dictionary<string, Guid>();

		public Guid Metal { get => ExtractGuid("Metal"); }

		public Guid Quality { get => ExtractGuid("Quality"); }

		public Guid PriceClusters { get => ExtractGuid("PriceCluster"); }
		public Guid Technologies { get => ExtractGuid("Technologies"); }

		public SpecSettings Load(IConfiguration configuration)
		{
			foreach (var voc in configuration.AsEnumerable().Where(x => x.Key.StartsWith(c_prefix)))
			{
				if (Guid.TryParse(voc.Value, out var id))
				{
					var vocName = voc.Key.Replace(c_prefix, "");
					VocIds[vocName] = id;
				}
			}

			return this;
		}

		private Guid ExtractGuid(string name)
		{
			if (!VocIds.TryGetValue(name, out var id))
				throw new ConflictApiException($"There is no {c_prefix}{name} value in service configuration");
			return id;
		}
	}
}
