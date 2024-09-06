using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTest
{
    internal static class JsonClass
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class PageData
        {
            public string PageName { get; set; }
            public List<int> AssignInput { get; set; }
            public List<int> AssignOutput { get; set; }
            public List<string> ValueOutput { get; set; }
            public List<string> AssignInputAddress { get; set; }
            public List<string> AssignOutputAddress { get; set; }
            public List<string> ValueOutputAddress { get; set; }
            public List<SerializedConnection> PageConnections { get; set; }
            public int PageNumber { get; set; }
        }

        public class Root
        {
            public string DataId { get; set; }
            public List<string> Page { get; set; }
            public List<string> Inputs { get; set; }
            public List<string> OutPut { get; set; }
            public int CountInput { get; set; }
            public int CountOutPut { get; set; }
            public List<PageData> PageData { get; set; }
        }


    }
}
