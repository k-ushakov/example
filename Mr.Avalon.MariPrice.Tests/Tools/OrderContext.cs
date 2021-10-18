using MongoDB.Bson;
using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.TestTools;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.Spec.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Tests
{
	public class OrderContext
	{
		public TestUserInfo User { get; set; }

		public TestCompanyInfo Company { get; set; }

		public TestUserInfo Manager { get; set; }
		public TestCompanyInfo ManagerCompany { get; set; }


		public OrderContext Create(MariPriceStarter starter)
		{
			User?.Create(starter.Profile);
			Company?.Create(starter.Profile);

			Manager?.Create(starter.Profile);
			ManagerCompany?.Create(starter.Profile);
			return this;
		}

		public OrderContext WithManager()
		{
			Manager = new TestUserInfo($"xxx+{Guid.NewGuid().ToString("N")}@gmail.com").Roles(UserRoles.ActivatedUser, UserRoles.MariManager);
			if (ManagerCompany == null)
				ManagerCompany = new TestCompanyInfo(Manager, "Test").Available(Manager);

			return this;
		}


		public static List<SpecApi.VocValue> GetSpecVocValues(int neededValuesCount, Guid vocId, SpecApiClient client)
		{
			SpecApi.VocInfo.ExecGet(client, vocId);

			var existValues = new SpecApi.VocValue.List.Request() { VocId = vocId }.Exec(client).Items;

			for (int i = 0, k = existValues.Count; i < neededValuesCount - k; i++)
			{
				existValues.Add(
					new SpecApi.VocValue.Create { VocId = vocId, State = SpecApi.VocValueState.Active, Value = $"{i}Test" }
				.Exec(client));
			}

			return existValues.Take(neededValuesCount).ToList();
		}

		public static void CreateCluster(
			int versionId,
			Guid clasterName,
			MariPriceApi.PortalPrice.Cluster.Append varians,
			MariPriceApi.PortalPrice.Group.Create group)
		{


		}

		public MariPriceApi.Price.PriceCompany CreatePriceCompany(MariPriceStarter starter)
		{
			var client = starter.MariPriceApi();
			var company = new MariPriceApi.Price.PriceCompany.Create() { CompanyId = ManagerCompany.CompanyId }.Exec(client);
			
			return company;

		}
	}


	public static class Convert
	{
		public static Guid GetGuid(this ObjectId objectId)
		{
			var bytes = objectId.ToByteArray();
			FixedArrayOrder(bytes);

			return new Guid(new byte[4].Concat(bytes).ToArray());
		}


		public static void FixedArrayOrder(byte[] bytes)
		{
			byte s = bytes[0];
			bytes[0] = bytes[1];
			bytes[1] = s;

			s = bytes[2];
			bytes[2] = bytes[3];
			bytes[3] = s;
		}
	}
}
