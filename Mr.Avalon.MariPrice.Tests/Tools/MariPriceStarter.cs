using d7k.Dto;
using Mr.Avalon.Auth.Client;
using Mr.Avalon.Common.Client;
using Mr.Avalon.Common.Core.TestTools;
using Mr.Avalon.Description.Client;
using Mr.Avalon.File.Client;
using Mr.Avalon.MariPrice.Api;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using Mr.Avalon.ProfileMari.Client;
using Mr.Avalon.Spec.Client;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Tests
{
	public class MariPriceStarter : Startup
	{
		TestApiStartup m_testApiStartup;
		public ICommonAuthenticator AccessToken { get; set; }
		public AuthApiClient Auth { get; set; }
		public ProfileApiClient Profile { get; set; }
		public SpecApiClient SpecApi { get; set; }
		public DescriptionApiClient DescriptionApi { get; set; }
		public DtoComplex Dto { get; set; }
		public string ServiceUrl { get; set; }
		public string System { get; set; }
		public SpecSettings SpecSettings { get; set; }
		public SearchSettings SearchSettings { get; set; }
		public FileApiClient Files { get; set; }

		public MariPriceStarter()
		{
			Dto = new DtoComplex().ByNestedClassesWithAttributes();
			System = "Mari";
			ServiceUrl = "http://localhost:9005";
			Auth = new AuthApiClient().Settings("##############").Build();
			AccessToken = Auth.GetServiceToken("Mr-Avalon-Test-MariPriceApi", "pwd");
			Profile = new ProfileApiClient("##############") { Authenticator = AccessToken };
			SpecApi = new SpecApiClient("##############") { Authenticator = AccessToken };
			DescriptionApi = new DescriptionApiClient("##############") { Authenticator = AccessToken };
			Files = new FileApiClient("##############") { Authenticator = AccessToken };

			SpecSettings = new SpecSettings
			{
				VocIds = new Dictionary<string, Guid>
				{
					["Metal"] = Guid.Parse("00000000-5ec2-4827-87e6-a31888b6d76e"),
					["Quality"] = Guid.Parse("00000000-5ec2-48aa-87e6-a31888b6d774"),
					["PriceCluster"] = Guid.Parse("00000000-60a6-232c-9f47-7f65bdd2701b"),
					["Technologies"] = Guid.Parse("00000000-5ec2-4796-87e6-a31888b6d765")
				}
			};

			SearchSettings = new SearchSettings
			{
				MaxCategoryReturnItems = 20,
				MaxPriceGroupsReturnItems = 100
			};

			var specEngine = new SpecEngine(SpecApi);

			Settings = new StartupSettings
			{
				AuthApi = new AuthApiClient().Settings("##############").Auth(AccessToken).Build(),
				MariSql = new SqlFactory("##############"),
				ProfileApi = Profile,
				SpecApi = SpecApi,
				DescriptionApi = DescriptionApi,
				SpecSettings = SpecSettings,
				SpecEngine = specEngine,
				SearchSettings = SearchSettings,
				Files = Files
			};

			Settings.Common.DefaultForTests();
			Settings.Common.SignKey = TestApiHelper.SignPrivateKey();

			m_testApiStartup = new TestApiStartup(ServiceUrl);
		}


		public IDisposable Start()
		{
			return m_testApiStartup.Start(this);
		}

		public MariPriceApiClient MariPriceApi(TestUserInfo user = null)
		{
			var result = new MariPriceApiClient(ServiceUrl);
			if (user == null)
				result.Authenticator = AccessToken;
			else
				result.Authenticator = Auth.GetUserToken(user.Email, user.Password);

			return result;
		}
	}
}
