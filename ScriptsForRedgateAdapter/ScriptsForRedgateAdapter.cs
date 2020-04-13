using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Interfaces.Console;

namespace ScriptsForRedgateAdapter
{
    public class ScriptsForRedgateAdapter : IScriptsForRedgateAdapter
    {
        IProcessRules _processRules;
        public ScriptsForRedgateAdapter(IProcessRules processRules)
        {
            _processRules = processRules;
        }

        /// <summary>
        /// Starts Application
        /// </summary>
        public void Start()
        {
           _processRules.Run();
        }
    }
}
