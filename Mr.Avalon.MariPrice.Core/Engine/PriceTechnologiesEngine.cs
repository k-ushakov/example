using d7k.Dto;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public class PriceTechnologiesEngine
	{
		private ISqlFactory m_sql;
		private DtoComplex m_dto;
		private ISpecEngine m_specEngine;
		private SpecSettings m_specSettings;

		public PriceTechnologiesEngine(ISqlFactory sql, DtoComplex dto, ISpecEngine specEngine, SpecSettings specSettings)
		{
			m_sql = sql;
			m_dto = dto;
			m_specEngine = specEngine;
			m_specSettings = specSettings;
		}

		public List<MariPriceApi.Price.Technologies> Get(MariPriceApi.Price.Technologies.List request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			if (!request.VersionIds.Any())
				return new List<MariPriceApi.Price.Technologies>();

			var dbRequest = new MariPriceDb.Price.CompanyTechnology.List()
				.ForVersionId(request.VersionIds.ToArray());

			var result = dbRequest.Exec(m_sql);

			var accum = new List<MariPriceApi.Price.Technologies>();
			foreach (var item in result)
			{
				accum.Add(new MariPriceApi.Price.Technologies()
				{
					VersionId = item.VersionId,
					TechnologyId = item.TechnologyId,
					WithNdsPrice = item.WithNdsPrice,
					WithoutNdsPrice = item.WithoutNdsPrice
				});
			}

			return accum;
		}

		public void Create(MariPriceApi.Price.Technologies.Create request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			//TO DO
			//ValidateTechnologies(request.TechnologyAdditions);

			var dbRequest = new MariPriceDb.Price.CompanyTechnology.Create()
			{
				VersionId = request.VersionId,
				TechnologyId = request.TechnologyId,
				WithNdsPrice = request.WithNdsPrice,
				WithoutNdsPrice = request.WithoutNdsPrice
			};

			dbRequest.Exec(m_sql);
		}

		public void Set(MariPriceApi.Price.Technologies.Set request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			// TO DO
			//ValidateTechnologies(request.TechnologyAdditions);

			var dbRequest = new MariPriceDb.Price.CompanyTechnology.Update()
			{
				VersionId = request.VersionId,
				TechnologyId = request.TechnologyId,
				WithNdsPrice = request.WithNdsPrice,
				WithoutNdsPrice = request.WithoutNdsPrice
			};

			dbRequest.Exec(m_sql);
		}

		public void Delete(int versionId, Guid technologyId)
		{
			using (var transSql = m_sql.Transaction())
			{
				var dbTechnology = new MariPriceDb.Price.CompanyTechnology.Delete() { VersionId = versionId, TechnologyId = technologyId };
				dbTechnology.Exec(transSql);

				transSql.Commit();
			}
		}

		private void ValidateTechnologies(Dictionary<Guid, MariPriceApi.Price.Technologies.TechnologyApi> technologyAdditions)
		{
			var allTechnologies = m_specEngine.GetVocValues(m_specSettings.Technologies, technologyAdditions.Keys.ToArray())
				.Items.Select(x => x.Id).ToHashSet();

			foreach (var techId in technologyAdditions.Keys)
			{
				if (!allTechnologies.Contains(techId))
					throw new ConflictApiException($"Techology with id '{techId}' does not exists");
			}
		}
	}
}
