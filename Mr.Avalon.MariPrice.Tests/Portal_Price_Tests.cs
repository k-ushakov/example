using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using Mr.Avalon.MariPrice.Core.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Tests
{
	[TestClass]
	public class Portal_Price_Tests
	{
		Guid technologyId = Guid.Parse("00000000-5ed0-ee91-7bdf-114bac71ab89");
		
		Guid metalGold = Guid.Parse("00000000-5ee7-6147-d3ab-eb1c1c5f7fc2");
		Guid metalSilver = Guid.Parse("00000000-5ee7-614f-d3ab-eb1c1c5f7fc5");

		Guid quality900 = Guid.Parse("00000000-5ee7-68bf-d3ab-eb1c1c5f8008");
		Guid quality585 = Guid.Parse("00000000-5ee7-66a8-d3ab-eb1c1c5f7ff6");

		[TestMethod]
		public void CreateVersion_When_GetCompany_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetActive(userClient, company.CompanyId);

				currentVersion.VersionId.Should().BeGreaterThan(0);
			}

		}

		[TestMethod]
		public void UpdateVersion_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetActive(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = currentVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = false;

				var price = update.Exec(userClient);

				price.WithNdsPriceRequired.Should().BeTrue();
				price.WithoutNdsPriceRequired.Should().BeFalse();
			}
		}

		[TestMethod]
		public void CreateCluster_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var clusterInfoAll = clusterSettings.Exec(userClient);
				clusterInfoAll.ManufactureSettings.Enabled.Should().BeTrue();
				clusterInfoAll.ManufactureSettings.Variants.Should().HaveCount(1);
				clusterInfoAll.ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				clusterInfoAll.ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
				clusterInfoAll.ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);

				var createGroup = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				createGroup.Id.Should().BeGreaterThan(0);
				createGroup.Name.Should().Be("GroupName");
				createGroup.DisplayName.Should().Be("GroupName");
				createGroup.LossPercentage.Should().Be(10);
				createGroup.Values.Should().HaveCount(1);
				createGroup.Values[0].PriceGroupValueId.Should().BeGreaterThan(0);
				createGroup.Values[0].PriceClusterVariantId.Should().Be(clusterInfoAll.ManufactureSettings.Variants[0].Id);
				createGroup.Values[0].WithNdsPrice.Should().BeNull();
				createGroup.Values[0].WithoutNdsPrice.Should().BeNull();

				var updateGroupValue = new MariPriceApi.PortalPrice.GroupValue.Update()
				{
					PriceGroupValueId = createGroup.Values[0].PriceGroupValueId,
					WithNdsPrice = 5,
					WithoutNdsPrice = 15
				}.Exec(userClient);

				currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				currentVersion.Clusters.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);

				currentVersion.Clusters[0].PriceGroups.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups[0].LossPercentage.Should().Be(10);
				currentVersion.Clusters[0].PriceGroups[0].Name.Should().Be("GroupName");
				currentVersion.Clusters[0].PriceGroups[0].DisplayName.Should().Be("GroupName");

				currentVersion.Clusters[0].PriceGroups[0].Values.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups[0].Values[0].PriceGroupValueId.Should().Be(createGroup.Values[0].PriceGroupValueId);
				currentVersion.Clusters[0].PriceGroups[0].Values[0].WithNdsPrice.Should().Be(5);
				currentVersion.Clusters[0].PriceGroups[0].Values[0].WithoutNdsPrice.Should().Be(15);
			}
		}

		[TestMethod]
		public void DeleteClusterSetting_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var clusterInfoAll = clusterSettings.Exec(userClient);
				clusterInfoAll.ManufactureSettings.Variants.Should().HaveCount(1);
				clusterInfoAll.ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				clusterInfoAll.ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
				clusterInfoAll.ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);

				var clusterSettingsDelete = new MariPriceApi.PortalPrice.Cluster.DeleteItem()
				{
					ClusterId = cluster.Id,
					SettingsVariantId = clusterInfoAll.ManufactureSettings.Variants[0].Id
				};

				var createGroup = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				var res = clusterSettingsDelete.Exec(userClient);
				res.Variants.Should().HaveCount(0);
			}
		}

		[TestMethod]
		public void DeleteCluster_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(2, starter.SpecSettings.PriceClusters, specClient);

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[0].Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				}.Exec(userClient).ManufactureSettings.Variants.FirstOrDefault();

				var cluster1 = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[1].Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalSilver,
					ClusterQuality = quality585
				}.Exec(userClient);

				var clusterSettings1 = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster1.Id,
					OrderMetalWeight = 10,
					ProductionTime = 12
				}.Exec(userClient).ManufactureSettings.Variants.FirstOrDefault();

				var createGroup = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);
				currentVersion.Clusters.Should().HaveCount(2);

				var deleteCluster = new MariPriceApi.PortalPrice.Cluster.Delete() { ClusterId = cluster1.Id }.Exec(userClient);

				currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);
				currentVersion.Clusters.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Id.Should().Be(cluster.Id);
				currentVersion.Clusters[0].ManufactureSettings.ClusterName.Id.Should().Be(clusterName[0].Id);
				currentVersion.Clusters[0].ManufactureSettings.ClusterName.Name.Should().Be(clusterName[0].Value);
				currentVersion.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].Id.Should().Be(clusterSettings.Id);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
			}
		}

		[TestMethod]
		public void GetGroup()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var clusterInfoAll = clusterSettings.Exec(userClient);
				clusterInfoAll.ManufactureSettings.Variants.Should().HaveCount(1);
				clusterInfoAll.ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				clusterInfoAll.ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
				clusterInfoAll.ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);

				var createGroup = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10
				}.Exec(userClient);

				var updateGroupValue = new MariPriceApi.PortalPrice.GroupValue.Update()
				{
					PriceGroupValueId = createGroup.Values[0].PriceGroupValueId,
					WithNdsPrice = 5,
					WithoutNdsPrice = 15
				}.Exec(userClient);

				var group = MariPriceApi.PortalPrice.Group.Exec(userClient, createGroup.Id);

				group.LossPercentage.Should().Be(10);
				group.Name.Should().Be("GroupName");
				group.DisplayName.Should().Be("GroupName");
			}
		}

		[TestMethod]
		public void DeleteGroup()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var clusterInfoAll = clusterSettings.Exec(userClient);
				clusterInfoAll.ManufactureSettings.Variants.Should().HaveCount(1);
				clusterInfoAll.ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				clusterInfoAll.ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
				clusterInfoAll.ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);

				var createGroup = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName1",
					DisplayName = "DisplayName1",
					LossPercentage = 10,

				}.Exec(userClient);

				var createGroup1 = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName2",
					DisplayName = "DisplayName2",
					LossPercentage = 20,

				}.Exec(userClient);

				var draft = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				draft.Clusters.Should().HaveCount(1);
				draft.Clusters[0].PriceGroups.Should().HaveCount(2);

				var f1 = draft.Clusters[0].PriceGroups.Find(x => x.Id == createGroup.Id);
				f1.Name.Should().Be("GroupName1");
				f1.DisplayName.Should().Be("GroupName1");
				f1.LossPercentage.Should().Be(10);

				var f2 = draft.Clusters[0].PriceGroups.Find(x => x.Id == createGroup1.Id);
				f2.Name.Should().Be("GroupName2");
				f2.DisplayName.Should().Be("GroupName2");
				f2.LossPercentage.Should().Be(20);

				var delete = new MariPriceApi.PortalPrice.Group.Delete() { PriceGroupId = createGroup.Id }.Exec(userClient);
				delete.PriceGroups.Should().HaveCount(1);
				delete.PriceGroups[0].Id.Should().Be(createGroup1.Id);
			}
		}

		[TestMethod]
		public void PriceCluster_SerachProducts_LinkExist()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var group = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				var settings = MariPriceApi.PortalPrice.Cluster.ExecGet(company.CompanyId, cluster.Id, userClient).ManufactureSettings;


				var productUid1 = ObjectId.GenerateNewId().GetGuid();
				var productUid2 = ObjectId.GenerateNewId().GetGuid();

				var syncItem_1_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					Title = "Test",
					ProductUid = productUid1
				};
				var syncItem_1_1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test2",
					SizeUid = Guid.NewGuid(),
					ProductUid = productUid1
				};
				var syncItem_2_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test3",
					ProductUid = productUid2
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem_1_0, syncItem_1_1, syncItem_2_0
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId
				};

				var products = searchRequest.Exec(userClient);
				var target = products.Items.Single(x => x.Name == syncItem_1_0.Name);

				new MariPriceApi.PortalPrice.Product.Create
				{
					PriceGroupId = group.Id,
					Id = target.Id,
				}.Exec(userClient);

				products = searchRequest.Exec(userClient);
				products.Items.Should().HaveCount(2);
				var p1 = products.Items.Single(p => p.ProductUid == syncItem_1_0.ProductUid);
				var p2 = products.Items.Single(p => p.ProductUid == syncItem_2_0.ProductUid);
				p1.Id.Should().BeGreaterThan(0);
				p1.Name.Should().Be(syncItem_1_0.Name);
				p1.ClusterId.Should().Be(cluster.Id);
				p2.Id.Should().BeGreaterThan(0);
				p2.Name.Should().Be(syncItem_2_0.Name);
				p2.ClusterId.Should().BeNull();

				new MariPriceApi.PortalPrice.Product.Delete
				{
					PriceGroupId = group.Id,
					Id = target.Id,
				}.Exec(userClient);

				products = searchRequest.Exec(userClient);
				products.Items.Should().HaveCount(2);
			}
		}


		[TestMethod]
		public void PriceCluster_SerachProductsCurrentGroup()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var group = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				var settings = MariPriceApi.PortalPrice.Cluster.ExecGet(company.CompanyId, cluster.Id, userClient).ManufactureSettings;


				var productUid1 = ObjectId.GenerateNewId().GetGuid();
				var productUid2 = ObjectId.GenerateNewId().GetGuid();

				var syncItem_1_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					Title = "Test",
					ProductUid = productUid1
				};
				var syncItem_1_1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test2",
					SizeUid = Guid.NewGuid(),
					ProductUid = productUid1
				};
				var syncItem_2_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test3",
					ProductUid = productUid2
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem_1_0, syncItem_1_1, syncItem_2_0
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId
				};

				var products = searchRequest.Exec(userClient);
				var target = products.Items.Single(x => x.Name == syncItem_1_0.Name);

				new MariPriceApi.PortalPrice.Product.Create
				{
					PriceGroupId = group.Id,
					Id = target.Id
				}.Exec(userClient);

				products = searchRequest.Exec(userClient);
				products.Items.Should().HaveCount(2);
				var p1 = products.Items.Single(p => p.ProductUid == syncItem_1_0.ProductUid);
				var p2 = products.Items.Single(p => p.ProductUid == syncItem_2_0.ProductUid);
				p1.Id.Should().BeGreaterThan(0);
				p1.Name.Should().Be(syncItem_1_0.Name);
				p1.ClusterId.Should().Be(cluster.Id);
				p2.Id.Should().BeGreaterThan(0);
				p2.Name.Should().Be(syncItem_2_0.Name);
				p2.ClusterId.Should().BeNull();

				searchRequest.GroupId = group.Id;
				searchRequest.Filter = new MariPriceApi.PortalPrice.Product.List.FilterInfo();
				searchRequest.Filter.OnlyGroupProduct = true;

				products = searchRequest.Exec(userClient);
				products.Items.Should().HaveCount(1);
			}
		}

		[TestMethod]
		public void PriceCluster_SerachProducts()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var syncItem1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = Guid.NewGuid()
				};
				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test2",
					ProductUid = Guid.NewGuid()
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem1, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId
				};

				var products = searchRequest.Exec(userClient);

				products.Items.Should().HaveCount(2);
				var p1 = products.Items.Single(p => p.ProductUid == syncItem1.ProductUid);
				var p2 = products.Items.Single(p => p.ProductUid == syncItem2.ProductUid);
				p1.Id.Should().BeGreaterThan(0);
				p1.Name.Should().Be(syncItem1.Name);
				p1.ClusterId.Should().BeNull();
				p2.Id.Should().BeGreaterThan(0);
				p2.Name.Should().Be(syncItem2.Name);
				p2.ClusterId.Should().BeNull();
			}
		}

		[TestMethod]
		public void PriceCluster_AddProduct()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var group = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				var settings = MariPriceApi.PortalPrice.Cluster.ExecGet(company.CompanyId, cluster.Id, userClient).ManufactureSettings;

				var productUid1 = Guid.NewGuid();
				var productUid2 = Guid.NewGuid();

				var syncItem_1_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid1
				};
				var syncItem_1_1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test2",
					SizeUid = Guid.NewGuid(),
					ProductUid = productUid1
				};
				var syncItem_2_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test3",
					ProductUid = productUid2
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem_1_0, syncItem_1_1,syncItem_2_0
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId
				};

				var products = searchRequest.Exec(userClient);
				products.Total.Should().Be(2);
				var target = products.Items.Single(x => x.Name == syncItem_1_0.Name).Id;

				new MariPriceApi.PortalPrice.Product.Create
				{
					PriceGroupId = group.Id,
					Id = target
				}.Exec(userClient);

				searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId,
					GroupId = group.Id
				};

				products = searchRequest.Exec(userClient);
				products.Total.Should().Be(2);
				products.Items.Should().HaveCount(2);

				var pr1 = products.Items.Single(x => x.Name == syncItem_1_0.Name);
				pr1.Include.Should().BeTrue();
				pr1.Sizes.Should().HaveCount(1);


				currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);
				currentVersion.Clusters.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups[0].ProductPublishInfo.Should().Be("Опубликовано 0 из 1");

			}
		}

		[TestMethod]
		public void PriceCluster_UpdateVariants()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				}.Exec(userClient);

				clusterSettings.ManufactureSettings.Variants.Should().HaveCount(1);
				clusterSettings.ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				clusterSettings.ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
				clusterSettings.ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);

				var clusterSettingsUpdate = new MariPriceApi.PortalPrice.Cluster.UpdateItem()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 10,
					ProductionTime = 11,
					SettingsVariantId = clusterSettings.ManufactureSettings.Variants[0].Id
				}.Exec(userClient);

				clusterSettingsUpdate.ProductionTime.Should().Be(11);
				clusterSettingsUpdate.OrderMetalWeight.Should().Be(10);
			}
		}

		[TestMethod]
		public void PriceCluster_UpdateGroup()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var group = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "Название",
					DisplayName = "Название 2",
					LossPercentage = 10,
					AdditionalLossPercentage = 20,

				}.Exec(userClient);

				group.Name.Should().Be("Название");
				group.DisplayName.Should().Be("Название");
				group.LossPercentage.Should().Be(10);
				group.AdditionalLossPercentage.Should().Be(20);

				/*((1+% потерь на производство/100) (1 +% потерь при передаче лома/100)-1)*100.*/
				var totalLost = ((1 + (10 / 100.0M)) * (1 + (20 / 100.0M)) - 1) * 100;
				group.TotalLossPercentage.Should().Be(totalLost);

				var updateGroup = new MariPriceApi.PortalPrice.Group.Update()
				{
					Id = group.Id,
					Name = "Название2",
					DisplayName = "Название3",
					LossPercentage = 44,
					AdditionalLossPercentage = 33

				}.Exec(userClient);

				updateGroup.Name.Should().Be("Название2");
				updateGroup.DisplayName.Should().Be("Название2");
				updateGroup.LossPercentage.Should().Be(44);
				totalLost = ((1 + (44 / 100.0M)) * (1 + (33 / 100.0M)) - 1) * 100;
				updateGroup.TotalLossPercentage.Should().Be(totalLost);
				updateGroup.AdditionalLossPercentage.Should().Be(33);
			}
		}

		[TestMethod]
		public void PriceCluster_SerachProducts_Filters()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, client).Single();

				var syncItem1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = Guid.NewGuid()
				};
				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test2",
					ProductUid = Guid.NewGuid()
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem1, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId
				};

				var products = searchRequest.Exec(userClient);

				products.Items.Should().HaveCount(2);
				var p1 = products.Items.Single(p => p.ProductUid == syncItem1.ProductUid);
				var p2 = products.Items.Single(p => p.ProductUid == syncItem2.ProductUid);
				p1.Id.Should().BeGreaterThan(0);
				p1.Name.Should().Be(syncItem1.Name);
				p1.ClusterId.Should().BeNull();
				p2.Id.Should().BeGreaterThan(0);
				p2.Name.Should().Be(syncItem2.Name);
				p2.ClusterId.Should().BeNull();
			}
		}


		[TestMethod]
		public void Cluster_GroupValue_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "DisplayName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);

				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				currentVersion.Clusters.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);

				currentVersion.Clusters[0].PriceGroups.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups[0].LossPercentage.Should().Be(10);
				currentVersion.Clusters[0].PriceGroups[0].Name.Should().Be("GroupName");
				currentVersion.Clusters[0].PriceGroups[0].DisplayName.Should().Be("GroupName");

				currentVersion.Clusters[0].PriceGroups[0].Values.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups[0].Values[0].PriceGroupValueId.Should().Be(createGroup.Values[0].PriceGroupValueId);
				currentVersion.Clusters[0].PriceGroups[0].Values[0].WithNdsPrice.Should().Be(5);
				currentVersion.Clusters[0].PriceGroups[0].Values[0].WithoutNdsPrice.Should().Be(15);
			}

		}

		[TestMethod]
		public void PriceCluster_Swap()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var updateVersion = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update()
				{
					VersionId = draftVersion.VersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(userClient);

				var createTechnology = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = draftVersion.VersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var updateFlag = new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster.Id, InOrder = true }.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "GroupName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);


				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithNdsMarkup = 5;
				updateGroupValueRequest.WithoutNdsMarkup = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);
				var active = MariPriceApi.PortalPrice.GetActive(userClient, company.CompanyId);
				active.Clusters.Should().HaveCount(0);

				draftVersion.Clusters.Should().HaveCount(1);
				draftVersion.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				draftVersion.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				draftVersion.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);

				draftVersion.Clusters[0].PriceGroups.Should().HaveCount(1);
				draftVersion.Clusters[0].PriceGroups[0].LossPercentage.Should().Be(10);
				draftVersion.Clusters[0].PriceGroups[0].Name.Should().Be("GroupName");
				draftVersion.Clusters[0].PriceGroups[0].DisplayName.Should().Be("GroupName");

				draftVersion.Clusters[0].PriceGroups[0].Values.Should().HaveCount(1);
				draftVersion.Clusters[0].PriceGroups[0].Values[0].PriceGroupValueId.Should().Be(createGroup.Values[0].PriceGroupValueId);
				draftVersion.Clusters[0].PriceGroups[0].Values[0].WithNdsPrice.Should().Be(5);
				draftVersion.Clusters[0].PriceGroups[0].Values[0].WithoutNdsPrice.Should().Be(15);

				draftVersion.Technologies.Should().NotBeNull();
				draftVersion.Technologies.Should().HaveCount(1);

				var technology = draftVersion.Technologies[0];
				technology.TechnologyId.Should().Be(technologyId);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);
				swap.Clusters.Should().HaveCount(1);
				swap.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				swap.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				swap.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);

				swap.Clusters[0].PriceGroups.Should().HaveCount(1);
				swap.Clusters[0].PriceGroups[0].LossPercentage.Should().Be(10);
				swap.Clusters[0].PriceGroups[0].Name.Should().Be("GroupName");
				swap.Clusters[0].PriceGroups[0].DisplayName.Should().Be("GroupName");

				swap.Clusters[0].PriceGroups[0].Values.Should().HaveCount(1);
				swap.Clusters[0].PriceGroups[0].Values[0].PriceGroupValueId.Should().Be(createGroup.Values[0].PriceGroupValueId);
				swap.Clusters[0].PriceGroups[0].Values[0].WithNdsPrice.Should().Be(5);
				swap.Clusters[0].PriceGroups[0].Values[0].WithoutNdsPrice.Should().Be(15);

				swap.Technologies.Should().NotBeNull();
				swap.Technologies.Should().HaveCount(1);

				technology = swap.Technologies[0];
				technology.TechnologyId.Should().Be(technologyId);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);

				draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);
				draftVersion.Clusters.Should().HaveCount(0);
				draftVersion.Technologies.Should().BeNullOrEmpty();

			}
		}

		[TestMethod]
		public void PriceCluster_Swap_WithoutVariantSettings()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(2, starter.SpecSettings.PriceClusters, specClient);

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[0].Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "DisplayName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);

				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithNdsMarkup = 5;
				updateGroupValueRequest.WithoutNdsMarkup = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[1].Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId };

				//			AssertionExtensions.Should(() => swap.Exec(userClient)).Throw<ValidationApiException>();
				AssertionExtensions.Should(() => swap.Exec(userClient)).Throw<SaleSchemeException>();

			}
		}

		[TestMethod]
		public void PriceCluster_LoadActive()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = false;

				update.Exec(userClient);

				new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = draftVersion.VersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var updateFlag = new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster.Id, InOrder = true }.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "GroupName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);

				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithoutNdsMarkup = 15;
				updateGroupValueRequest.WithNdsMarkup = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);
				swap.Clusters.Should().HaveCount(1);

				draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);
				draftVersion.Clusters.Should().HaveCount(0);

				var loadactive = new MariPriceApi.PortalPrice.PriceCompany.LoadActiveVersion() { CompanyId = company.CompanyId }.Exec(userClient);
				//loadactive.VersionId.Should().Be(draftVersion.VersionId);

				loadactive.WithNdsPriceRequired.Should().BeTrue();
				loadactive.WithoutNdsPriceRequired.Should().BeFalse();

				loadactive.Clusters.Should().HaveCount(1);
				loadactive.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				loadactive.Clusters[0].ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				loadactive.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				loadactive.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);

				loadactive.Clusters[0].PriceGroups.Should().HaveCount(1);
				loadactive.Clusters[0].PriceGroups[0].LossPercentage.Should().Be(10);
				loadactive.Clusters[0].PriceGroups[0].Name.Should().Be("GroupName");
				loadactive.Clusters[0].PriceGroups[0].DisplayName.Should().Be("GroupName");

				loadactive.Clusters[0].PriceGroups[0].Values.Should().HaveCount(1);
				loadactive.Clusters[0].PriceGroups[0].Values[0].PriceGroupValueId.Should().BeGreaterThan(0);
				loadactive.Clusters[0].PriceGroups[0].Values[0].PriceClusterVariantId.Should().Be(loadactive.Clusters[0].ManufactureSettings.Variants[0].Id);
				loadactive.Clusters[0].PriceGroups[0].Values[0].WithNdsPrice.Should().Be(5);
				loadactive.Clusters[0].PriceGroups[0].Values[0].WithoutNdsPrice.Should().Be(15);

				loadactive.Technologies.Should().NotBeNull();
				loadactive.Technologies.Should().HaveCount(1);

				var technology = loadactive.Technologies[0];
				technology.TechnologyId.Should().Be(technologyId);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void GetAggregate_PriceContent_SizesExist_Information()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var createTechnology = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = draftVersion.VersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = true;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var updateFlag = new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster.Id, InOrder = true }.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "DisplayName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);

				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithoutNdsMarkup = 15;
				updateGroupValueRequest.WithNdsMarkup = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);
				swap.Clusters.Should().HaveCount(1);

				var productUid = Guid.NewGuid();
				var syncItem = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid
				};

				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -company.CompanyId,
					Enabled = true,
					Name = "Test",
					SizeUid = Guid.NewGuid(),

					ProductUid = productUid
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var products = new MariPriceApi.Price.Product.List
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = products[0].Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = products[1].Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				var priceAggregate = new MariPriceApi.Price.AggregatePrice.Request
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				priceAggregate.Items.Should().HaveCount(1);
				var productPrice = priceAggregate.Items[productUid];
				productPrice.PriceCluster.Id.Should().Be(cluster.Id);
				productPrice.PriceCluster.Name.Should().Be(clusterName.Value);
				productPrice.MainManufacturingOptions.Should().HaveCount(2);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).CostForGramm.Should().Be(updateGroupValueRequest.WithNdsPrice);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).CostForGramm.Should().Be(updateGroupValueRequest.WithoutNdsPrice);

				productPrice.SizeOwnManufacturingOptions.Should().HaveCount(1);
				var sizeOptions = productPrice.SizeOwnManufacturingOptions[syncItem2.SizeUid];
				sizeOptions.Should().HaveCount(2);
				sizeOptions.Single(x => x.WithNds == true).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				sizeOptions.Single(x => x.WithNds == true).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				sizeOptions.Single(x => x.WithNds == true).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				sizeOptions.Single(x => x.WithNds == true).CostForGramm.Should().Be(updateGroupValueRequest.WithNdsPrice);
				sizeOptions.Single(x => x.WithNds == false).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				sizeOptions.Single(x => x.WithNds == false).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				sizeOptions.Single(x => x.WithNds == false).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				sizeOptions.Single(x => x.WithNds == false).CostForGramm.Should().Be(updateGroupValueRequest.WithoutNdsPrice);

				productPrice.TechnologiesAdditionalPrices.Should().NotBeNull();
				productPrice.TechnologiesAdditionalPrices.Should().HaveCount(1);

				var technology = productPrice.TechnologiesAdditionalPrices[technologyId];
				technology.TechnologyId.Should().Be(technologyId);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void LinkProduct_ToAnotherGroupInSameCluster_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = true;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient);

				var cluster1 = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[0].Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);


				new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster1.Id, InOrder = true }.Exec(userClient);

				var clusterSettings1 = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings1.ClusterId = cluster1.Id;
				clusterSettings1.OrderMetalWeight = 1;
				clusterSettings1.ProductionTime = 10;

				var clusterInfoAll1 = clusterSettings1.Exec(userClient);

				var createGroupRequest1 = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest1.ClusterId = cluster1.Id;
				createGroupRequest1.Name = "GroupName1";
				createGroupRequest1.DisplayName = "DisplayName1";
				createGroupRequest1.LossPercentage = 10;

				var createGroup1 = createGroupRequest1.Exec(userClient);
				createGroup1.Values.Should().HaveCount(1);


				var createGroupRequest2 = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest2.ClusterId = cluster1.Id;
				createGroupRequest2.Name = "GroupName2";
				createGroupRequest2.DisplayName = "DisplayName2";
				createGroupRequest2.LossPercentage = 10;

				var createGroup2 = createGroupRequest2.Exec(userClient);
				createGroup2.Values.Should().HaveCount(1);


				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup1.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithoutNdsMarkup = 15;
				updateGroupValueRequest.WithNdsMarkup = 15;

				updateGroupValueRequest.Exec(userClient);
				updateGroupValueRequest.PriceGroupValueId = createGroup2.Values[0].PriceGroupValueId;
				updateGroupValueRequest.Exec(userClient);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);

				var productUid = Guid.NewGuid();
				var sizeUid = Guid.NewGuid();
				var syncItem = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid
				};

				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					SizeUid = sizeUid,
					ProductUid = productUid
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var products = new MariPriceApi.Price.Product.List
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup1.Id,
						ProductId = products[0].Id,
						PriceClusterId = cluster1.Id,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup1.Id,
						ProductId = products[1].Id,
						PriceClusterId = cluster1.Id,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				var portalLinkRequest = new MariPriceApi.PortalPrice.Product.Create
				{
					Id = products[1].Id,
					PriceGroupId = createGroup2.Id,
					RemoveOldLink = false
				};

				AssertionExtensions.Should(() => portalLinkRequest.Exec(userClient)).Throw<PriceGroupLinkOtherGroupException>();

				portalLinkRequest.RemoveOldLink = true;

				portalLinkRequest.Exec(userClient);


				var productGroupLinks = new MariPriceDb.Price.Product.GroupLinkGet() { ProductUid = productUid }.Exec(m_sql);
				productGroupLinks.Should().HaveCount(2);
				productGroupLinks.Single(x => x.ProductId == products[0].Id).PriceGroupId.Should().Be(createGroup1.Id);
				productGroupLinks.Single(x => x.ProductId == products[1].Id).PriceGroupId.Should().Be(createGroup2.Id);
			}
		}



		[TestMethod]
		public void LinkProduct_GroupFromAnotherCluster_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = true;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(2, starter.SpecSettings.PriceClusters, specClient);

				var cluster1 = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[0].Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var cluster2 = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName[1].Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalSilver,
					ClusterQuality = quality585
				}.Exec(userClient);

				new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster1.Id, InOrder = true }.Exec(userClient);
				new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster2.Id, InOrder = true }.Exec(userClient);

				var clusterSettings1 = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings1.ClusterId = cluster1.Id;
				clusterSettings1.OrderMetalWeight = 1;
				clusterSettings1.ProductionTime = 10;

				var clusterSettings2 = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings2.ClusterId = cluster2.Id;
				clusterSettings2.OrderMetalWeight = 1;
				clusterSettings2.ProductionTime = 10;

				var clusterInfoAll1 = clusterSettings1.Exec(userClient);
				var clusterInfoAll2 = clusterSettings2.Exec(userClient);

				var createGroupRequest1 = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest1.ClusterId = cluster1.Id;
				createGroupRequest1.Name = "GroupName1";
				createGroupRequest1.DisplayName = "DisplayName1";
				createGroupRequest1.LossPercentage = 10;

				var createGroup1 = createGroupRequest1.Exec(userClient);
				createGroup1.Values.Should().HaveCount(1);


				var createGroupRequest2 = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest2.ClusterId = cluster2.Id;
				createGroupRequest2.Name = "GroupName2";
				createGroupRequest2.DisplayName = "DisplayName2";
				createGroupRequest2.LossPercentage = 10;

				var createGroup2 = createGroupRequest2.Exec(userClient);
				createGroup2.Values.Should().HaveCount(1);


				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup1.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithoutNdsMarkup = 15;
				updateGroupValueRequest.WithNdsMarkup = 15;

				updateGroupValueRequest.Exec(userClient);
				updateGroupValueRequest.PriceGroupValueId = createGroup2.Values[0].PriceGroupValueId;
				updateGroupValueRequest.Exec(userClient);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);

				var productUid = Guid.NewGuid();
				var sizeUid = Guid.NewGuid();
				var syncItem = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid
				};

				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					SizeUid = sizeUid,
					ProductUid = productUid
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var products = new MariPriceApi.Price.Product.List
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup1.Id,
						ProductId = products[0].Id,
						PriceClusterId = cluster1.Id,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				var portalLinkRequest = new MariPriceApi.PortalPrice.Product.Create
				{
					Id = products[1].Id,
					PriceGroupId = createGroup2.Id,
					RemoveOldLink = false
				};

				AssertionExtensions.Should(() => portalLinkRequest.Exec(userClient)).Throw<PriceGroupLinkOtherClusterException>();

				portalLinkRequest.RemoveOldLink = true;

				portalLinkRequest.Exec(userClient);


				var productGroupLinks = new MariPriceDb.Price.Product.GroupLinkGet() { ProductUid = productUid }.Exec(m_sql);
				productGroupLinks.Should().HaveCount(1);
				productGroupLinks[0].PriceClusterId.Should().Be(cluster2.Id);
				productGroupLinks[0].ProductId.Should().Be(products[1].Id);
			}
		}


		[TestMethod]
		public void GetAggregate_PriceContent_InstockInformation()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var createTechnology = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = draftVersion.VersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = true;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var updateFlag = new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster.Id, InOrder = true }.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "DisplayName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);

				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithoutNdsMarkup = 15;
				updateGroupValueRequest.WithNdsMarkup = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				var updateGroupInstockValueRequest = new MariPriceApi.PortalPrice.InstockGroupValue.Update();
				updateGroupInstockValueRequest.PriceGroupId = createGroup.Id;
				updateGroupInstockValueRequest.WithNdsPrice = 55;
				updateGroupInstockValueRequest.WithoutNdsPrice = 66;
				updateGroupInstockValueRequest.WithoutNdsMarkup = 77;
				updateGroupInstockValueRequest.WithNdsMarkup = 88;

				updateGroupInstockValueRequest.Exec(userClient);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);
				swap.Clusters.Should().HaveCount(1);

				var productUid = Guid.NewGuid();
				var syncItem = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid
				};

				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -company.CompanyId,
					Enabled = true,
					Name = "Test",
					SizeUid = Guid.NewGuid(),

					ProductUid = productUid
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var products = new MariPriceApi.Price.Product.List
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = products[0].Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = products[1].Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				var priceAggregate = new MariPriceApi.Price.AggregatePrice.Request
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				priceAggregate.Items.Should().HaveCount(1);
				var productPrice = priceAggregate.Items[productUid];
				productPrice.PriceCluster.Id.Should().Be(cluster.Id);
				productPrice.PriceCluster.Name.Should().Be(clusterName.Value);
				productPrice.MainManufacturingOptions.Should().HaveCount(2);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).CostForGramm.Should().Be(updateGroupValueRequest.WithNdsPrice);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).CostForGramm.Should().Be(updateGroupValueRequest.WithoutNdsPrice);

				productPrice.ProductInstockInfo.MainInstockItems.WithNdsBuyPrice.Should().Be(updateGroupInstockValueRequest.WithNdsPrice);
				productPrice.ProductInstockInfo.MainInstockItems.WithNdsMarkup.Should().Be(updateGroupInstockValueRequest.WithNdsMarkup);
				productPrice.ProductInstockInfo.MainInstockItems.WithoutNdsBuyPrice.Should().Be(updateGroupInstockValueRequest.WithoutNdsPrice);
				productPrice.ProductInstockInfo.MainInstockItems.WithoutNdsMarkup.Should().Be(updateGroupInstockValueRequest.WithoutNdsMarkup);

				productPrice.SizeOwnManufacturingOptions.Should().HaveCount(1);
				var sizeOptions = productPrice.SizeOwnManufacturingOptions[syncItem2.SizeUid];
				sizeOptions.Should().HaveCount(2);
				sizeOptions.Single(x => x.WithNds == true).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				sizeOptions.Single(x => x.WithNds == true).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				sizeOptions.Single(x => x.WithNds == true).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				sizeOptions.Single(x => x.WithNds == true).CostForGramm.Should().Be(updateGroupValueRequest.WithNdsPrice);
				sizeOptions.Single(x => x.WithNds == false).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				sizeOptions.Single(x => x.WithNds == false).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				sizeOptions.Single(x => x.WithNds == false).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				sizeOptions.Single(x => x.WithNds == false).CostForGramm.Should().Be(updateGroupValueRequest.WithoutNdsPrice);

				productPrice.TechnologiesAdditionalPrices.Should().NotBeNull();
				productPrice.TechnologiesAdditionalPrices.Should().HaveCount(1);

				var technology = productPrice.TechnologiesAdditionalPrices[technologyId];
				technology.TechnologyId.Should().Be(technologyId);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void PriceCluster_ProductListPageInfo()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);
				var client = starter.SpecApi;

				var productUid1 = Guid.NewGuid();
				var productUid2 = Guid.NewGuid();
				var productUid3 = Guid.NewGuid();
				var productUid4 = Guid.NewGuid();
				var productUid5 = Guid.NewGuid();

				var syncItem_1_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid1
				};
				var syncItem_1_1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test1 - size 1",
					SizeUid = Guid.NewGuid(),
					ProductUid = productUid1
				};
				var syncItem_2_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test2",
					ProductUid = productUid2
				};
				var syncItem_3_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test3",
					ProductUid = productUid3
				};
				var syncItem_4_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test4",
					ProductUid = productUid4
				};
				var syncItem_5_0 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test5",
					ProductUid = productUid5
				};
				var syncItem_5_1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test5-1",
					SizeUid = Guid.NewGuid(),
					ProductUid = productUid5

				};
				var syncItem_5_2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = company.CompanyId,
					Enabled = true,
					Name = "Test5-2",
					SizeUid = Guid.NewGuid(),
					ProductUid = productUid5
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[]
					{
						syncItem_1_0,
						syncItem_1_1,
						syncItem_2_0,
						syncItem_3_0,
						syncItem_4_0,
						syncItem_5_0,
						syncItem_5_1,
						syncItem_5_2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var searchRequest = new MariPriceApi.PortalPrice.Product.List()
				{
					CompanyId = company.CompanyId,
					PageInfo = new Common.Dto.PageInfo() { PageNumber = 1, PageSize = 2 }
				};

				var products = searchRequest.Exec(userClient);
				products.Total.Should().Be(5);
				products.Items.Should().HaveCount(2);

				searchRequest.PageInfo.PageNumber = 3;
				products = searchRequest.Exec(userClient);
				products.Total.Should().Be(5);
				products.Items.Should().HaveCount(1);
			}
		}

		[TestMethod]
		public void GetPriceContent()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = true;

				update.Exec(userClient);

				var createTechnology = new MariPriceApi.PortalPrice.TechnologiesAdditions.Create
				{
					VersionId = draftVersion.VersionId,
					TechnologyId = technologyId,
					WithNdsPrice = 1.0M,
					WithoutNdsPrice = 2.0M
				}.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var updateFlag = new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag() { ClusterId = cluster.Id, InOrder = true }.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);

				var createGroupRequest = new MariPriceApi.PortalPrice.Group.Create();
				createGroupRequest.ClusterId = cluster.Id;
				createGroupRequest.Name = "GroupName";
				createGroupRequest.DisplayName = "DisplayName";
				createGroupRequest.LossPercentage = 10;

				var createGroup = createGroupRequest.Exec(userClient);
				createGroup.Values.Should().HaveCount(1);

				var updateGroupValueRequest = new MariPriceApi.PortalPrice.GroupValue.Update();
				updateGroupValueRequest.PriceGroupValueId = createGroup.Values[0].PriceGroupValueId;
				updateGroupValueRequest.WithNdsPrice = 5;
				updateGroupValueRequest.WithoutNdsPrice = 15;
				updateGroupValueRequest.WithoutNdsMarkup = 15;
				updateGroupValueRequest.WithNdsMarkup = 15;

				var updateGroupValue = updateGroupValueRequest.Exec(userClient);

				var swap = new MariPriceApi.PortalPrice.PriceCompany.Swap() { CompanyId = company.CompanyId }.Exec(userClient);
				swap.Clusters.Should().HaveCount(1);

				var productUid = Guid.NewGuid();
				var syncItem = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -1,
					Enabled = true,
					Name = "Test",
					ProductUid = productUid
				};

				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -1,
					Enabled = true,
					Name = "Test",
					SizeUid = Guid.NewGuid(),

					ProductUid = productUid
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem, syncItem2
				}
				};

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var products = new MariPriceApi.Price.Product.List
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = products[0].Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = products[1].Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = swap.VersionId
					}
				}.Exec(client);


				var priceContent = new MariPriceApi.Price.Product.PriceContent.List().ForProduct(productUid).Exec(client);

				priceContent.Should().HaveCount(1);
				var productPrice = priceContent[0];
				productPrice.PriceCluster.Id.Should().Be(cluster.Id);
				productPrice.PriceCluster.Name.Should().Be(clusterName.Value);
				productPrice.MainManufacturingOptions.Should().HaveCount(2);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == true).CostForGramm.Should().Be(updateGroupValueRequest.WithNdsPrice);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				productPrice.MainManufacturingOptions.Single(x => x.WithNds == false).CostForGramm.Should().Be(updateGroupValueRequest.WithoutNdsPrice);

				productPrice.SizeOwnManufacturingOptions.Should().HaveCount(1);
				var sizeOptions = productPrice.SizeOwnManufacturingOptions[syncItem2.SizeUid];
				sizeOptions.Should().HaveCount(2);
				sizeOptions.Single(x => x.WithNds == true).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				sizeOptions.Single(x => x.WithNds == true).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				sizeOptions.Single(x => x.WithNds == true).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				sizeOptions.Single(x => x.WithNds == true).CostForGramm.Should().Be(updateGroupValueRequest.WithNdsPrice);
				sizeOptions.Single(x => x.WithNds == false).LossPercentage.Should().Be(createGroupRequest.LossPercentage);
				sizeOptions.Single(x => x.WithNds == false).OrderMetalWeight.Should().Be(clusterSettings.OrderMetalWeight);
				sizeOptions.Single(x => x.WithNds == false).ProductionTime.Should().Be(clusterSettings.ProductionTime);
				sizeOptions.Single(x => x.WithNds == false).CostForGramm.Should().Be(updateGroupValueRequest.WithoutNdsPrice);

				productPrice.TechnologiesAdditionalPrices.Should().NotBeNull();
				productPrice.TechnologiesAdditionalPrices.Should().HaveCount(1);

				var technology = productPrice.TechnologiesAdditionalPrices[technologyId];
				technology.TechnologyId.Should().Be(technologyId);
				technology.WithNdsPrice.Should().Be(1.0M);
				technology.WithoutNdsPrice.Should().Be(2.0M);
			}
		}

		[TestMethod]
		public void CreateTheSameVariants()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = false;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append();
				clusterSettings.ClusterId = cluster.Id;
				clusterSettings.OrderMetalWeight = 1;
				clusterSettings.ProductionTime = 10;

				var clusterInfoAll = clusterSettings.Exec(userClient);
				AssertionExtensions.Should(() => clusterSettings.Exec(userClient)).Throw<RecordDuplicateApiException>();
			}
		}

		[TestMethod]
		public void ChangeStatusCluster()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = false;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				cluster.Enabled.Should().BeTrue();

				var updateCluster = new MariPriceApi.PortalPrice.Cluster.Update()
				{
					ClusterId = cluster.Id,
					ClusterName = clusterName.Id,
					Enabled = false,
					ClusterMetal = metalSilver,
					ClusterQuality = quality585
				}.Exec(userClient);

				updateCluster.ClusterName.Id.Should().Be(clusterName.Id);
				updateCluster.Enabled.Should().BeFalse();
				updateCluster.ClusterMetal.Should().Be(updateCluster.ClusterMetal);
				updateCluster.ClusterQuality.Should().Be(updateCluster.ClusterQuality);
			}
		}

		[TestMethod]
		public void UpdateClusterFlag()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var draftVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var update = new MariPriceApi.PortalPrice.PriceCompanyVersion.Update() { VersionId = draftVersion.VersionId };
				update.WithNdsPriceRequired = true;
				update.WithoutNdsPriceRequired = false;

				update.Exec(userClient);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = draftVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
			}.Exec(userClient);

				cluster.Enabled.Should().BeTrue();
				cluster.InOrder.Should().BeFalse();
				cluster.InStock.Should().BeFalse();

				var updateFlagInOrder = new MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag()
				{
					ClusterId = cluster.Id,
					InOrder = true
				}.Exec(userClient);

				updateFlagInOrder.ClusterName.Id.Should().Be(clusterName.Id);
				updateFlagInOrder.Enabled.Should().BeTrue();
				updateFlagInOrder.InOrder.Should().BeTrue();
				updateFlagInOrder.InStock.Should().BeFalse();

				var updateFlagInStock = new MariPriceApi.PortalPrice.Cluster.UpdateInStockFlag()
				{
					ClusterId = cluster.Id,
					InStock = true
				}.Exec(userClient);

				updateFlagInStock.ClusterName.Id.Should().Be(clusterName.Id);
				updateFlagInStock.Enabled.Should().BeTrue();
				updateFlagInStock.InOrder.Should().BeTrue();
				updateFlagInStock.InStock.Should().BeTrue();
			}
		}

		[TestMethod]
		public void InstockValueinGroupCreate_Test()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);
				var user = context.Manager;
				var company = context.ManagerCompany;

				var userClient = starter.MariPriceApi(user);

				var currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient).Single();

				var cluster = new MariPriceApi.PortalPrice.Cluster.Create
				{
					ClusterName = clusterName.Id,
					VersionId = currentVersion.VersionId,
					ClusterMetal = metalGold,
					ClusterQuality = quality900
				}.Exec(userClient);

				var clusterSettings = new MariPriceApi.PortalPrice.Cluster.Append()
				{
					ClusterId = cluster.Id,
					OrderMetalWeight = 1,
					ProductionTime = 10
				};

				var clusterInfoAll = clusterSettings.Exec(userClient);
				clusterInfoAll.ManufactureSettings.Enabled.Should().BeTrue();
				clusterInfoAll.ManufactureSettings.Variants.Should().HaveCount(1);
				clusterInfoAll.ManufactureSettings.Variants[0].Id.Should().BeGreaterThan(0);
				clusterInfoAll.ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);
				clusterInfoAll.ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);

				var createGroup = new MariPriceApi.PortalPrice.Group.Create()
				{
					ClusterId = cluster.Id,
					Name = "GroupName",
					DisplayName = "DisplayName",
					LossPercentage = 10,

				}.Exec(userClient);

				createGroup.Id.Should().BeGreaterThan(0);
				createGroup.Name.Should().Be("GroupName");
				createGroup.DisplayName.Should().Be("GroupName");
				createGroup.LossPercentage.Should().Be(10);
				createGroup.Values.Should().HaveCount(1);
				createGroup.Values[0].PriceGroupValueId.Should().BeGreaterThan(0);
				createGroup.Values[0].PriceClusterVariantId.Should().Be(clusterInfoAll.ManufactureSettings.Variants[0].Id);
				createGroup.Values[0].WithNdsPrice.Should().BeNull();
				createGroup.Values[0].WithoutNdsPrice.Should().BeNull();

				var updateGroupValue = new MariPriceApi.PortalPrice.InstockGroupValue.Update()
				{
					PriceGroupId = createGroup.Id,
					WithNdsPrice = 5,
					WithoutNdsPrice = 15,
					WithNdsMarkup = 33,
					WithoutNdsMarkup = 12
				}.Exec(userClient);

				currentVersion = MariPriceApi.PortalPrice.GetDraft(userClient, company.CompanyId);

				currentVersion.Clusters.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants.Should().HaveCount(1);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].ProductionTime.Should().Be(10);
				currentVersion.Clusters[0].ManufactureSettings.Variants[0].OrderMetalWeight.Should().Be(1);

				currentVersion.Clusters[0].PriceGroups.Should().HaveCount(1);
				currentVersion.Clusters[0].PriceGroups[0].LossPercentage.Should().Be(10);
				currentVersion.Clusters[0].PriceGroups[0].Name.Should().Be("GroupName");
				currentVersion.Clusters[0].PriceGroups[0].DisplayName.Should().Be("GroupName");

				currentVersion.Clusters[0].PriceGroups[0].Values.Should().HaveCount(1);

				currentVersion.Clusters[0].PriceGroups[0].InStockValues.Should().NotBeNull();
				currentVersion.Clusters[0].PriceGroups[0].InStockValues.WithNdsPrice.Should().Be(5);
				currentVersion.Clusters[0].PriceGroups[0].InStockValues.WithoutNdsPrice.Should().Be(15);
				currentVersion.Clusters[0].PriceGroups[0].InStockValues.WithNdsMarkup.Should().Be(33);
				currentVersion.Clusters[0].PriceGroups[0].InStockValues.WithoutNdsMarkup.Should().Be(12);
			}
		}
	}
}