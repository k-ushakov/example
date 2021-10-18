using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.MariPrice.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Api
{
	[Route("api/v1/price")]
	public class PriceController : Controller
	{
		PriceEngine m_priceEngine;

		public PriceController(PriceEngine priceEngine)
		{
			m_priceEngine = priceEngine;
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("aggregate")]
		public MariPriceApi.Price.AggregatePrice GetAggregatePriceInfo([FromBody] MariPriceApi.Price.AggregatePrice.Request request)
		{
			return m_priceEngine.GetAggregatePriceInfoForProducts(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("cluster/create")]
		public MariPriceApi.Price.Cluster CreateClaster([FromBody] MariPriceApi.Price.Cluster.Create request)
		{
			var id = m_priceEngine.CreateCluster(request);

			return m_priceEngine.GetCluster(new MariPriceApi.Price.Cluster.List().ForIds(id)).Single();
		}

		[HttpGet]
		[Authorize(Access.MariPriceManagement)]
		[Route("company/{companyId}")]
		public MariPriceApi.Price.PriceCompany GetCompanyVersions(int companyId)
		{
			return m_priceEngine.GetCompanyPrice(companyId);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("company/createversion")]
		public MariPriceApi.Price.PriceCompany CreateVersion([FromBody] MariPriceApi.Price.PriceCompany.Create request)
		{
			return m_priceEngine.CreateCompanyVersion(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("companyversion/update")]
		public void UpdateVersion([FromBody] MariPriceApi.Price.PriceCompanyVersion.Update request)
		{
			m_priceEngine.UpdateVersion(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("pricecontent")]
		public List<MariPriceApi.Price> GetProductPrice([FromBody] MariPriceApi.Price.Product.PriceContent.List request)
		{
			return m_priceEngine.GetProductPriceContent(request);
		}

		[HttpGet]
		[Authorize(Access.MariPriceManagement)]
		[Route("companyversion/{versionId}")]
		public MariPriceApi.Price.PriceCompanyVersion GetVersion(int versionId)
		{
			return m_priceEngine.GetVersion(versionId);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("cluster/update")]
		public void UpdateCluster([FromBody] MariPriceApi.Price.Cluster.Update request)
		{
			m_priceEngine.UpdateCluster(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("cluster")]
		public List<MariPriceApi.Price.Cluster> GetCluster([FromBody] MariPriceApi.Price.Cluster.List request)
		{
			return m_priceEngine.GetCluster(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("swap")]
		public void Swap([FromBody] MariPriceApi.Price.PriceCompany.Swap request)
		{
			m_priceEngine.ClusterSwap(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("cluster/createsetting")]
		public MariPriceApi.Price.ClusterSetting SettingsCreate([FromBody] MariPriceApi.Price.ClusterSetting.Create request)
		{
			var id = m_priceEngine.CreateSetting(request);
			return m_priceEngine.GetSetting(new MariPriceApi.Price.ClusterSetting.List().ForIds(id)).FirstOrDefault();
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("cluster/updatesetting")]
		public void SettingsUpdate([FromBody] MariPriceApi.Price.ClusterSetting.Update request)
		{
			m_priceEngine.UpdateSetting(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("cluster/deletesetting")]
		public void DeleteUpdate([FromBody] MariPriceApi.Price.ClusterSetting.Delete request)
		{
			m_priceEngine.DeleteSetting(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("clustersetting")]
		public List<MariPriceApi.Price.ClusterSetting> SettingsGet([FromBody] MariPriceApi.Price.ClusterSetting.List request)
		{
			return m_priceEngine.GetSetting(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group")]
		public List<MariPriceApi.Price.Group> GroupGet([FromBody] MariPriceApi.Price.Group.List request)
		{
			return m_priceEngine.GetGroups(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group-with-version")]
		public List<MariPriceApi.Price.GroupWithVersion> GroupWithVersionGet([FromBody] MariPriceApi.Price.Group.List request)
		{
			return m_priceEngine.GetGroupsWithVersion(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/create")]
		public MariPriceApi.Price.Group GroupCreate([FromBody] MariPriceApi.Price.Group.Create request)
		{
			var id = m_priceEngine.CreateGroup(request);
			return m_priceEngine.GetGroups(new MariPriceApi.Price.Group.List().ForIds(id)).FirstOrDefault();
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/update")]
		public void GroupUpdate([FromBody] MariPriceApi.Price.Group.Update request)
		{
			m_priceEngine.UpdateGroup(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/value/create")]
		public MariPriceApi.Price.GroupValue GroupValueCreate([FromBody] MariPriceApi.Price.GroupValue.Create request)
		{
			var id = m_priceEngine.CreateGroupValue(request);
			return m_priceEngine.GetGroupsValues(new MariPriceApi.Price.GroupValue.List().ForIds(id)).FirstOrDefault();
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/value/update")]
		public void GroupValueUpdate([FromBody] MariPriceApi.Price.GroupValue.Update request)
		{
			m_priceEngine.UpdateGroupValue(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/value/delete")]
		public void GroupValueDelete([FromBody] MariPriceApi.Price.GroupValue.Delete request)
		{
			m_priceEngine.DeleteGroupValue(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/value")]
		public List<MariPriceApi.Price.GroupValue> GetGroupVaue([FromBody] MariPriceApi.Price.GroupValue.List request)
		{
			return m_priceEngine.GetGroupsValues(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("product")]
		public List<MariPriceApi.Price.Product> SyncProducts([FromBody] MariPriceApi.Price.Product.Sync request)
		{
			m_priceEngine.SyncProducts(request);
			return m_priceEngine.GetProducts(new MariPriceApi.Price.Product.List().ForProducts(request.Products.Select(x => x.ProductUid).ToArray()));
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("product/list")]
		public List<MariPriceApi.Price.Product> GetProducts([FromBody] MariPriceApi.Price.Product.List request)
		{
			return m_priceEngine.GetProducts(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("product/link")]
		public void SetProductLinks([FromBody] MariPriceApi.Price.Product.Link request)
		{
			m_priceEngine.SetLinks(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("product/updatestatus")]
		public void UpdateStatus([FromBody] MariPriceApi.Price.Product.UpdateStatus request)
		{
			m_priceEngine.UpdateProdutStatus(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("technologies")]
		public List<MariPriceApi.Price.Technologies> Get([FromBody] MariPriceApi.Price.Technologies.List request)
		{
			return m_priceEngine.GetTechnologyPrices(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("technologies/create")]
		public MariPriceApi.Price.Technologies SetTechnology([FromBody] MariPriceApi.Price.Technologies.Create request)
		{
			return m_priceEngine.CreateTechnologiesPrice(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("technologies/set")]
		public MariPriceApi.Price.Technologies SetTechnology([FromBody] MariPriceApi.Price.Technologies.Set request)
		{
			return m_priceEngine.SetTechnologiesPrice(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("instock/import")]
		public void BarcodesImport([FromBody]MariPriceApi.Price.Instock.Import request)
		{
			m_priceEngine.SetBarcodes(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("instock/export")]
		public MariPriceApi.Price.Instock.Export.Result BarcodesExport([FromBody]MariPriceApi.Price.Instock.Export request)
		{
			return m_priceEngine.ExportBarcodes(request, null);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("instock/available")]
		public List<MariPriceApi.Price.Instock> GetAvailableBarcodes([FromBody]MariPriceApi.Price.Instock.List request)
		{
			return m_priceEngine.GetBarcodes(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/instockvalue/create")]
		public MariPriceApi.Price.InstockGroupValue GroupInstockValueCreate([FromBody] MariPriceApi.Price.InstockGroupValue.Create request)
		{
			m_priceEngine.CreateInstockGroupValue(request);
			return m_priceEngine.GetGroupsInstockValues(new MariPriceApi.Price.InstockGroupValue.List().ForGroups(request.PriceGroupId)).FirstOrDefault();
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/instockvalue/update")]
		public void GroupInstockValueUpdate([FromBody] MariPriceApi.Price.InstockGroupValue.Update request)
		{
			m_priceEngine.UpdateGroupInstockValue(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/instockvalue/delete")]
		public void GroupInstockValueUpdate([FromBody] MariPriceApi.Price.InstockGroupValue.Delete request)
		{
			m_priceEngine.DeleteGroupInstockValue(request);
		}

		[HttpPost]
		[Authorize(Access.MariPriceManagement)]
		[Route("group/instockvalue")]
		public List<MariPriceApi.Price.InstockGroupValue> GetGroupInstockVaue([FromBody] MariPriceApi.Price.InstockGroupValue.List request)
		{
			return m_priceEngine.GetGroupsInstockValues(request);
		}
	}
}
