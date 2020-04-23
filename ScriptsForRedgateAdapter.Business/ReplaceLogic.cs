using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptsForRedgateAdapter.Business
{
    public class ReplaceLogic : IReplaceLogic
    {
        private const string _createTable = "CREATE OR ALTER PROCEDURE ";
        private const string _creatSproc = "CREATE TABLE ";

        /// <summary>
        /// Applies replacement rules to SQL code.
        /// </summary>
        /// <param name="replacmentRule"></param>
        /// <param name="sqlCode"></param>
        /// <returns></returns>
        public string ApplyReplacementRules(List<string> replacmentRule, string sqlCode)
        {
            replacmentRule.ForEach(rule => {
                string[] definition = rule.Split(":");
                if (definition.Length == 1)
                {
                    sqlCode = sqlCode.Replace(definition[0], "");
                }
                else if (definition.Length == 2)
                {
                    sqlCode = sqlCode.Replace(definition[0], definition[1]);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"There was an issue with replacement rules. {rule}");
                    Console.ResetColor();
                }
            });

            return sqlCode;
        }

        /// <summary>
        /// Uses the name of the Sproc or Procedure to generate the filename.
        /// </summary>
        /// <param name="sqlCode"></param>
        /// <returns></returns>
        public string ApplyNewScriptName(string sqlCode)
        {
            string[] sqlLines = sqlCode.Split("\r\n");
            string lineWithFileName = sqlLines.Where(line => line.Contains(_creatSproc)
                                        || line.Contains(_createTable)).FirstOrDefault();

            if (lineWithFileName.Contains(_creatSproc))
            {
                return NameExtractor(lineWithFileName, _creatSproc);
            }

            if (lineWithFileName.Contains(_createTable))
            {
                return NameExtractor(lineWithFileName, _createTable);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the name out of the line of text that contains the identifier by removing the identifier then trimming.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public string NameExtractor(string line, string identifier)
        {
            string newName = line.Replace(identifier, "").Trim();
            if (newName.Split(" ").Count() > 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Generating name for script not success full. {newName} not Updated");
                Console.ResetColor();
                return string.Empty;
            }
            return newName;
        }

        /// <summary>
        /// Used to locate and  replace the name of the table or sproc that is in the template.
        /// </summary>
        /// <param name="newSql"></param>
        /// <param name="sqlTemplate"></param>
        /// <returns></returns>
        public SqlTemplate ReplaceSchemaNamesInTemplate(string newSql, SqlTemplate sqlTemplate)
        {
            string[] sql = newSql.Split("\r\n");
            sqlTemplate.ReplaceMentChars.ForEach(replace => {
                if (replace.Contains(":"))
                {
                    return;
                }

                string sqlLine = sql.Where(declare => declare.Contains(_createTable) || declare.Contains(_creatSproc)).FirstOrDefault();
                if (string.IsNullOrEmpty(sqlLine))
                {
                    return;
                }
                string value = "";
                if (sqlLine.Split(".").Count() == 2)
                {
                    value = sqlLine.Split(".")[1]
                                                .Replace("[", string.Empty)
                                                .Replace("](", string.Empty)
                                                .Replace("]", string.Empty);
                }
                else
                {
                    value = sqlLine.Replace(_createTable, string.Empty)
                                   .Replace(_creatSproc, string.Empty)
                                   .Replace("[", string.Empty)
                                   .Replace("](", string.Empty)
                                   .Replace("]", string.Empty);
                }
                sqlTemplate.SqlCodeTemplate = sqlTemplate.SqlCodeTemplate.Replace(replace, value);
            });

            return sqlTemplate;
        }

        /// <summary>
        /// Replaces values that are declared. Only works 1 per line.
        /// </summary>
        /// <param name="newSql"></param>
        /// <param name="sqlTemplate"></param>
        /// <returns></returns>
        public SqlTemplate ReplaceDeclareValues(string newSql, SqlTemplate sqlTemplate)
        {
            string[] sql = newSql.Split("\r\n");
            sqlTemplate.ReplaceMentChars.ForEach(replace => {
                if (!replace.Contains(":"))
                {
                    return;
                }
                string[] replacmentKey = replace.Split(":");
                string sqlLine = sql.Where(declare => declare.Contains($"DECLARE {replacmentKey[0]}")).FirstOrDefault();
                if (sqlLine == "")
                {
                    return;
                }
                string value = sqlLine.Split("=")[1];
                sqlTemplate.SqlCodeTemplate = sqlTemplate.SqlCodeTemplate.Replace(replacmentKey[1], value);
            });

            return sqlTemplate;
        }
    }
}
