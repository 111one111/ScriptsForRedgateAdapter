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
        public string SqlCodeTemplate
        {
            get
            {
                if (SqlCodeTemplate == string.Empty)
                {
                    SqlCodeTemplate = ProcessTemplateArray(SqlCodeTemplateArray);
                }
                return SqlCodeTemplate;
            }

            set { SqlCodeTemplate = value; }
        }

        /// <summary>
        /// Array that is populated from Templates JSON. needs to be formatted into a single string.
        /// </summary>
        public List<string> SqlCodeTemplateArray { get; set; }

        /// <summary>
        /// A list of characters that will need to be replaced to create file successfully.
        /// </summary>
        public List<string> ReplaceMentChars { get; set; }

        /// <summary>
        /// Directory to output file to.
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Array that is populated from Templates JSON. needs to be formatted into a single string.
        /// </summary>
        public List<string> ExistingCodeTemplateArray { get; set; }

        /// <summary>
        /// Stores the existing SQL Code template.
        /// </summary>
        public string ExistingCodeTemplate
        {
            get
            {
                if (ExistingCodeTemplate == string.Empty)
                {
                    ExistingCodeTemplate = ProcessTemplateArray(ExistingCodeTemplateArray);
                }
                return ExistingCodeTemplate;
            }

            set { ExistingCodeTemplate = value; } 
        }
        



        private string ProcessTemplateArray(List<string> sqlArray)
        {
            string sql = "";
            ExistingCodeTemplateArray.ForEach(sqlLine =>
            {
                if (sqlLine == ExistingCodeTemplateArray[0])
                {
                    sql = sqlLine;
                }
                else
                {
                    sql = $"{sql}\r\n{sqlLine}";
                }
            });

            return sql;
        }
    }
}
