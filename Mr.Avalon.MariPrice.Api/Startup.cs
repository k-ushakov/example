using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Core;

namespace Mr.Avalon.MariPrice.Api
{
	public class Startup : BaseStartup<StartupSettings>
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			services.UseAllCommonSettings(Settings.Common, new Access());

			var dto = Settings.Common.Injections.Dto;
			var priceEngine = new PriceEngine(Settings.MariSql, dto, Settings.SpecEngine, Settings.SpecSettings, Settings.DescriptionApi, Settings.PrintApi);
			services.AddSingleton(priceEngine);

			var portalPrieEngine = new PortalPriceEngine(dto, priceEngine, Settings.ProfileApi, Settings.SpecApi, Settings.SpecEngine, Settings.SpecSettings, Settings.SearchSettings, Settings.MariSql, Settings.BarcodeStorage, Settings.DefaultImageUrl, Settings.Files, Settings.PrintApi, Settings.EventsApi,Settings.BulkPublishSettings);
			services.AddSingleton(portalPrieEngine);
		}

		public override StartupSettings LoadConfiguration(IConfiguration configuration)
		{
			var setting = base.LoadConfiguration(configuration);

			return setting.LoadFromConfig(configuration);
		}
	}
}
