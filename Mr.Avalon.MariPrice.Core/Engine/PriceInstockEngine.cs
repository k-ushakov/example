using d7k.Dto;
using d7k.Dto.Utilities;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.Print.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class PriceInstockEngine
	{
		private ISqlFactory m_sql;
		private DtoComplex m_dto;
		PrintApiClient m_print;
		PriceEngine m_price;

		public PriceInstockEngine(ISqlFactory sql, PrintApiClient print, DtoComplex dto, PriceEngine price)
		{
			m_sql = sql;
			m_dto = dto;

			m_price = price;
			m_print = print;
		}

		public void SetBarcodes(MariPriceApi.Price.Instock.Import request)
		{
			var dbRequest = new MariPriceDb.Price.Instock.Import
			{
				CompanyId = request.CompanyId,
				NewBarcodes = request.NewBarcodes.Select(x => new MariPriceDb.Price.Instock.Import.Item().CopyFrom(x, m_dto)).ToList()
			};

			using (var t = m_sql.Transaction())
			{
				dbRequest.Exec(t);
				t.Commit();
			}
		}

		public List<MariPriceApi.Price.Instock> GetBarcodes(MariPriceApi.Price.Instock.List request)
		{
			var dbRequest = new MariPriceDb.Price.Instock.List().CopyFrom(request, m_dto);

			var dbResult = dbRequest.Exec(m_sql);

			var result = dbResult.Select(x => new MariPriceApi.Price.Instock().CopyFrom(x, m_dto)).ToList();

			return result;
		}

		public MariPriceApi.Price.Instock.Export.Result ExportBarcode(MariPriceApi.Price.Instock.Export request, UserInfo user)
		{
			var dbRequest = new MariPriceDb.Price.Instock.List2() { CompanyIds = new List<int>() { request.CompanyId }, VersionId = request.VersionId };

			var dbResult = dbRequest.Exec(m_sql);

			var accum = new Print.Doc.MariInstock() { Barcodes = new List<Print.Doc.MariInstock.Item>() };
			if (dbResult.Any())
			{
				BuildHelpCacheOfPriceGroups(dbResult, out var products, out var priceGroups);

				foreach (var barcode in dbResult)
				{
					if (string.IsNullOrEmpty(barcode.GroupName))
					{
						if (products.TryGetValue(barcode.ProductUid, out var product) && product.PriceGroupId.HasValue &&
							priceGroups.TryGetValue(product.PriceGroupId.Value, out var group))
						{
							accum.Barcodes.Add(GetPrintItem(barcode, group.Name));
						}
					}
					else
					{
						accum.Barcodes.Add(GetPrintItem(barcode));
					}
				}
			}

			var url = m_print.CreateDoc(accum, user == null ? Guid.NewGuid() : user.SessionId, user == null ? DateTime.Now.AddMinutes(10) : user.Expired);

			return new MariPriceApi.Price.Instock.Export.Result() { Url = url };
		}

		private void BuildHelpCacheOfPriceGroups(List<MariPriceDb.Price.InstockFull> dbResult, out Dictionary<Guid, MariPriceApi.Price.Product> products, out Dictionary<int, MariPriceApi.Price.Group> priceGroups)
		{
			HashSet<Guid> rootPriceGroupNeeded = new HashSet<Guid>();
			foreach (var barcode in dbResult.Where(x => string.IsNullOrEmpty(x.GroupName)))
			{
				rootPriceGroupNeeded.Add(barcode.ProductUid);
			}

			if (!rootPriceGroupNeeded.Any())
			{
				products = new Dictionary<Guid, MariPriceApi.Price.Product>();
				priceGroups = new Dictionary<int, MariPriceApi.Price.Group>();
				return;
			}

			var rootProducts = m_price.GetProductsActiveCluster(new MariPriceApi.Price.Product.List().ForProducts(rootPriceGroupNeeded.ToArray()).ForSizes(Guid.Empty), true);
			products = rootProducts.GroupBy(x => x.ProductUid).ToDictionary(x => x.Key, x => x.First());
			var groupsIds = rootProducts.Where(x => x.PriceGroupId.HasValue).Select(x => x.PriceGroupId.Value).Distinct().ToArray();
			priceGroups = m_price.GetGroups(new MariPriceApi.Price.Group.List().ForIds(groupsIds)).ToDictionary(x => x.Id);
		}

		private Print.Doc.MariInstock.Item GetPrintItem(MariPriceDb.Price.InstockFull barcode, string groupName = null)
		{
			return new Print.Doc.MariInstock.Item()
			{
				Barcode = barcode.Barcode,
				Size = barcode.Size,
				ProductPn = barcode.Pn,
				SizePn = barcode.SizePn,
				Weight = barcode.Weight,
				WireThickness = barcode.WireThickness,
				PriceGroupName = barcode.GroupName ?? groupName
			};
		}
	}
}
