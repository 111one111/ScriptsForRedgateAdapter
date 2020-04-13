using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Templates;


namespace ScriptsForRedgateAdapter.Interfaces.Business
{
    public interface IProcessTemplate
    {
        /// <summary>
        /// Returns the Template related to the Rule By Filename
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        SqlTemplate GetTemplateForSqlScript(Rule rule);

        /// <summary>
        /// Applies the rollback template to create a rollback file. Used if there is no existing file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="newSql"></param>
        /// <param name="sqlTemplate"></param>
        void ApplyRollBackTemplate(string filename, string newSql, SqlTemplate sqlTemplate);

        /// <summary>
        /// Gets the current file in the folder and uses it to create a rollback script.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sqlTemplate"></param>
        void ApplyExistingCodeTemplate(string filename, SqlTemplate sqlTemplate);
    }
}
