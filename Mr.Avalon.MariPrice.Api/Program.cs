using Mr.Avalon.Common.Core.Api;
using System;

namespace Mr.Avalon.MariPrice.Api
{
	class Program
	{
		static void Main(string[] args)
		{
			ConsoleStarter.Start<Startup, StartupSettings>("http://localhost:9008/", args);
		}
	}
}
