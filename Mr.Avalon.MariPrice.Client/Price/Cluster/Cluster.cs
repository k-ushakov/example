using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public partial class MariPriceApi
	{
		public class EntityName<T>
		{
			public T Id { get; set; }
			public string Name { get; set; }

			public EntityName() { }
			public EntityName(T id, string name)
			{
				Id = id;
				Name = name;
			}
		}

		public partial class Price
		{
			public partial class Cluster
			{
				public int Id { get; set; }
				public int VersionId { get; set; }

				public bool Enabled { get; set; }
				public bool InStock { get; set; }
				public bool InOrder { get; set; }

				public Guid Name { get; set; }
				public Guid? Metal { get; set; }
				public Guid? Quality { get; set; }

				public List<ClusterSetting> VariantsSettings { get; set; } = new List<ClusterSetting>();
			}
		}
	}
}
