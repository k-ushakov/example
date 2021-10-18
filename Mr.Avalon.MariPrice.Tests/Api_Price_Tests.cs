using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Tests
{
	[TestClass]
	public class Api_Price_Tests
	{
		const int companyId = 1;
		Guid metalGold = Guid.Parse("00000000-5ee7-6147-d3ab-eb1c1c5f7fc2");
		Guid metalSilver = Guid.Parse("00000000-5ee7-614f-d3ab-eb1c1c5f7fc5");

		Guid quality900 = Guid.Parse("00000000-5ee7-68bf-d3ab-eb1c1c5f8008");
		Guid quality585 = Guid.Parse("00000000-5ee7-66a8-d3ab-eb1c1c5f7ff6");

		[TestMethod]
		public void ClusterCreate()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;
			}
		}

		[TestMethod]
		public void CreateCompanyVersions()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				create.CompanyId.Should().Be(context.ManagerCompany.CompanyId);
				create.ActiveVersionId.Should().BeGreaterThan(0);
				create.DraftVersionId.Should().BeGreaterThan(0);
			}
		}

		[TestMethod]
		public void CreateCluster()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				create.CompanyId.Should().Be(context.ManagerCompany.CompanyId);
				create.ActiveVersionId.Should().BeGreaterThan(0);
				create.DraftVersionId.Should().BeGreaterThan(0);

				new MariPriceApi.Price.PriceCompanyVersion.Update()
				{
					VersionId = create.DraftVersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(client);

				var version = MariPriceApi.Price.PriceCompanyVersion.GetPrice(create.DraftVersionId, client);
				version.WithNdsPriceRequired.Should().BeTrue();
				version.WithoutNdsPriceRequired.Should().BeTrue();

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient);
				var createClaster = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName.FirstOrDefault().Id,
					VersionId = create.DraftVersionId,
					InOrder = true,
					InStock = false,
					Metal = metalGold,
					Quality = quality900
				}.Exec(client);

				createClaster.Id.Should().BeGreaterThan(0);
				createClaster.VersionId.Should().Be(create.DraftVersionId);
				createClaster.Name.Should().Be(createClaster.Name);
				createClaster.InOrder.Should().BeTrue();
				createClaster.InStock.Should().BeFalse();
				createClaster.Metal.Should().Be(metalGold);
				createClaster.Quality.Should().Be(quality900);

				var createVariants = new MariPriceApi.Price.ClusterSetting.Create()
				{
					ClusterId = createClaster.Id,
					OrderMetalWeight = 10,
					ProductionTime = 5
				}.Exec(client);

				createVariants.Id.Should().BeGreaterThan(0);
				createVariants.OrderMetalWeight.Should().Be(createVariants.OrderMetalWeight);
				createVariants.ProductionTime.Should().Be(createVariants.ProductionTime);

				var createGroup = new MariPriceApi.Price.Group.Create()
				{
					ClusterId = createClaster.Id,
					Name = "Name",
					LossPercentage = 12,
					AdditionalLossPercentage = 14,
					DisplayName = "DName"
				}.Exec(client);

				createGroup.ClusterId.Should().Be(createClaster.Id);
				createGroup.LossPercentage.Should().Be(createGroup.LossPercentage);
				createGroup.AdditionalLossPercentage.Should().Be(createGroup.AdditionalLossPercentage);
				createGroup.TotalLossPercentage.Should().Be(createGroup.TotalLossPercentage);
				createGroup.Name.Should().Be(createGroup.Name);
				createGroup.DisplayName.Should().Be(createGroup.DisplayName);
				createGroup.Id.Should().BeGreaterThan(0);

				var createValue = new MariPriceApi.Price.GroupValue.Create()
				{
					PriceClusterVariantId = createVariants.Id,
					PriceGroupId = createGroup.Id,
					WithNdsMarkup = 1,
					WithNdsPrice = 2,
					WithoutNdsMarkup = 3,
					WithoutNdsPrice = 4
				}.Exec(client);

				var groups = new MariPriceApi.Price.Group.List() { Ids = new List<int>() { createGroup.Id } }.Exec(client);
				groups.Should().HaveCount(1);
				groups[0].Values.Should().HaveCount(1);
				groups[0].Values[0].WithNdsMarkup.Should().Be(1);
				groups[0].Values[0].WithNdsPrice.Should().Be(2);
				groups[0].Values[0].WithoutNdsMarkup.Should().Be(3);
				groups[0].Values[0].WithoutNdsPrice.Should().Be(4);

				new MariPriceApi.Price.PriceCompany.Swap() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				var info = MariPriceApi.Price.PriceCompany.GetPrice(context.ManagerCompany.CompanyId, client);
				info.ActiveVersionId.Should().Be(create.DraftVersionId);
				info.DraftVersionId.Should().Be(create.ActiveVersionId);
			}
		}


		[TestMethod]
		public void UpdateCluster()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				create.CompanyId.Should().Be(context.ManagerCompany.CompanyId);
				create.ActiveVersionId.Should().BeGreaterThan(0);
				create.DraftVersionId.Should().BeGreaterThan(0);

				new MariPriceApi.Price.PriceCompanyVersion.Update()
				{
					VersionId = create.DraftVersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(client);

				var version = MariPriceApi.Price.PriceCompanyVersion.GetPrice(create.DraftVersionId, client);
				version.WithNdsPriceRequired.Should().BeTrue();
				version.WithoutNdsPriceRequired.Should().BeTrue();

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient);
				var createClaster = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName.FirstOrDefault().Id,
					VersionId = create.DraftVersionId,
					InOrder = true,
					InStock = false,
					Enabled = false,
					Metal = metalGold,
					Quality = quality900
				}.Exec(client);

				var update = new MariPriceApi.Price.Cluster.Update()
				{
					Id = createClaster.Id,
					InStock = false,
					InOrder = true,
					Enabled = true,
					Metal = metalSilver,
					Quality = quality585
				};
				update.UpdationList = new string[] {
					nameof(update.InOrder),
					nameof(update.InStock),
					nameof(update.Enabled),
					nameof(update.Metal),
					nameof(update.Quality)
				};

				update.Exec(client);

				var clusterOrigin = new MariPriceApi.Price.Cluster.List().ForIds(createClaster.Id).Exec(client).FirstOrDefault();
				clusterOrigin.Should().NotBeNull();
				clusterOrigin.InStock.Should().BeFalse();
				clusterOrigin.InOrder.Should().BeTrue();
				clusterOrigin.Enabled.Should().BeTrue();
				clusterOrigin.Metal.Should().Be(metalSilver);
				clusterOrigin.Quality.Should().Be(quality585);
			}
		}

		[TestMethod]
		public void SetBarcodes()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);
				var companyId = create.CompanyId;

				var productUid = Guid.NewGuid();
				new MariPriceDb.Price.Product.Merge
				{
					Products = new[]
					{
						new MariPriceDb.Price.Product.Merge.Item
						{
							CompanyId = companyId,
							ProductUid = productUid,
							Pn = "articul",
						}
					}
				}.Exec(starter.Settings.MariSql);

				var product = new MariPriceDb.Price.Product.List().ForProducts(productUid).Exec(starter.Settings.MariSql).Single();

				var importRequest = new MariPriceApi.Price.Instock.Import
				{
					CompanyId = companyId,
					NewBarcodes = new List<MariPriceApi.Price.Instock.Import.Item>
					{
						new MariPriceApi.Price.Instock.Import.Item{
							Barcode = "111",
							ProductId = product.Id,
							Weight = 42.5M
						},
						new MariPriceApi.Price.Instock.Import.Item{
							Barcode = "222",
							ProductId = product.Id,
							Weight = 42.5M
						}
					}
				};
				importRequest.Exec(client);

				var barcodes = new MariPriceApi.Price.Instock.List().ForCompanies(companyId).Exec(client);

				barcodes.Should().HaveCount(2);
				barcodes.Should().OnlyContain(x => x.ProductId == product.Id);
				barcodes.Should().OnlyContain(x => x.ProductUid == productUid);
				barcodes.Should().OnlyContain(x => x.SizeUid == Guid.Empty);
				barcodes.Should().OnlyContain(x => x.CompanyId == companyId);
				barcodes.Should().OnlyContain(x => x.Barcode == "111" || x.Barcode == "222");
				barcodes.Should().OnlyContain(x => x.Weight == 42.5M);

				importRequest.NewBarcodes = new List<MariPriceApi.Price.Instock.Import.Item>
					{
						new MariPriceApi.Price.Instock.Import.Item{
							Barcode = "333",
							ProductId = product.Id,
							Weight = 84.333M
						},
						new MariPriceApi.Price.Instock.Import.Item{
							Barcode = "222",
							ProductId = product.Id,
							Weight = 84.333M
						}
					};
				importRequest.Exec(client);

				barcodes = new MariPriceApi.Price.Instock.List().ForCompanies(companyId).Exec(client);

				barcodes.Should().HaveCount(2);
				barcodes.Should().OnlyContain(x => x.ProductId == product.Id);
				barcodes.Should().OnlyContain(x => x.ProductUid == productUid);
				barcodes.Should().OnlyContain(x => x.SizeUid == Guid.Empty);
				barcodes.Should().OnlyContain(x => x.CompanyId == companyId);
				barcodes.Should().OnlyContain(x => x.Barcode == "333" || x.Barcode == "222");
				barcodes.Should().OnlyContain(x => x.Weight == 84.333M);
			}
		}


		[TestMethod]
		public void MergeProductDb()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var item = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = 1,
					ProductUid = Guid.NewGuid(),
					SizeUid = Guid.Empty,
					Metal = "золото",
					Name = "Title",
					Status = Spec.Dto.ProductState.Active,
					Pn = "Pn",
					SizePn = "Pn",
					ImageUrl = "imageUrl",
					Title = "Title",
					Size = "123.456",
					WireThickness = 567.890M
				};

				var item1 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = 1,
					ProductUid = item.ProductUid,
					SizeUid = Guid.NewGuid(),
					Metal = "золото",
					Name = "Title",
					Status = Spec.Dto.ProductState.Active,
					Pn = "Pn",
					SizePn = "PnSize",
					ImageUrl = "imageUrl",
					Title = "Title"

				};
				var mergeDb = new MariPriceDb.Price.Product.Merge() { Products = new MariPriceDb.Price.Product.Merge.Item[] { item, item1 } };

				var m_sql = starter.Settings.MariSql;

				using (var transSql = m_sql.Transaction())
				{
					mergeDb.Exec(transSql);
					transSql.Commit();
				}

				var res = new MariPriceDb.Price.Product.SearchProduct() { ProductUids = new Guid[] { item.ProductUid } }.Exec(m_sql);
				res.Should().HaveCount(2);
				var itemRes = res.FirstOrDefault(x => x.ProductUid == item.ProductUid && x.SizeUid == Guid.Empty);
				itemRes.Status.Should().Be(4);
				itemRes.Title.Should().Be("Title");
				itemRes.Metal.Should().Be("золото");
				itemRes.Name.Should().Be("Title");
				itemRes.Pn.Should().Be("Pn");
				itemRes.ImageUrl.Should().Be("imageUrl");
				itemRes.Size.Should().Be("123.456");
				itemRes.WireThickness.Should().Be(567.890M);

				var item1Res = res.FirstOrDefault(x => x.ProductUid == item.ProductUid && x.SizeUid == item1.SizeUid);
				item1Res.Status.Should().Be(4);
				item1Res.Title.Should().Be("Title");
				item1Res.Metal.Should().Be("золото");
				item1Res.Name.Should().Be("Title");
				item1Res.Pn.Should().Be("PnSize");
				item1Res.ImageUrl.Should().Be("imageUrl");
				item1Res.Size.Should().Be("");
				item1Res.WireThickness.Should().BeNull();
			}
		}
		[TestMethod]
		public void UpdateStatus()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var productReq = new MariPriceApi.Price.Product.Sync();

				productReq.Action = Spec.Dto.ProductAction.Save;
				productReq.Products = new List<MariPriceApi.Price.Product.Sync.Item>();

				var productUid = ObjectId.GenerateNewId().GetGuid();
				var sizeUid = ObjectId.GenerateNewId().GetGuid();
				var item = new MariPriceApi.Price.Product.Sync.Item();
				item.ProductUid = productUid;
				item.DescriptionProductSyncInfo = new Description.Dto.ProductSync();
				item.DescriptionProductSyncInfo.Items = new List<Description.Dto.ProductSync.Item>();
				item.DescriptionProductSyncInfo.Items.Add(new Description.Dto.ProductSync.Item()
				{
					ProductUid = productUid,
					CompanyId = 1,
					ProductPn = "КольцоТест",
					Metal = "серебро 585",
					SizeUid = Guid.Empty,
					Title = "Кольцо. Красное серебро 585 пробы",
					Status = 4
				});
				item.DescriptionProductSyncInfo.Items.Add(new Description.Dto.ProductSync.Item()
				{
					ProductUid = productUid,
					CompanyId = 1,
					ProductPn = "dddd",
					Metal = "серебро 585",
					SizeUid = sizeUid,
					Title = "eeee",
					Status = 4
				});

				productReq.Products.Add(item);

				var origin = productReq.Exec(client);
				origin.Count.Should().Be(2);
				var accum = origin.First(x => x.Pn == "КольцоТест");
				accum.Status.Should().Be(4);
				accum = origin.First(x => x.Pn == "dddd");
				accum.Status.Should().Be(4);

				new MariPriceApi.Price.Product.UpdateStatus()
				{
					ProductUid = productUid,
					Status = 5
				}.Exec(client);

				var products = new MariPriceApi.Price.Product.List
				{
					ProductUids = new List<Guid> { productUid }
				}.Exec(client);

				products.Should().HaveCount(2);
				accum = products.First(x => x.SizeUid == Guid.Empty);
				accum.Pn.Should().Be("КольцоТест");
				accum.Status.Should().Be(5);

				accum = products.First(x => x.SizeUid == sizeUid);
				accum.Pn.Should().Be("dddd");
				accum.Status.Should().Be(5);
			}
		}

		[TestMethod]
		public void SetProduct()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{

				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);
				new MariPriceApi.Price.PriceCompanyVersion.Update()
				{
					VersionId = create.DraftVersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(client);

				var clusterName = OrderContext.GetSpecVocValues(2, starter.SpecSettings.PriceClusters, specClient);
				var createClaster = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName[0].Id,
					VersionId = create.DraftVersionId,
					Metal = metalGold,
					Quality = quality900
				}.Exec(client);
				var createClaster2 = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName[1].Id,
					VersionId = create.DraftVersionId,
					Metal = metalSilver,
					Quality = quality585
				}.Exec(client);

				var createVariants = new MariPriceApi.Price.ClusterSetting.Create()
				{
					ClusterId = createClaster.Id,
					OrderMetalWeight = 10,
					ProductionTime = 5
				}.Exec(client);
				var createVariants2 = new MariPriceApi.Price.ClusterSetting.Create()
				{
					ClusterId = createClaster2.Id,
					OrderMetalWeight = 20,
					ProductionTime = 10
				}.Exec(client);

				var createGroup = new MariPriceApi.Price.Group.Create()
				{
					ClusterId = createClaster.Id,
					Name = "Name",
					LossPercentage = 12,
					DisplayName = "DName"
				}.Exec(client);
				var createGroup2 = new MariPriceApi.Price.Group.Create()
				{
					ClusterId = createClaster2.Id,
					Name = "Name2",
					LossPercentage = 2,
					DisplayName = "DName2"
				}.Exec(client);

				var createValue = new MariPriceApi.Price.GroupValue.Create()
				{
					PriceClusterVariantId = createVariants.Id,
					PriceGroupId = createGroup.Id,
					WithNdsMarkup = 1,
					WithNdsPrice = 2,
					WithoutNdsMarkup = 3,
					WithoutNdsPrice = 4
				}.Exec(client);
				var createValue2 = new MariPriceApi.Price.GroupValue.Create()
				{
					PriceClusterVariantId = createVariants2.Id,
					PriceGroupId = createGroup2.Id,
					WithNdsMarkup = 12,
					WithNdsPrice = 22,
					WithoutNdsMarkup = 32,
					WithoutNdsPrice = 42
				}.Exec(client);

				var productUid = Guid.NewGuid();
				var sizeUid = Guid.NewGuid();
				var sizeUid2 = Guid.NewGuid();
				var syncItem = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -1,
					Enabled = true,
					Name = "Product",
					ProductUid = productUid
				};

				var syncItem2 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -1,
					Enabled = true,
					Name = "ProductSize14",
					SizeUid = sizeUid,
					ProductUid = productUid
				};

				var syncItem3 = new MariPriceDb.Price.Product.Merge.Item()
				{
					CompanyId = -1,
					Enabled = true,
					Name = "ProductSize16",
					SizeUid = sizeUid2,
					ProductUid = productUid
				};

				var mergeDb = new MariPriceDb.Price.Product.Merge()
				{
					Products = new MariPriceDb.Price.Product.Merge.Item[] {

						syncItem, syncItem2,syncItem3
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

				products.Count.Should().Be(3);

				var pr = products.FirstOrDefault(x => x.SizeUid == Guid.Empty);
				pr.Name.Should().Be("Product");
				var prs14 = products.FirstOrDefault(x => x.SizeUid == sizeUid);
				prs14.Name.Should().Be("ProductSize14");
				var prs16 = products.FirstOrDefault(x => x.SizeUid == sizeUid2);
				prs16.Name.Should().Be("ProductSize16");

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = pr.Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = create.DraftVersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup.Id,
						ProductId = prs14.Id,
						PriceClusterId = createGroup.ClusterId,
						ProductUid = productUid,
						VersionId = create.DraftVersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForSet = new MariPriceApi.Price.Product.Link.Set
					{
						PriceGroupId = createGroup2.Id,
						ProductId = prs16.Id,
						PriceClusterId = createGroup2.ClusterId,
						ProductUid = productUid,
						VersionId = create.DraftVersionId
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForDelete = new MariPriceApi.Price.Product.Link.Delete
					{
						PriceGroupId = createGroup.Id,
						ProductId = pr.Id
					}
				}.Exec(client);

				new MariPriceApi.Price.Product.Link
				{
					ForDelete = new MariPriceApi.Price.Product.Link.Delete
					{
						PriceGroupId = createGroup.Id,
						ProductId = prs14.Id
					}
				}.Exec(client);
			}
		}

		[TestMethod]
		public void SyncProduct()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var productReq = new MariPriceApi.Price.Product.Sync();

				productReq.Action = Spec.Dto.ProductAction.Save;
				productReq.Products = new List<MariPriceApi.Price.Product.Sync.Item>();

				var productUid = ObjectId.GenerateNewId().GetGuid();
				var item = new MariPriceApi.Price.Product.Sync.Item();
				item.ProductUid = productUid;
				item.DescriptionProductSyncInfo = new Description.Dto.ProductSync();
				item.DescriptionProductSyncInfo.Items = new List<Description.Dto.ProductSync.Item>();
				item.DescriptionProductSyncInfo.Items.Add(new Description.Dto.ProductSync.Item()
				{
					ProductUid = productUid,
					CompanyId = 1,
					Pn = "КольцоТест",
					Metal = "серебро 585",
					SizeUid = Guid.Empty,
					Title = "Кольцо. Красное серебро 585 пробы"
				});
				item.DescriptionProductSyncInfo.Items.Add(new Description.Dto.ProductSync.Item()
				{
					ProductUid = productUid,
					CompanyId = 1,
					Pn = "dddd",
					Metal = "серебро 585",
					SizeUid = ObjectId.GenerateNewId().GetGuid(),
					Title = "eeee"
				});

				productReq.Products.Add(item);

				productReq.Exec(client);
			}
		}

		[TestMethod]
		public void CreateInstockValues()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				create.CompanyId.Should().Be(context.ManagerCompany.CompanyId);
				create.ActiveVersionId.Should().BeGreaterThan(0);
				create.DraftVersionId.Should().BeGreaterThan(0);

				new MariPriceApi.Price.PriceCompanyVersion.Update()
				{
					VersionId = create.DraftVersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(client);

				var version = MariPriceApi.Price.PriceCompanyVersion.GetPrice(create.DraftVersionId, client);
				version.WithNdsPriceRequired.Should().BeTrue();
				version.WithoutNdsPriceRequired.Should().BeTrue();

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient);
				var createClaster = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName.FirstOrDefault().Id,
					VersionId = create.DraftVersionId,
					Metal = metalGold,
					Quality = quality900
				}.Exec(client);

				createClaster.Id.Should().BeGreaterThan(0);
				createClaster.VersionId.Should().Be(create.DraftVersionId);
				createClaster.Name.Should().Be(createClaster.Name);
				createClaster.Metal.Should().Be(createClaster.Metal);
				createClaster.Quality.Should().Be(createClaster.Quality);

				var createGroup = new MariPriceApi.Price.Group.Create()
				{
					ClusterId = createClaster.Id,
					Name = "Name",
					LossPercentage = 12,
					DisplayName = "DName"
				}.Exec(client);

				createGroup.ClusterId.Should().Be(createClaster.Id);
				createGroup.LossPercentage.Should().Be(createGroup.LossPercentage);
				createGroup.Name.Should().Be(createGroup.Name);
				createGroup.DisplayName.Should().Be(createGroup.DisplayName);
				createGroup.Id.Should().BeGreaterThan(0);
				createGroup.Values.Should().HaveCount(0);
				createGroup.InStockValues.Should().BeNull();

				var createInstock = new MariPriceApi.Price.InstockGroupValue.Create()
				{
					PriceGroupId = createGroup.Id,
					WithNdsMarkup = 1,
					WithNdsPrice = 2,
					WithoutNdsMarkup = 3,
					WithoutNdsPrice = 4
				}.Exec(client);

				createInstock.PriceGroupId.Should().Be(createGroup.Id);
				createInstock.WithNdsMarkup.Should().Be(1);
				createInstock.WithNdsPrice.Should().Be(2);
				createInstock.WithoutNdsMarkup.Should().Be(3);
				createInstock.WithoutNdsPrice.Should().Be(4);

				var group = new MariPriceApi.Price.Group.List().ForIds(createGroup.Id).Exec(client);
				group.Should().HaveCount(1);
				group[0].ClusterId.Should().Be(createClaster.Id);
				group[0].LossPercentage.Should().Be(createGroup.LossPercentage);
				group[0].Name.Should().Be(createGroup.Name);
				group[0].DisplayName.Should().Be(createGroup.DisplayName);
				group[0].Id.Should().BeGreaterThan(0);

				group[0].InStockValues.PriceGroupId.Should().Be(createGroup.Id);
				group[0].InStockValues.WithNdsMarkup.Should().Be(1);
				group[0].InStockValues.WithNdsPrice.Should().Be(2);
				group[0].InStockValues.WithoutNdsMarkup.Should().Be(3);
				group[0].InStockValues.WithoutNdsPrice.Should().Be(4);
			}
		}

		[TestMethod]
		public void UpdateInstockValues()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				create.CompanyId.Should().Be(context.ManagerCompany.CompanyId);
				create.ActiveVersionId.Should().BeGreaterThan(0);
				create.DraftVersionId.Should().BeGreaterThan(0);

				new MariPriceApi.Price.PriceCompanyVersion.Update()
				{
					VersionId = create.DraftVersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(client);

				var version = MariPriceApi.Price.PriceCompanyVersion.GetPrice(create.DraftVersionId, client);
				version.WithNdsPriceRequired.Should().BeTrue();
				version.WithoutNdsPriceRequired.Should().BeTrue();

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient);
				var createClaster = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName.FirstOrDefault().Id,
					VersionId = create.DraftVersionId,
					Metal = metalGold,
					Quality = quality900
				}.Exec(client);

				createClaster.Id.Should().BeGreaterThan(0);
				createClaster.VersionId.Should().Be(create.DraftVersionId);
				createClaster.Name.Should().Be(createClaster.Name);
				createClaster.Metal.Should().Be(createClaster.Metal);
				createClaster.Quality.Should().Be(createClaster.Quality);

				var createGroup = new MariPriceApi.Price.Group.Create()
				{
					ClusterId = createClaster.Id,
					Name = "Name",
					LossPercentage = 12,
					DisplayName = "DName"
				}.Exec(client);

				createGroup.ClusterId.Should().Be(createClaster.Id);
				createGroup.LossPercentage.Should().Be(createGroup.LossPercentage);
				createGroup.Name.Should().Be(createGroup.Name);
				createGroup.DisplayName.Should().Be(createGroup.DisplayName);
				createGroup.Id.Should().BeGreaterThan(0);
				createGroup.Values.Should().HaveCount(0);
				createGroup.InStockValues.Should().BeNull();

				var createInstock = new MariPriceApi.Price.InstockGroupValue.Create()
				{
					PriceGroupId = createGroup.Id,
					WithNdsMarkup = 1,
					WithNdsPrice = 2,
					WithoutNdsMarkup = 3,
					WithoutNdsPrice = 4
				}.Exec(client);

				new MariPriceApi.Price.InstockGroupValue.Update()
				{
					PriceGroupId = createGroup.Id,
					WithNdsMarkup = 10,
					WithNdsPrice = 20,
					WithoutNdsMarkup = 30,
					WithoutNdsPrice = 40
				}.Exec(client);

				var group = new MariPriceApi.Price.Group.List().ForIds(createGroup.Id).Exec(client);
				group.Should().HaveCount(1);

				group[0].InStockValues.PriceGroupId.Should().Be(createGroup.Id);
				group[0].InStockValues.WithNdsMarkup.Should().Be(10);
				group[0].InStockValues.WithNdsPrice.Should().Be(20);
				group[0].InStockValues.WithoutNdsMarkup.Should().Be(30);
				group[0].InStockValues.WithoutNdsPrice.Should().Be(40);
			}
		}

		[TestMethod]
		public void DeleteInstockValues()
		{
			var starter = new MariPriceStarter();
			using (starter.Start())
			{
				var client = starter.MariPriceApi();
				var specClient = starter.SpecApi;

				var context = new OrderContext().WithManager().Create(starter);

				var create = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = context.ManagerCompany.CompanyId }.Exec(client);

				create.CompanyId.Should().Be(context.ManagerCompany.CompanyId);
				create.ActiveVersionId.Should().BeGreaterThan(0);
				create.DraftVersionId.Should().BeGreaterThan(0);

				new MariPriceApi.Price.PriceCompanyVersion.Update()
				{
					VersionId = create.DraftVersionId,
					WithNdsPriceRequired = true,
					WithoutNdsPriceRequired = true
				}.Exec(client);

				var version = MariPriceApi.Price.PriceCompanyVersion.GetPrice(create.DraftVersionId, client);
				version.WithNdsPriceRequired.Should().BeTrue();
				version.WithoutNdsPriceRequired.Should().BeTrue();

				var clusterName = OrderContext.GetSpecVocValues(1, starter.SpecSettings.PriceClusters, specClient);
				var createClaster = new MariPriceApi.Price.Cluster.Create()
				{
					Name = clusterName.FirstOrDefault().Id,
					VersionId = create.DraftVersionId,
					Metal = metalGold,
					Quality = quality900
				}.Exec(client);

				createClaster.Id.Should().BeGreaterThan(0);
				createClaster.VersionId.Should().Be(create.DraftVersionId);
				createClaster.Name.Should().Be(createClaster.Name);
				createClaster.Metal.Should().Be(createClaster.Metal);
				createClaster.Quality.Should().Be(createClaster.Quality);

				var createGroup = new MariPriceApi.Price.Group.Create()
				{
					ClusterId = createClaster.Id,
					Name = "Name",
					LossPercentage = 12,
					DisplayName = "DName"
				}.Exec(client);

				createGroup.ClusterId.Should().Be(createClaster.Id);
				createGroup.LossPercentage.Should().Be(createGroup.LossPercentage);
				createGroup.Name.Should().Be(createGroup.Name);
				createGroup.DisplayName.Should().Be(createGroup.DisplayName);
				createGroup.Id.Should().BeGreaterThan(0);
				createGroup.Values.Should().HaveCount(0);
				createGroup.InStockValues.Should().BeNull();

				var createInstock = new MariPriceApi.Price.InstockGroupValue.Create()
				{
					PriceGroupId = createGroup.Id,
					WithNdsMarkup = 1,
					WithNdsPrice = 2,
					WithoutNdsMarkup = 3,
					WithoutNdsPrice = 4
				}.Exec(client);

				new MariPriceApi.Price.InstockGroupValue.Delete()
				{
					PriceGroupId = createGroup.Id
				}.Exec(client);

				var group = new MariPriceApi.Price.Group.List().ForIds(createGroup.Id).Exec(client);
				group.Should().HaveCount(1);

				group[0].InStockValues.Should().BeNull();
			}
		}
	}
}
