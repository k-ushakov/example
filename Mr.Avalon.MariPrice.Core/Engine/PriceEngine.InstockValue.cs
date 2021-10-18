using d7k.Dto;
using d7k.Dto.Utilities;
using Mr.Avalon.Common;
using Mr.Avalon.Common.Core.Api;
using Mr.Avalon.Description.Client;
using Mr.Avalon.MariPrice.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Sql;

namespace Mr.Avalon.MariPrice.Core
{
	public partial class PriceEngine
	{
		public List<MariPriceApi.Price.InstockGroupValue> GetGroupsInstockValues(MariPriceApi.Price.InstockGroupValue.List list)
		{
			return m_group.GetInstockValues(list);
		}

		public void CreateInstockGroupValue(MariPriceApi.Price.InstockGroupValue.Create request)
		{
			m_group.CreateInstockValue(request);
		}

		public void UpdateGroupInstockValue(MariPriceApi.Price.InstockGroupValue.Update request)
		{
			m_group.UpdateInstockValue(request);
		}

		public void DeleteGroupInstockValue(MariPriceApi.Price.InstockGroupValue.Delete request)
		{
			m_group.DeleteInstockVaue(request);
		}

	}
}
