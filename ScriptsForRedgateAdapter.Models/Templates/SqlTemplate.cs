using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Models.Templates
{
    public class SqlTemplate
    {
        /// <summary>
        /// Name will be matched to rules to determine if the rule engine needs to run.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SQL Template for the script that will be written into the appropriate directory.
        /// </summary>
        public string SqlCodeTemplate { get; set; }

        /// <summary>
        /// A list of characters that will need to be replaced to create file successfully.
        /// </summary>
        public List<string> ReplaceMentChars { get; set; }

        /// <summary>
        /// Directory to output file to.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Stores the existing SQL Code template.
        /// </summary>
        public string ExistingCodeTemplate { get; set; }
    }
}
