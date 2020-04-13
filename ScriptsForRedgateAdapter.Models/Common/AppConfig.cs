using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Models.Common
{
    public class AppConfig
    {
        public List<DataBaseDetails> DataBaseList { get; set; }
        public string RoleBackScriptLocation { get; set; }
        public int RoleBackPrefixCharCount { get; set; }
        public string CRUDScriptLocation { get; set; }
        public string ScriptDirectory { get; set; }
        public string SqlTemplatesFile { get; set; }
        public string ScriptHistoryFile { get; set; }
        public string RulesJsonFile { get; set; }
        public string TicketNumber { get; set; }
    }
}
