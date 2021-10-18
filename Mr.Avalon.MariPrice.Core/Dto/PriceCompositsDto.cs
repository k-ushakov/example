using d7k.Dto;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	[DtoContainer]
	public static class PriceCompositsDto
	{
		public interface IInstockInfo
		{
			string Barcode { get; set; }
			int ProductId { get; set; }
			decimal Weight { get; set; }
		}

		public interface IPriceClusterApiInfo
		{
			List<MariPriceApi.Price.ClusterSetting> VariantsSettings { get; set; }
		}
		public interface IPriceClusterPortalInfo
		{
			List<MariPriceApi.Price.ClusterSetting> Variants { get; set; }
		}

		#region PriceSettingsApiInfo

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IPriceClusterApiInfo> fact)
		{
			fact.RuleFor(x => x.VariantsSettings.ScanAll().OrderMetalWeight).NotLesser(0);
			fact.RuleFor(x => x.VariantsSettings.ScanAll().ProductionTime).NotLesser(0);
		}

		[DtoConvert]
		static void Convert(IPriceClusterPortalInfo dst, IPriceClusterApiInfo src)
		{
			dst.Variants = src.VariantsSettings;
		}

		#endregion

		public interface IGuidInfo
		{
			Guid Id { get; set; }
		}

		public interface IPriceSettingsItemInfo
		{
			decimal OrderMetalWeight { get; set; }
			int ProductionTime { get; set; }
		}

		public interface IClusterFlugs
		{
			bool Enabled { get; set; }
			bool InStock { get; set; }
			bool InOrder { get; set; }
		}

		#region PriceSettingsItemInfo

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IPriceSettingsItemInfo> fact)
		{
			fact.RuleFor(x => x.OrderMetalWeight).NotLesser(0);
			fact.RuleFor(x => x.ProductionTime).NotLesser(0);
		}

		#endregion

		public interface IVersionId
		{
			int VersionId { get; set; }
		}

		public interface IClusterNameGuidInfo
		{
			Guid Name { get; set; }
		}

		public interface IMetalGuidInfo
		{
			Guid? Metal { get; set; }
			Guid? Quality { get; set; }
		}

		public interface IPriceGroupValue
		{
			List<MariPriceApi.Price.GroupValue> Values { get; set; }
			MariPriceApi.Price.InstockGroupValue InStockValues { get; set; }
		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IPriceGroupValue> fact)
		{
			fact.RuleFor(x => x.Values.ScanAll()).ValidateDto();
		}

		public interface INdsPricesInfo
		{
			decimal? WithNdsPrice { get; set; }
			decimal? WithNdsMarkup { get; set; }
			decimal? WithoutNdsPrice { get; set; }
			decimal? WithoutNdsMarkup { get; set; }
		}

		public interface INdsFlagsInfo
		{
			bool WithNdsPriceRequired { get; set; }
			bool WithoutNdsPriceRequired { get; set; }
		}

		#region PriceGroupApiInfo

		[DtoValidate]
		static void Validate(ValidationRuleFactory<INdsPricesInfo> fact)
		{
			fact.RuleForIf(x => x.WithNdsPrice, x => x.WithNdsPrice.HasValue).NotLesser(0);
			fact.RuleForIf(x => x.WithoutNdsPrice, x => x.WithoutNdsPrice.HasValue).NotLesser(0);
		}

		#endregion

		public interface IClusterIdInfo
		{
			int ClusterId { get; set; }
		}

		public interface IPriceGroupNameInfo
		{
			string Name { get; set; }
		}
		public interface IPriceGroupDispleyNameInfo
		{
			string DisplayName { get; set; }
		}

		public interface IPriceGroupLostInfo
		{
			decimal LossPercentage { get; set; }
			decimal? AdditionalLossPercentage { get; set; }
		}
		public interface IPriceGroupLostCalculatedInfo
		{
			decimal? TotalLossPercentage { get; set; }
		}

		public interface IPriceGroupWithVersion
		{
			int VersionId { get; set; }
			int ActiveVersionId { get; set; }
			int DraftVersionId { get; set; }

		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IPriceGroupLostInfo> fact)
		{
			fact.RuleFor(x => x.LossPercentage).NotLesser(0);
			fact.RuleFor(x => x.AdditionalLossPercentage).NotLesser(0);
		}

		public interface ITechnologyGuidInfo
		{
			Guid TechnologyId { get; set; }
		}

		public interface ITechnologPriceInfo
		{
			decimal? WithNdsPrice { get; set; }
			decimal? WithoutNdsPrice { get; set; }
		}

		public interface ICompanyId
		{
			int CompanyId { get; set; }
		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<ICompanyId> fact)
		{
			fact.RuleFor(x => x.CompanyId).Greater(0);
		}

		public interface IVersionInfo
		{
			int ActiveVersionId { get; set; }
			int DraftVersionId { get; set; }
		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IVersionInfo> fact)
		{
			fact.RuleFor(x => x.ActiveVersionId).Greater(0);
			fact.RuleFor(x => x.DraftVersionId).Greater(0);
		}

		public interface IPriceGroupId
		{
			int PriceGroupId { get; set; }
		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IPriceGroupId> fact)
		{
			fact.RuleFor(x => x.PriceGroupId).Greater(0);
		}

		public interface IPriceClusterSettingIdId
		{
			int PriceClusterVariantId { get; set; }
		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IPriceClusterSettingIdId> fact)
		{
			fact.RuleFor(x => x.PriceClusterVariantId).Greater(0);
		}

		public interface IGroupValueId
		{
			int PriceGroupValueId { get; set; }
		}

		[DtoValidate]
		static void Validate(ValidationRuleFactory<IGroupValueId> fact)
		{
			fact.RuleFor(x => x.PriceGroupValueId).Greater(0);
		}

		public interface IPublishProductInfo
		{
			string ProductPublishInfo { get; set; }
		}

		public class MariDb_PriceCluster : MariPriceDb.Price.Cluster, IClusterFlugs, SysDto.IId, IVersionId, IClusterNameGuidInfo, IMetalGuidInfo { }
		public class MariApi_PriceCluster : MariPriceApi.Price.Cluster, IClusterFlugs, IPriceClusterApiInfo, SysDto.IId, IVersionId, IClusterNameGuidInfo, IMetalGuidInfo { }
		public class MariPortal_PriceCluster_Info : MariPriceApi.PortalPrice.Cluster.Info, IClusterFlugs, IPriceClusterPortalInfo, SysDto.IId, IVersionId { }

		public class MariDb_PriceCluster_Create : MariPriceDb.Price.Cluster.Create, IClusterFlugs, IVersionId, IClusterNameGuidInfo, IMetalGuidInfo { }
		public class MariApi_PriceCluster_Create : MariPriceApi.Price.Cluster.Create, IVersionId, IClusterFlugs, IClusterNameGuidInfo, IMetalGuidInfo { }
		public class MariApiPortal_PriceCluster_Create : MariPriceApi.PortalPrice.Cluster.Create, IVersionId { }

		public class MariDb_PriceCluster_Update : MariPriceDb.Price.Cluster.Update, SysDto.IId, IClusterFlugs, IClusterNameGuidInfo, IMetalGuidInfo { }
		public class MariApi_PriceCluster_Update : MariPriceApi.Price.Cluster.Update, IPriceClusterApiInfo, IClusterFlugs, SysDto.IId, IClusterNameGuidInfo, IMetalGuidInfo { }

		public class MariPortal_PriceCluster_Variant_Append : MariPriceApi.PortalPrice.Cluster.Append, IPriceSettingsItemInfo { }
		public class MariApi_PriceCluster_Variant : MariPriceApi.Price.ClusterSetting, IPriceSettingsItemInfo, SysDto.IId { }
		public class MariPortal_PriceCluster_Variant_Update : MariPriceApi.PortalPrice.Cluster.UpdateItem, IPriceSettingsItemInfo { }

		public class MariDb_PriceGroup : MariPriceDb.Price.Group, SysDto.IId, IClusterIdInfo, IPriceGroupDispleyNameInfo, IPriceGroupNameInfo, IPriceGroupLostInfo { }
		public class MariApi_PriceGroup : MariPriceApi.Price.Group, IPriceGroupValue, IPublishProductInfo, SysDto.IId, IClusterIdInfo, IPriceGroupDispleyNameInfo, IPriceGroupNameInfo, IPriceGroupLostInfo, IPriceGroupLostCalculatedInfo { }

		public class MariDb_PriceGroupWithVersion : MariPriceDb.Price.GroupWithVersion, SysDto.IId, IClusterIdInfo, IPriceGroupDispleyNameInfo, IPriceGroupNameInfo, IPriceGroupLostInfo, IPriceGroupWithVersion { }
		public class MariApi_PriceGroupWithVersion : MariPriceApi.Price.GroupWithVersion, IPriceGroupValue, IPublishProductInfo, SysDto.IId, IClusterIdInfo, IPriceGroupDispleyNameInfo, IPriceGroupNameInfo, IPriceGroupLostInfo, IPriceGroupLostCalculatedInfo, IPriceGroupWithVersion { }

		public class MariPortal_PriceGroup : MariPriceApi.PortalPrice.Group, IPublishProductInfo, IPriceGroupValue, SysDto.IId, IClusterIdInfo, IPriceGroupDispleyNameInfo, IPriceGroupNameInfo, IPriceGroupLostInfo, IPriceGroupLostCalculatedInfo { }

		public class MariDb_PriceGroup_Create : MariPriceDb.Price.Group.Create, IClusterIdInfo, IPriceGroupNameInfo, IPriceGroupDispleyNameInfo, IPriceGroupLostInfo { }
		public class MariApi_PriceGroup_Create : MariPriceApi.Price.Group.Create, IClusterIdInfo, IPriceGroupNameInfo, IPriceGroupDispleyNameInfo, IPriceGroupLostInfo { }
		public class MariPortal_PriceGroup_Create : MariPriceApi.PortalPrice.Group.Create, IClusterIdInfo, IPriceGroupNameInfo, IPriceGroupDispleyNameInfo, IPriceGroupLostInfo { }

		public class MariDb_PriceGroup_Update : MariPriceDb.Price.Group.Update, SysDto.IId, IPriceGroupNameInfo, IPriceGroupDispleyNameInfo, IPriceGroupLostInfo { }
		public class MariApi_PriceGroup_Update : MariPriceApi.Price.Group.Update, IPriceGroupValue, SysDto.IId, IPriceGroupNameInfo, IPriceGroupDispleyNameInfo, IPriceGroupLostInfo { }
		public class MariPortal_PriceGroup_Update : MariPriceApi.PortalPrice.Group.Update, SysDto.IId, IPriceGroupNameInfo, IPriceGroupDispleyNameInfo, IPriceGroupLostInfo { }

		//TO DO
		public class MariDb_PriceTechnologies : MariPriceDb.Price.CompanyTechnology, IVersionId, ITechnologPriceInfo, ITechnologyGuidInfo { }

		public class MariApi_PriceTechnologies_Api_Item : MariPriceApi.Price.Technologies.TechnologyApi, ITechnologPriceInfo, ITechnologyGuidInfo { }
		public class MariApi_PriceTechnologies_Portal_Item : MariPriceApi.Price.Technologies.TechnologyPortal, ITechnologPriceInfo, ITechnologyGuidInfo { }

		//Portal
		public class MariApi_PriceTechnologies_Portal_Create : MariPriceApi.Price.Technologies.Create, ITechnologPriceInfo, ITechnologyGuidInfo { }
		public class MariApi_PriceTechnologies_Api_Create : MariPriceApi.Price.Technologies.Create, ITechnologPriceInfo, ITechnologyGuidInfo { }
		public class MariApi_PriceTechnologies_Db_Create : MariPriceDb.Price.CompanyTechnology.Create, ITechnologPriceInfo, ITechnologyGuidInfo { }

		//Portal
		public class MariApi_PriceTechnologies_Portal_Update : MariPriceApi.Price.Technologies.Set, ITechnologPriceInfo, ITechnologyGuidInfo { }
		public class MariApi_PriceTechnologies_Api_Update : MariPriceApi.Price.Technologies.Set, ITechnologPriceInfo, ITechnologyGuidInfo { }
		public class MariApi_PriceTechnologies_Db_Update : MariPriceDb.Price.CompanyTechnology.Update, ITechnologPriceInfo, ITechnologyGuidInfo { }

		public class MariApi_PriceCompany : MariPriceApi.Price.PriceCompany, IVersionInfo, ICompanyId { }
		public class MariDbApi_PriceCompany : MariPriceDb.Price.Company, IVersionInfo, ICompanyId { }

		public class MariDbApi_PriceCompany_Create : MariPriceDb.Price.Company.Create, IVersionInfo, ICompanyId { }
		public class MariDbApi_PriceCompany_ListItem : MariPriceDb.Price.Company.List.Item, IVersionInfo, ICompanyId { }
		public class MariDbApi_PriceCompany_ListByVersionItem : MariPriceDb.Price.Company.ListByVersion.Item, IVersionInfo, ICompanyId { }
		public class MariDbApi_PriceCompany_Swap : MariPriceDb.Price.Company.Swap, ICompanyId { }

		public class MariDbApi_PriceVersion_ListItem : MariPriceDb.Price.PriceVersion.List.Item, IVersionId, INdsFlagsInfo { }
		public class MariDbApi_PriceVersion_Create : MariPriceDb.Price.PriceVersion.Create, INdsFlagsInfo { }
		public class MariDbApi_PriceVersion_Update : MariPriceDb.Price.PriceVersion.Update, IVersionId, INdsFlagsInfo { }

		public class MariApi_PriceVersion_Create : MariPriceApi.Price.PriceCompanyVersion.Create, INdsFlagsInfo { }
		public class MariApi_PriceVersion_Update : MariPriceApi.Price.PriceCompanyVersion.Update, IVersionId, INdsFlagsInfo { }
		public class MariApi_PriceVersion : MariPriceApi.Price.PriceCompanyVersion, IVersionId, INdsFlagsInfo { }
		public class MariApi_Portal_PriceVersion_Update : MariPriceApi.PortalPrice.PriceCompanyVersion.Update, IVersionId, INdsFlagsInfo { }

		public class MariApo_Price_CusterSetting : MariPriceApi.Price.ClusterSetting.Create, IClusterIdInfo, IPriceSettingsItemInfo { }
		public class MariApo_Price_CusterSetting_Update : MariPriceApi.Price.ClusterSetting.Update, SysDto.IId, IPriceSettingsItemInfo { }
		public class MariApo_Price_CusterSetting_ListItem : MariPriceApi.Price.ClusterSetting, SysDto.IId, IPriceSettingsItemInfo { }

		public class MariDb_Price_CluserSettings_Create : MariPriceDb.Price.ClusterSetting.Create, IClusterIdInfo, IPriceSettingsItemInfo { }
		public class MariDb_Price_CluserSettings_Update : MariPriceDb.Price.ClusterSetting.Update, SysDto.IId, IPriceSettingsItemInfo { }
		public class MariDb_Price_CluserSettings_ListItem : MariPriceDb.Price.ClusterSetting, SysDto.IId, IClusterIdInfo, IPriceSettingsItemInfo { }

		public class MariApi_Price_CusterSettingAppend : MariPriceApi.PortalPrice.Cluster.Append, IClusterIdInfo, IPriceSettingsItemInfo { }

		public class MariApi_Price_GroupValueCreate : MariPriceApi.Price.GroupValue.Create, INdsPricesInfo, IPriceClusterSettingIdId, IPriceGroupId { }
		public class MariApi_Price_GroupValueUpdate : MariPriceApi.Price.GroupValue.Update, INdsPricesInfo, IGroupValueId { }
		public class MariApi_Price_GroupValue : MariPriceApi.Price.GroupValue, IGroupValueId, INdsPricesInfo, IPriceClusterSettingIdId, IPriceGroupId { }

		public class MariPortal_Price_GroupValue : MariPriceApi.PortalPrice.GroupValue, IPriceClusterSettingIdId, IGroupValueId, INdsPricesInfo { }
		public class MariPortal_Price_GroupValueUpdate : MariPriceApi.PortalPrice.GroupValue.Update, IGroupValueId, INdsPricesInfo { }

		public class MariDb_Price_GroupValueCreate : MariPriceDb.Price.GroupValue.Create, INdsPricesInfo, IPriceClusterSettingIdId, IPriceGroupId { }
		public class MariDb_Price_GroupValueUpdate : MariPriceDb.Price.GroupValue.Update, INdsPricesInfo, IGroupValueId { }
		public class MariDb_Price_GroupValue : MariPriceDb.Price.GroupValue, IGroupValueId, INdsPricesInfo, IPriceClusterSettingIdId, IPriceGroupId { }

		public class MariApi_Price_GroupInstockValueCreate : MariPriceApi.Price.InstockGroupValue.Create, INdsPricesInfo, IPriceGroupId { }
		public class MariApi_Price_GroupInstockValueUpdate : MariPriceApi.Price.InstockGroupValue.Update, INdsPricesInfo, IPriceGroupId { }
		public class MariApi_Price_GroupInstockValue : MariPriceApi.Price.InstockGroupValue, INdsPricesInfo, IPriceGroupId { }

		public class MariPortal_Price_GroupInstockValue : MariPriceApi.PortalPrice.InstockGroupValue, IPriceGroupId, INdsPricesInfo { }
		public class MariPortal_Price_GroupInstockValueUpdate : MariPriceApi.PortalPrice.InstockGroupValue.Update, IPriceGroupId, INdsPricesInfo { }

		public class MariDb_Price_GroupInstockValueCreate : MariPriceDb.Price.InstockGroupValue.Create, INdsPricesInfo, IPriceGroupId { }
		public class MariDb_Price_GroupInstockValueUpdate : MariPriceDb.Price.InstockGroupValue.Update, INdsPricesInfo, IPriceGroupId { }
		public class MariDb_Price_GroupInstockValue : MariPriceDb.Price.InstockGroupValue, INdsPricesInfo, IPriceGroupId { }

		public class MariApi_PortalPrice_ProductLst : MariPriceApi.PortalPrice.Product.List, SysDto.IPageInfoSource { }
		public class MariDb_Price_Product_Search : MariPriceDb.Price.Product.Search, SysDto.IDbPageInfo { }

		public class MariDb_Price_Company_Swap : MariPriceDb.Price.Company.Swap, ICompanyId { }
		public class MariPriceApi_Price_Company_Swap : MariPriceApi.Price.PriceCompany.Swap, ICompanyId { }
		public class MariPriceApi_PortalPrice_Company_Swap : MariPriceApi.PortalPrice.PriceCompany.Swap, ICompanyId { }

		public class MariDb_Price_Instock : MariPriceDb.Price.Instock, IInstockInfo { }
		public class MariPriceApi_Price_Instock : MariPriceApi.Price.Instock, IInstockInfo { }
	}
}
