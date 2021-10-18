using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Api
{
	public class Access : BaseAccess
	{
		public const string MariPriceManagement = "MariPriceManagement";
		public const string MariBuyer = "MariBuyer";
		public const string MariAccess = "MariAccess";
		public const string MariManager = "MariManager";
		public const string MariSeller = "MariSeller";

		public Access()
		{
			Items.Add(new ResourceAccess(MariPriceManagement, new[] { "Mr-Avalon-Resource-MariPriceManagement" }));
			Items.Add(new UserAccess(MariBuyer, UserRoles.MariBuyer));
			Items.Add(new UserAccess(MariAccess, UserRoles.MariAccess));
			Items.Add(new UserAccess(MariManager, UserRoles.MariManager));
			Items.Add(new UserAccess(MariSeller, UserRoles.MariSeller));
		}
	}
}
