using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Interfaces.Business
{
    public interface IScriptCheck
    {
        /// <summary>
        /// Gets the files from the script directory and compares it to the configuration file.
        /// </summary>
        /// <returns></returns>
        List<string> GetScriptsNotYetRun();

        /// <summary>
        /// Looks for related rules based off whats in script and the Rules Script Identifier
        /// </summary>
        /// <param name="sqlFileContent"></param>
        /// <returns></returns>
        List<Rule> FindRulesRelatedToScript(string sqlFileContent, List<Rule> sQLPageRules);

        /// <summary>
        /// Checks to see if the script is an existing script
        /// </summary>
        /// <param name="sqlTemplate"></param>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        bool CheckIfScriptExists(SqlTemplate sqlTemplate, string fullFileName);
    }
}
