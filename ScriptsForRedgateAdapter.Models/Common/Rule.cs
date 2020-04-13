using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Models.Common
{
    public class Rule
    {
        /// <summary>
        /// Name need to match corresponding template.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// What the rules engine looks for to make a rule run.
        /// </summary>
        public List<string> ScriptIdentifier { get; set; }

        /// <summary>
        /// A list of values a script shouldn't contain
        /// </summary>
        public List<string> ShouldNotContain { get; set; }

        /// <summary>
        /// For rules pertaining to replacing strings.
        /// </summary>
        public List<string> Replace { get; set; }
    }
}
