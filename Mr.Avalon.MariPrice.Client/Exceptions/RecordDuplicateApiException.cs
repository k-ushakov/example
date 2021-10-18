using Mr.Avalon.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mr.Avalon.MariPrice.Client
{
	public class RecordDuplicateApiException : ConflictApiException
	{
		public string[] Fields { get; set; }

		public RecordDuplicateApiException() { }
		public RecordDuplicateApiException(string message, params string[] fields) : base(message)
		{
			Fields = fields;
		}

		public override void BuildExtensionFields(JObject accum)
		{
			Fields = accum["fields"].ToObject<string[]>();
		}

		public override JObject InitExtensionFields()
		{
			var res = new JObject();
			res["fields"] = JArray.FromObject(Fields ?? new string[0]);
			return res;
		}
	}
}
