using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Templates;
using System.Collections.Generic;

namespace ScriptsForRedgateAdapter.Interfaces.Business
{
    public interface IProcessRules
    {
        /// <summary>
        /// Run all templates with matching rules.
        /// </summary>
        /// <param name="sqlTemplates"></param>
        void Run();
    }
}
