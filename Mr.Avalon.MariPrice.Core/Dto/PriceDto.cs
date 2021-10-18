using d7k.Dto;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.Spec.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	[DtoContainer]
	public static class PriceDto
	{
		public interface ICompanyInfo
		{
			int CompanyId { get; set; }
		}

		public interface IEnabledInfo
		{
			bool Enabled { get; set; }
		}

		public interface IPnInfo
		{
			string Pn { get; set; }
			string SizePn { get; set; }
		}

		public interface IProductDbStatus
		{
			int? Status { get; set; }
		}

		public interface IProductApiStatus
		{
			ProductState Status { get; set; }
		}

		[DtoConvert]
		static void Convert(IProductDbStatus dst, IProductApiStatus src)
		{
			dst.Status = (int)src.Status;
		}

		[DtoConvert]
		static void Convert(IProductApiStatus dst, IProductDbStatus src)
		{
			dst.Status = (src.Status.HasValue) ? (ProductState)src.Status.Value : ProductState.Active;
		}

		public interface IProductInfo
		{
			string Name { get; set; }
			Guid ProductUid { get; set; }
			string SizeFullName { get; set; }
			Guid SizeUid { get; set; }
			string Metal { get; set; }
		}

		public interface IPriceGroupInfo
		{
			int? PriceGroupId { get; set; }
		}

		public interface IInstockInfo
		{
			int ProductId { get; set; }
			string Barcode { get; set; }
			decimal Weight { get; set; }
		}

		public interface IAdditionalInstockInfo
		{
			int CompanyId { get; set; }
			Guid ProductUid { get; set; }
			Guid SizeUid { get; set; }
		}

		public interface IBarcodeFilterInfo
		{
			List<int> CompanyIds { get; set; }
			List<int> ProductIds { get; set; }
		}
		public interface IBulkPublishInformationByCluster
		{
			int ClusterId { get; set; }
			int AllProduct { get; set; }
			int CompleteProduct { get; set; }
			int ErrorProduct { get; set; }
			DateTime DateStart { get; set; }
			DateTime DateEnd { get; set; }

		}
		public class Bulk_Publish_Information_By_Cluster_Mari : MariPrice.Client.Portal.BulkPublishInformationByCluster, IBulkPublishInformationByCluster { }

		public class Bulk_Publish_Information_By_Cluster_Spec : Mr.Avalon.Spec.Client.SpecApi.Product.BulkPublishInformationByCluster, IBulkPublishInformationByCluster { }
		public class MariPriceDb_Price_Product : MariPriceDb.Price.Product, SysDto.IId, ICompanyInfo, IEnabledInfo, IPnInfo, IProductDbStatus, IProductInfo, IPriceGroupInfo { }
		public class MariPriceDb_Price_ProductList_Item : MariPriceDb.Price.Product.ListOnlyProducts.Item, SysDto.IId, ICompanyInfo, IEnabledInfo, IPnInfo, IProductDbStatus, IProductInfo { }
		public class MariPriceApi_Price_Product : MariPriceApi.Price.Product, SysDto.IId, ICompanyInfo, IEnabledInfo, IPnInfo, IProductApiStatus, IProductInfo, IPriceGroupInfo { }

		public class MariPriceDb_Price_Instock : MariPriceDb.Price.Instock, IInstockInfo, IAdditionalInstockInfo { }
		public class MariPriceDb_Price_Instock_CreateItem : MariPriceDb.Price.Instock.Import.Item, IInstockInfo { }
		public class MariPriceApi_Price_Instock : MariPriceApi.Price.Instock, IInstockInfo, IAdditionalInstockInfo { }
		public class MariPriceApi_Price_Instock_CreateItem : MariPriceApi.Price.Instock.Import.Item, IInstockInfo { }

		public class MariPriceDb_Price_Instock_List : MariPriceDb.Price.Instock.List, IBarcodeFilterInfo { }
		public class MariPriceApi_Price_Instock_List : MariPriceApi.Price.Instock.List, IBarcodeFilterInfo { }
	}
}
