using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Mr.Avalon.Auth.Client;
using Mr.Avalon.Common.Client;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.Description.Client;
using Mr.Avalon.Event.Client;
using Mr.Avalon.File.Client;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using Mr.Avalon.Print.Client;
using Mr.Avalon.ProfileMari.Client;
using Mr.Avalon.Spec.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Utilities;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Api
{
	public class StartupSettings : IAllCommonSettingsSource
	{
		public AllCommonSettings Common { get; set; } = new AllCommonSettings();

		public ISqlFactory MariSql { get; set; }
		public AuthApiClient AuthApi { get; set; }
		public ProfileApiClient ProfileApi { get; set; }
		public DescriptionApiClient DescriptionApi { get; set; }
		public SpecApiClient SpecApi { get; set; }
		public IBarcodeStorage BarcodeStorage { get; set; }
		public SpecSettings SpecSettings { get; set; }
		public SearchSettings SearchSettings { get; set; }
		public BulkPublishSettings BulkPublishSettings { get; set; }
		public ISpecEngine SpecEngine { get; set; }
		public FileApiClient Files { get; set; }
		public PrintApiClient PrintApi { get; set; }

		public String DefaultImageUrl { get; set; }

		public IEventSenderClient EventsApi { get; set; }

		public StartupSettings LoadFromConfig(IConfiguration configuration)
		{
			MariSql = new SqlFactory(configuration["Common.Storage"]);
			var system = configuration["Common.System"];
			var authUrl = configuration["Auth.Url"];
			AuthApi = new AuthApiClient().Settings(authUrl).Timeout(TimeSpan.FromDays(1)).HttpSingleHandler().Build();
			var token = AuthApi.GetServiceToken(configuration["Auth.User"], configuration["Auth.Password"]);

			ProfileApi = new ProfileApiClient(configuration["Common.Profile.Url"]) { Authenticator = token };
			SpecApi = new SpecApiClient(configuration["Common.Spec.Url"]) { Authenticator = token };
			DescriptionApi = new DescriptionApiClient(configuration["Common.Description.Url"]) { Authenticator = token };
			Files = new FileApiClient(configuration["Common.Files.Url"]) { Authenticator = token };
			PrintApi = new PrintApiClient(configuration["Common.Print.Url"]) { Authenticator = token };
			EventsApi = new EventApiClient().Settings(configuration["Common.Events.Url"]).System(system).Auth(token).Build();

			SpecEngine = new SpecEngine(SpecApi);

			var cloudStorage = CloudStorageAccount.Parse(configuration["Common.BlobStorage"]);
			var cloudTableClient = cloudStorage.CreateCloudTableClient();

			var barcodesTable = cloudTableClient.GetTableReference(configuration["MariPrice.BarcodeTable"]);
			barcodesTable.CreateIfNotExistsAsync().Wait();
			BarcodeStorage = new BarcodeStorage(barcodesTable);

			SpecSettings = new SpecSettings().Load(configuration);
			SearchSettings = new SearchSettings().Load(configuration);
			BulkPublishSettings = new BulkPublishSettings().Load(configuration);
			var m_values = new NameValueCollection().FromConfig(configuration);
			DefaultImageUrl = configuration["DefaultImageUrl"];

			return this;
		}
	}
}
