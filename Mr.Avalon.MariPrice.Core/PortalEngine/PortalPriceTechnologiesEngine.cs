using d7k.Dto;
using d7k.Dto.Utilities;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mr.Avalon.MariPrice.Core
{
	public class PortalPriceTechnologiesEngine
	{
		private PriceEngine m_engine;
		private DtoComplex m_dto;
		private ISpecEngine m_specEngine;
		private SpecSettings m_specSettings;

		public PortalPriceTechnologiesEngine(PriceEngine engine, DtoComplex dto, ISpecEngine specEngine, SpecSettings specSettings)
		{
			m_engine = engine;
			m_dto = dto;
			m_specEngine = specEngine;
			m_specSettings = specSettings;
		}

		public MariPriceApi.Price.Technologies.TechnologyPortal Create(MariPriceApi.PortalPrice.TechnologiesAdditions.Create request)
		{
			var setRequest = new MariPriceApi.Price.Technologies.Create
			{
				VersionId = request.VersionId,
				TechnologyId = request.TechnologyId,
				WithNdsPrice = request.WithNdsPrice,
				WithoutNdsPrice = request.WithoutNdsPrice
			};
			m_engine.CreateTechnologiesPrice(setRequest);
			return Get(request.VersionId).Technologies.Single(x => x.TechnologyId == request.TechnologyId);
		}

		public MariPriceApi.Price.Technologies.TechnologyPortal Update(MariPriceApi.PortalPrice.TechnologiesAdditions.Update request)
		{
			var apiRequest = new MariPriceApi.Price.Technologies.List().ForVersions(request.VersionId);
			var technologies = m_engine.GetTechnologyPrices(apiRequest).SingleOrDefault(x => x.TechnologyId == request.TechnologyId);
			if (technologies == null)
				throw new RecordNotFoundApiException($"There is no such technologies for Version: {request.VersionId} with id {request.TechnologyId}");


			var setRequest = new MariPriceApi.Price.Technologies.Set
			{
				VersionId = request.VersionId,
				TechnologyId = request.TechnologyId,
				WithNdsPrice = request.WithNdsPrice,
				WithoutNdsPrice = request.WithoutNdsPrice
			};

			m_engine.SetTechnologiesPrice(setRequest);

			return Get(request.VersionId).Technologies.Single(x => x.TechnologyId == request.TechnologyId);
		}

		public MariPriceApi.PortalPrice.TechnologiesAdditions Get(int versionId)
		{
			var result = new MariPriceApi.PortalPrice.TechnologiesAdditions();
			var technologies = m_engine.GetTechnologyPrices(new MariPriceApi.Price.Technologies.List().ForVersions(versionId));
			if (technologies == null || !technologies.Any())
				return result;

			var ids = technologies.Select(x => x.TechnologyId).Distinct().ToArray();
			var techNames = m_specEngine.GetVocValues(m_specSettings.Technologies, ids)
				.Items.ToDictionary(x => x.Id, x => x.Value);

			result.Technologies = technologies.Select(x =>
				new MariPriceApi.Price.Technologies.TechnologyPortal
				{
					TechnologyId = x.TechnologyId,
					Name = techNames.Get(x.TechnologyId),
					WithNdsPrice = x.WithNdsPrice,
					WithoutNdsPrice = x.WithoutNdsPrice
				}).ToList();

			return result;
		}

		public MariPriceApi.PortalPrice.TechnologiesAdditions Delete(MariPriceApi.PortalPrice.TechnologiesAdditions.Delete request)
		{
			var apiRequest = new MariPriceApi.Price.Technologies.List().ForVersions(request.VersionId);
			var technologies = m_engine.GetTechnologyPrices(apiRequest).FirstOrDefault(x => x.TechnologyId == request.TechnologyId);
			if (technologies == null)
				throw new RecordNotFoundApiException($"There is no such technologies for version: {request.VersionId}");

			m_engine.DeleteTechnology(request.VersionId, technologies.TechnologyId);

			return Get(request.VersionId);
		}
	}
}
