using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	public class SearchSettings
	{
		public int MaxCategoryReturnItems { get; set; }
		public int MaxPriceGroupsReturnItems { get; set; }

		public SearchSettings Load(IConfiguration configuration)
		{
			if (!int.TryParse(configuration["CategoryPotentialParentsReturnMaxItems"], out var val))
				val = 20;
			MaxCategoryReturnItems = val;

			if (!int.TryParse(configuration["Price.PriceGroups.MaxCount"], out val))
				val = 100;
			MaxPriceGroupsReturnItems = val;

			return this;
		}
	}
}
