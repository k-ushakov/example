using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Tests
{
	[TestClass]
	public class Api_Technology_Tests
	{
		const int companyId = 1;
		Guid technologyId = Guid.NewGuid();
		Guid technologyId2 = Guid.NewGuid();

		[TestMethod]
		public void Create_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var company = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				var create = new MariPriceApi.Price.Technologies.Create
				{
					VersionId = company.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(client);

				create.Should().NotBeNull();
				create.VersionId.Should().Be(company.ActiveVersionId);
				create.TechnologyId.Should().Be(technologyId);
				create.WithNdsPrice.Should().Be(1.0M);
				create.WithoutNdsPrice.Should().Be(2.0M);

				var technologies = new MariPriceApi.Price.Technologies.List
				{
					VersionIds = new List<int>() { company.ActiveVersionId }
				}.Exec(client);

				technologies.Should().NotBeNull();
				technologies.Should().HaveCount(1);

				technologies[0].VersionId.Should().Be(company.ActiveVersionId);
				technologies[0].TechnologyId.Should().Be(technologyId);
				technologies[0].WithNdsPrice.Should().Be(1.0M);
				technologies[0].WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void Create_Technology_WithNullNds_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var company = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				var create = new MariPriceApi.Price.Technologies.Create
				{
					VersionId = company.ActiveVersionId,
					TechnologyId = technologyId,
					WithoutNdsPrice = 2.0M
				}.Exec(client);

				create.Should().NotBeNull();
				create.VersionId.Should().Be(company.ActiveVersionId);
				create.TechnologyId.Should().Be(technologyId);
				create.WithNdsPrice.Should().BeNull();
				create.WithoutNdsPrice.Should().Be(2.0M);

				var technologies = new MariPriceApi.Price.Technologies.List
				{
					VersionIds = new List<int>() { company.ActiveVersionId }
				}.Exec(client);

				technologies.Should().NotBeNull();
				technologies.Should().HaveCount(1);

				technologies[0].VersionId.Should().Be(company.ActiveVersionId);
				technologies[0].TechnologyId.Should().Be(technologyId);
				technologies[0].WithNdsPrice.Should().BeNull();
				technologies[0].WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void Create_Technology_WithoutNullNds_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var company = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				var create = new MariPriceApi.Price.Technologies.Create
				{
					VersionId = company.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M
				}.Exec(client);

				create.Should().NotBeNull();
				create.VersionId.Should().Be(company.ActiveVersionId);
				create.TechnologyId.Should().Be(technologyId);
				create.WithNdsPrice.Should().Be(1.0M);
				create.WithoutNdsPrice.Should().BeNull();

				var technologies = new MariPriceApi.Price.Technologies.List
				{
					VersionIds = new List<int>() { company.ActiveVersionId }
				}.Exec(client);

				technologies.Should().NotBeNull();
				technologies.Should().HaveCount(1);

				technologies[0].VersionId.Should().Be(company.ActiveVersionId);
				technologies[0].TechnologyId.Should().Be(technologyId);
				technologies[0].WithNdsPrice.Should().Be(1.0M);
				technologies[0].WithoutNdsPrice.Should().BeNull();
			}
		}

		[TestMethod]
		public void List_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context1 = new OrderContext().WithManager().Create(starter);
				var context2 = new OrderContext().WithManager().Create(starter);

				var company1 = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context1.ManagerCompany.CompanyId }.Exec(client);
				var company2 = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context2.ManagerCompany.CompanyId }.Exec(client);

				var create1 = new MariPriceApi.Price.Technologies.Create
				{
					VersionId = company1.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(client);

				var create2 = new MariPriceApi.Price.Technologies.Create
				{
					VersionId = company2.ActiveVersionId,
					TechnologyId = technologyId2,
					WithNdsPrice = 11.0M,
					WithoutNdsPrice = 22.0M
				}.Exec(client);

				var technologies = new MariPriceApi.Price.Technologies.List
				{
					VersionIds = new List<int>() { company1.ActiveVersionId, company2.ActiveVersionId, int.MaxValue  }
				}.Exec(client);

				technologies.Should().NotBeNull();
				technologies.Should().HaveCount(2);

				var technology1 = technologies.First(i => i.VersionId == create1.VersionId);
				technology1.VersionId.Should().Be(company1.ActiveVersionId);
				technology1.TechnologyId.Should().Be(technologyId);
				technology1.WithNdsPrice.Should().Be(1.0M);
				technology1.WithoutNdsPrice.Should().Be(2.0M);

				var technology2 = technologies.First(i => i.VersionId == create2.VersionId);
				technology2.VersionId.Should().Be(company2.ActiveVersionId);
				technology2.TechnologyId.Should().Be(technologyId2);
				technology2.WithNdsPrice.Should().Be(11.0M);
				technology2.WithoutNdsPrice.Should().Be(22.0M);

			}
		}

		[TestMethod]
		public void Update_Technology_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var company = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				var create = new MariPriceApi.Price.Technologies.Create
				{
					VersionId = company.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(client);

				var update = new MariPriceApi.Price.Technologies.Set
				{
					VersionId = company.ActiveVersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 11.0M,
					WithoutNdsPrice = 22.0M
				}.Exec(client);

				update.Should().NotBeNull();
				update.VersionId.Should().Be(company.ActiveVersionId);
				update.TechnologyId.Should().Be(technologyId);
				update.WithNdsPrice.Should().Be(11.0M);
				update.WithoutNdsPrice.Should().Be(22.0M);

				var technologies = new MariPriceApi.Price.Technologies.List
				{
					VersionIds = new List<int>() { company.ActiveVersionId }
				}.Exec(client);

				technologies.Should().NotBeNull();
				technologies.Should().HaveCount(1);

				technologies[0].VersionId.Should().Be(company.ActiveVersionId);
				technologies[0].TechnologyId.Should().Be(technologyId);
				technologies[0].WithNdsPrice.Should().Be(11.0M);
				technologies[0].WithoutNdsPrice.Should().Be(22.0M);
			}
		}

	}
}
