using ScriptsForRedgateAdapter.Models.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScriptsForRedgateAdapter.Interfaces.Business
{
    public interface IReplaceLogic
    {
        /// <summary>
        /// Applies replacement rules to SQL code.
        /// </summary>
        /// <param name="replacmentRule"></param>
        /// <param name="sqlCode"></param>
        /// <returns></returns>
        string ApplyReplacementRules(List<string> replacmentRule, string sqlCode);

        /// <summary>
        /// Uses the name of the Sproc or Procedure to generate the filename.
        /// </summary>
        /// <param name="sqlCode"></param>
        /// <returns></returns>
        string ApplyNewScriptName(string sqlCode);

        /// <summary>
        /// Used to locate and  replace the name of the table or sproc that is in the template.
        /// </summary>
        /// <param name="newSql"></param>
        /// <param name="sqlTemplate"></param>
        /// <returns></returns>
        SqlTemplate ReplaceSchemaNamesInTemplate(string newSql, SqlTemplate sqlTemplate);

        /// <summary>
        /// Replaces values that are declared. Only works 1 per line.
        /// </summary>
        /// <param name="newSql"></param>
        /// <param name="sqlTemplate"></param>
        /// <returns></returns>
        SqlTemplate ReplaceDeclareValues(string newSql, SqlTemplate sqlTemplate);
    }
}
