using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Api
{
	[Route("api/v1/portal/price")]
	public class PortalPriceController : Controller
	{
		PortalPriceEngine m_priceEngine;
		IRequestInfo m_infoPrincipal;

		public PortalPriceController(PortalPriceEngine priceEngine, IRequestInfo infoPrincipal)
		{
			m_priceEngine = priceEngine;
			m_infoPrincipal = infoPrincipal;
		}

		[HttpGet]
		[Authorize(Access.MariManager)]
		[Route("{companyId}")]
		public MariPriceApi.PortalPrice GetActivePrice(int companyId)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetActivePrice(companyId, userInfo);
		}

		[HttpGet]
		[Authorize(Access.MariManager)]
		[Route("draft/{companyId}")]
		public MariPriceApi.PortalPrice GetDraftPrice(int companyId)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetDraftPrice(companyId, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster")]
		public MariPriceApi.PortalPrice.Cluster.Info NewCluster([FromBody] MariPriceApi.PortalPrice.Cluster.Create request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.NewCluster(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster/pubishproduct")]
		public void PublishProduct([FromBody] MariPriceApi.PortalPrice.Cluster.PublishProduct request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			m_priceEngine.PublishProduct(request, userInfo);
		}


		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("swap")]
		public MariPriceApi.PortalPrice SwapCluster([FromBody] MariPriceApi.PortalPrice.PriceCompany.Swap request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ClusterSwap(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("loadactive")]
		public MariPriceApi.PortalPrice LoadActiveVersion([FromBody] MariPriceApi.PortalPrice.PriceCompany.LoadActiveVersion request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);

			return m_priceEngine.LoadActiveVerson(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster/update")]
		public MariPriceApi.PortalPrice.Cluster.Info UpdateCluster([FromBody] MariPriceApi.PortalPrice.Cluster.Update request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateCluster(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster/updateinstockflag")]
		public MariPriceApi.PortalPrice.Cluster.Info UpdateClusterInStockFlug([FromBody] MariPriceApi.PortalPrice.Cluster.UpdateInStockFlag request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateClusterInStockFlug(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster/updateinorderflag")]
		public MariPriceApi.PortalPrice.Cluster.Info UpdateInOrderFlag([FromBody] MariPriceApi.PortalPrice.Cluster.UpdateInOrderFlag request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateInOrderFlag(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster/updatestatus")]
		public MariPriceApi.PortalPrice.Cluster.Info UpdateClusterStatus([FromBody] MariPriceApi.PortalPrice.Cluster.UpdateStatus request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateClusterStatus(request, userInfo);
		}


		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("cluster/delete")]
		public MariPriceApi.PortalPrice DeleteCuster([FromBody] MariPriceApi.PortalPrice.Cluster.Delete request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.DeleteCluster(request, userInfo);
		}

		[HttpGet]
		[Authorize(Access.MariManager)]
		[Route("cluster")]
		public MariPriceApi.PortalPrice.Cluster GetCluster(int companyId, int clusterId)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetCluster(companyId, clusterId, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("settings/create")]
		public MariPriceApi.PortalPrice.Cluster NewSettingsItem([FromBody] MariPriceApi.PortalPrice.Cluster.Append request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.AddSettingsItem(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("settings/item/update")]
		public MariPriceApi.Price.ClusterSetting UpdateSettingsItem([FromBody] MariPriceApi.PortalPrice.Cluster.UpdateItem request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateSettingsItem(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("settings/item/delete")]
		public MariPriceApi.PortalPrice.Cluster.Info DeleteSettingsItem([FromBody] MariPriceApi.PortalPrice.Cluster.DeleteItem request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.DeleteSettingsItem(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/create")]
		public MariPriceApi.PortalPrice.Group NewGroup([FromBody] MariPriceApi.PortalPrice.Group.Create request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.NewGroup(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/update")]
		public MariPriceApi.PortalPrice.Group UpdateGroup([FromBody] MariPriceApi.PortalPrice.Group.Update request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateGroup(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/delete")]
		public MariPriceApi.PortalPrice.Cluster DeleteGroup([FromBody] MariPriceApi.PortalPrice.Group.Delete request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.DeleteGroup(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/value/update")]
		public MariPriceApi.PortalPrice.GroupValue UpdateGroupValue([FromBody] MariPriceApi.PortalPrice.GroupValue.Update request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateGroupValue(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/instockvalue/update")]
		public MariPriceApi.PortalPrice.InstockGroupValue UpdateGroupInstockValue([FromBody] MariPriceApi.PortalPrice.InstockGroupValue.Update request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateGroupInstockValue(request, userInfo);
		}

		[HttpGet]
		[Authorize(Access.MariSeller)]
		[Route("group/list")]
		public List<MariPriceApi.EntityName<int>> GetGroupsForCompanyAndFilter(int compnanyId, string groupName)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetGroupsForCompany(compnanyId, groupName, userInfo);
		}


		[HttpGet]
		[Authorize(Access.MariSeller)]
		[Route("group/{groupId}")]
		public MariPriceApi.PortalPrice.Group GetGroup(int groupId)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetGroupById(groupId, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariSeller)]
		[Route("group/list")]
		public List<MariPriceApi.EntityName<int>> GetGroupsBySas([FromBody] MariPriceApi.PortalPrice.Group.SasRequest request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(Request.HttpContext);
			var sasRequest = m_infoPrincipal.SasObject<MariPriceApi.PortalPrice.Group.SasRequest.SasSource>(Request.HttpContext, request.Sas);

			return m_priceEngine.GetGroupsBySas(sasRequest, request.Name);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("version/update")]
		public MariPriceApi.PortalPrice VersionUpdate([FromBody] MariPriceApi.PortalPrice.PriceCompanyVersion.Update request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateVersion(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("product/list")]
		public MariPriceApi.PortalPrice.Product GetProducts([FromBody] MariPriceApi.PortalPrice.Product.List request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetProductNamesForLinks(request, userInfo);
		}


		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("product/create")]
		public void CreateLinkProduct([FromBody] MariPriceApi.PortalPrice.Product.Create request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			m_priceEngine.CreateGroupLink1(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("product/delete")]
		public void DeleteLinkProduct([FromBody] MariPriceApi.PortalPrice.Product.Delete request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			m_priceEngine.DeleteGroupLink1(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("technologies/create")]
		public MariPriceApi.Price.Technologies.TechnologyPortal NewTechnology([FromBody] MariPriceApi.PortalPrice.TechnologiesAdditions.Create request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.AppendTechnologyPrices(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("technologies/loadactive")]
		public MariPriceApi.Price.Technologies.TechnologyPortal LoadActiveTechnology([FromBody] MariPriceApi.PortalPrice.TechnologiesAdditions.LoadActiveVersion request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.LoadActiveTechnology(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("technologies/update")]
		public MariPriceApi.Price.Technologies.TechnologyPortal UpdateTechnology([FromBody] MariPriceApi.PortalPrice.TechnologiesAdditions.Update request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.UpdateTechnologyPrices(request, userInfo);
		}

		[HttpGet]
		[Authorize(Access.MariManager)]
		[Route("technologies/active/{versionId}")]
		public MariPriceApi.PortalPrice.TechnologiesAdditions GetTechnologyActive(int versionId)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetTechnologiesPricesActive(versionId, userInfo);
		}

		[HttpGet]
		[Authorize(Access.MariManager)]
		[Route("technologies/draft/{versionId}")]
		public MariPriceApi.PortalPrice.TechnologiesAdditions GetTechnologyDraft(int versionId)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.GetTechnologiesPricesDraft(versionId, userInfo);
		}


		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("technologies/delete")]
		public MariPriceApi.PortalPrice.TechnologiesAdditions DeleteGroup([FromBody] MariPriceApi.PortalPrice.TechnologiesAdditions.Delete request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.DeleteTechnologyPrices(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("instock/import")]
		public MariPriceApi.PortalPrice.Instock.ImportRequest.Result BarcodesImport([FromBody] MariPriceApi.PortalPrice.Instock.ImportRequest request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ImportBarcodes(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("instock/export")]
		public MariPriceApi.PortalPrice.Instock.Export.Result BarcodesExport([FromBody] MariPriceApi.PortalPrice.Instock.Export request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ExportBarcodes(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/import")]
		public MariPriceApi.PortalPrice.Group.Import.Response ImportPriceGroup([FromBody] MariPriceApi.PortalPrice.Group.Import request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ImportPriceGroup(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("group/export")]
		public MariPriceApi.PortalPrice.Group.Export.Response ExportPriceGroup([FromBody] MariPriceApi.PortalPrice.Group.Export request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ExportPriceGroup(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("product/import")]
		public MariPriceApi.PortalPrice.Product.Import.Response ExportPriceProduct([FromBody] MariPriceApi.PortalPrice.Product.Import request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ImportPriceProduct(request, userInfo);
		}

		[HttpPost]
		[Authorize(Access.MariManager)]
		[Route("product/export")]
		public MariPriceApi.PortalPrice.Product.Export.Response ExportPriceProduct([FromBody] MariPriceApi.PortalPrice.Product.Export request)
		{
			var userInfo = m_infoPrincipal.GetUserInfo(HttpContext);
			return m_priceEngine.ExportPriceProduct(request, userInfo);
		}
	}
}
