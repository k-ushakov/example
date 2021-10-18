using System;
using System.Collections.Generic;

namespace Mr.Avalon.MariPrice.Core
{
	public interface IBarcodeStorage
	{
		List<BarcodeTableEntity> ReadOneDownloadSession(Guid sessionId);
	}
}