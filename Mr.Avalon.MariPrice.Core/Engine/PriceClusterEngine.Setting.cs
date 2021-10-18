using d7k.Dto;
using Mr.Avalon.Common;
using Mr.Avalon.MariPrice.Client;
using Mr.Avalon.Spec.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class PriceClusterEngine
	{
		public int CreateSetting(MariPriceApi.Price.ClusterSetting.Create request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			// TO DO VALIDATION

			var dbRequest = new MariPriceDb.Price.ClusterSetting.Create().CopyFrom(request, m_dto);

			var id = dbRequest.Exec(m_sql);

			return id;
		}

		public void UpdateSetting(MariPriceApi.Price.ClusterSetting.Update request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.ClusterSetting.Update()
			.CopyFrom(request, m_dto)
			.DefaultUpdationList();

			dbRequest.Exec(m_sql);
		}

		public List<MariPriceApi.Price.ClusterSetting> GetSetting(MariPriceApi.Price.ClusterSetting.List request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.ClusterSetting.List()
				.ForIds(request.Ids?.ToArray())
				.ForClusters(request.PriceClusterIds?.ToArray());

			var dbResult = dbRequest.Exec(m_sql);

			if (!dbResult.Any())
				return new List<MariPriceApi.Price.ClusterSetting>();

			return dbResult.Select(x => new MariPriceApi.Price.ClusterSetting().CopyFrom(x, m_dto))?.ToList();
		}

		public void DeleteSetting(MariPriceApi.Price.ClusterSetting.Delete request)
		{
			request = m_dto.ValidationRepository.FixValue(request, nameof(request), x => x.NotEmpty().ValidateDto());

			var dbRequest = new MariPriceDb.Price.ClusterSetting.Delete()
			{
				PriceClusterSettingIds = new List<int>() { request.SettingsVariantId },
				ClusterId = request.ClusterId
			};

			dbRequest.Exec(m_sql);
		}
	}
}
