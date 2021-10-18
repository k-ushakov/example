using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Utilities.Tasks;

namespace Mr.Avalon.MariPrice.Core
{
	public class BarcodeStorage : IBarcodeStorage
	{
		private CloudTable m_table;

		public BarcodeStorage(CloudTable barcodesTable)
		{
			m_table = barcodesTable;
		}

		public List<BarcodeTableEntity> ReadOneDownloadSession(Guid sessionId)
		{
			var filter = TableQuery.GenerateFilterCondition(nameof(BarcodeTableEntity.PartitionKey), QueryComparisons.Equal, sessionId.ToString());
			var query = new TableQuery<BarcodeTableEntity>().Where(filter);

			return ExecuteQuery(query).ToList();
		}

		private IEnumerable<BarcodeTableEntity> ExecuteQuery(TableQuery<BarcodeTableEntity> query)
		{
			TableContinuationToken token = null;
			do
			{
				var items = m_table.ExecuteQuerySegmentedAsync(query, token).WaitResult();
				foreach (var item in items.Results)
				{
					yield return item;
				}

				token = items.ContinuationToken;

			} while (token != null);
		}
	}
}
