using d7k.Dto;
using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.Print.Client;
using Mr.Avalon.Print.Doc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mr.Avalon.MariPrice.Core
{
	public class PortalPriceInstockEngine
	{
		DtoComplex m_dto;
		IBarcodeStorage m_barcodeStorage;
		PriceEngine m_priceEngine;
		PrintApiClient m_print;

		public PortalPriceInstockEngine(DtoComplex dto, IBarcodeStorage barcodeStorage, PrintApiClient print, PriceEngine priceEngine)
		{
			m_dto = dto;
			m_barcodeStorage = barcodeStorage;
			m_priceEngine = priceEngine;
			m_print = print;
		}

		public MariPriceApi.PortalPrice.Instock.ImportRequest.Result ImportBarcodes(MariPriceApi.PortalPrice.Instock.ImportRequest request, UserInfo userInfo)
		{
			var newBarcodes = m_barcodeStorage.ReadOneDownloadSession(request.DownloadSessionId);

			if (newBarcodes?.Any(x => x.SaleCompany != request.CompanyId) == true)
			{
				throw new ConflictApiException("Different companyIds in request and download session");
			}

			var printReport = new MariInstockImportReport
			{
				Barcodes = new List<MariInstockImportReport.Item>()
			};

			var productRequest = new MariPriceApi.Price.Product.List().ForCompanies(request.CompanyId);
			var src = m_priceEngine.GetOnlyProductsWithoutAdditionalInfo(productRequest);
			var activeProducts = src
				.Where(x => x.SizeFullName != null)
				.GroupBy(x => x.Pn)
				.ToDictionary(x => x.Key, 
					x => x.GroupBy(s => s.SizeFullName)
					.ToDictionary(s => s.Key, s => s.First(), StringComparer.InvariantCultureIgnoreCase));

			var setBarcodesRequest = new MariPriceApi.Price.Instock.Import
			{
				CompanyId = request.CompanyId
			};

			foreach (var newBarcode in newBarcodes)
			{
				var newSizeBarcodeFullName = PriceProductEngine.GetSizeFullName(newBarcode.Size, newBarcode.WireThickness);
				var reportItem = GetReportItem(newBarcode);
				if (activeProducts.TryGetValue(newBarcode.ProductPn, out var productDictionary) &&
					productDictionary.TryGetValue(newSizeBarcodeFullName, out var product))
				{
					setBarcodesRequest.NewBarcodes.Add(new MariPriceApi.Price.Instock.Import.Item
					{
						Barcode = newBarcode.Barcode,
						ProductId = product.Id,
						Weight = newBarcode.Weight
					});
					reportItem.Report = "Успешно загружен";
				}
				else
				{
					reportItem.Report = "Не найден соответствующий продукт";
				}
				printReport.Barcodes.Add(reportItem);
			}

			m_priceEngine.SetBarcodes(setBarcodesRequest);

			var printUrl = m_print.CreateDoc(printReport, userInfo.SessionId, userInfo.Expired);

			return new MariPriceApi.PortalPrice.Instock.ImportRequest.Result
			{
				ReportUrl = printUrl
			};
		}

		private MariInstockImportReport.Item GetReportItem(BarcodeTableEntity newBarcode)
		{
			return new MariInstockImportReport.Item
			{
				Barcode = newBarcode.Barcode,
				ProductPn = newBarcode.ProductPn,
				Size = newBarcode.Size,
				SizePn = newBarcode.SizePn,
				Weight = newBarcode.Weight,
				WireThickness = newBarcode.WireThickness
			};
		}

		public MariPriceApi.PortalPrice.Instock.Export.Result ExportBarcodes(MariPriceApi.PortalPrice.Instock.Export request, UserInfo userInfo)
		{
			var result = new MariPriceApi.PortalPrice.Instock.Export.Result();

			result.Url = m_priceEngine.ExportBarcodes(new MariPriceApi.Price.Instock.Export()
			{
				CompanyId = request.CompanyId,
				VersionId = request.VersionId
			}, userInfo).Url;

			return result;
		}
	}
}
