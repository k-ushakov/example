using d7k.Dto;
using Mr.Avalon.MariPrice.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class PriceGroupEngine
	{
		public int CreateValue(MariPriceApi.Price.GroupValue.Create request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.GroupValue.Create().CopyFrom(request, m_dto);

			var id = dbRequest.Exec(m_sql);

			return id;
		}

		public void UpdateValue(MariPriceApi.Price.GroupValue.Update request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.GroupValue.Update()
			.CopyFrom(request, m_dto)
			.DefaultUpdationList();

			dbRequest.Exec(m_sql);
		}

		public void DeleteVaue(MariPriceApi.Price.GroupValue.Delete request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.GroupValue.DeleteById() { PriceGroupValueId = request.PriceGroupValueId };

			dbRequest.Exec(m_sql);
		}

		public List<MariPriceApi.Price.GroupValue> GetValues(MariPriceApi.Price.GroupValue.List request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.GroupValue.List()
				.ForIds(request.Ids?.ToArray())
				.ForGroupIds(request.GroupIds?.ToArray());

			var dbResult = dbRequest.Exec(m_sql);

			if (!dbResult.Any())
				return new List<MariPriceApi.Price.GroupValue>();

			return dbResult.Select(x => new MariPriceApi.Price.GroupValue().CopyFrom(x, m_dto))?.ToList();
		}

		public void CreateInstockValue(MariPriceApi.Price.InstockGroupValue.Create request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.InstockGroupValue.Create().CopyFrom(request, m_dto);

			dbRequest.Exec(m_sql);
		}

		public void UpdateInstockValue(MariPriceApi.Price.InstockGroupValue.Update request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.InstockGroupValue.Update()
			.CopyFrom(request, m_dto)
			.DefaultUpdationList();

			dbRequest.Exec(m_sql);
		}

		public void DeleteInstockVaue(MariPriceApi.Price.InstockGroupValue.Delete request)
		{
			var dbRequest = new MariPriceDb.Price.InstockGroupValue.DeleteByGroup() { PriceGroupId = request.PriceGroupId };

			dbRequest.Exec(m_sql);
		}

		public List<MariPriceApi.Price.InstockGroupValue> GetInstockValues(MariPriceApi.Price.InstockGroupValue.List request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.InstockGroupValue.List()
				.ForGroupIds(request.GroupIds?.ToArray());

			var dbResult = dbRequest.Exec(m_sql);

			if (!dbResult.Any())
				return new List<MariPriceApi.Price.InstockGroupValue>();

			return dbResult.Select(x => new MariPriceApi.Price.InstockGroupValue().CopyFrom(x, m_dto))?.ToList();
		}
	}
}
