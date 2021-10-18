using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Tests
{
	[TestClass]
	public class Portal_Technology_Tests
	{
		Guid technologyId = Guid.Parse("00000000-5ed0-ee91-7bdf-114bac71ab89");
		const string technologyName = "оксидирование";

		Guid technologyId2 = Guid.Parse("00000000-5ed0-ee55-7bdf-114bac71ab80");
		const string technologyName2 = "золочение";

		Guid technologyId3 = Guid.Parse("00000000-5ed0-ee40-7bdf-114bac71ab7d");
		const string technologyName3 = "штамп";

		[TestMethod]
		public void Create_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var priceCompany = context.CreatePriceCompany(starter);

				var create = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				create.Should().NotBeNull();
				create.TechnologyId.Should().Be(technologyId);
				create.Name.Should().Be(technologyName);
				create.WithNdsPrice.Should().Be(1.0M);
				create.WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void Update_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var priceCompany = context.CreatePriceCompany(starter);

				var create = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var update = new MariPriceApi.PortalPrice.TechnologiesAdditions.Update
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 11.0M,
					WithoutNdsPrice = 22.0M
				}.Exec(userClient);

				update.Should().NotBeNull();
				update.Name.Should().Be(technologyName);
				update.TechnologyId.Should().Be(technologyId);
				update.WithNdsPrice.Should().Be(11.0M);
				update.WithoutNdsPrice.Should().Be(22.0M);
			}
		}


		[TestMethod]
		public void Get_Active_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var priceCompany = context.CreatePriceCompany(starter);

				var create = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var additions = MariPriceApi.PortalPrice.TechnologiesAdditions.ExecGet(priceCompany.ActiveVersionId, userClient);

				additions.Should().NotBeNull();
				additions.Technologies.Should().NotBeNullOrEmpty();
				additions.Technologies.Should().HaveCount(1);

				var technology = additions.Technologies[0];
				technology.TechnologyId.Should().Be(technologyId);
				technology.Name.Should().Be(technologyName);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void Delete_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var priceCompany = context.CreatePriceCompany(starter);

				var createTechnology1 = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var createTechnology2 = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId2,
					WithNdsPrice = 11.0M,
					WithoutNdsPrice = 22.0M
				}.Exec(userClient);

				var createTechnology3 = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = priceCompany.ActiveVersionId,
					TechnologyId = technologyId3,
					WithNdsPrice = 111.0M,
					WithoutNdsPrice = 222.0M
				}.Exec(userClient);

				var additions = MariPriceApi.PortalPrice.TechnologiesAdditions.ExecGet(priceCompany.ActiveVersionId, userClient);

				additions.Should().NotBeNull();
				additions.Technologies.Should().NotBeNullOrEmpty();
				additions.Technologies.Should().HaveCount(3);

				new MariPriceApi.PortalPrice.TechnologiesAdditions.Delete
				{
					TechnologyId = technologyId3,
					VersionId = priceCompany.ActiveVersionId
				}.Exec(userClient);

				additions = MariPriceApi.PortalPrice.TechnologiesAdditions.ExecGet(priceCompany.ActiveVersionId, userClient);

				additions.Should().NotBeNull();
				additions.Technologies.Should().NotBeNullOrEmpty();
				additions.Technologies.Should().HaveCount(2);

				var technology = additions.Technologies.FirstOrDefault(x => x.TechnologyId == technologyId);
				technology.TechnologyId.Should().Be(technologyId);
				technology.Name.Should().Be(technologyName);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);

				var technology2 = additions.Technologies.FirstOrDefault(x => x.TechnologyId == technologyId2);
				technology2.TechnologyId.Should().Be(technologyId2);
				technology2.Name.Should().Be(technologyName2);
				technology2.WithNdsPrice.Should().Be(11.0M);
				technology2.WithoutNdsPrice.Should().Be(22.0M);

			}
		}
	}
}
