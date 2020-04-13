using Microsoft.Extensions.Options;
using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Enums;
using ScriptsForRedgateAdapter.Models.Templates;
using System.Collections.Generic;
using System.IO;

namespace ScriptsForRedgateAdapter.Business
{
    public class ScriptCheck : IScriptCheck
    {
        IFileAccess<string> _fileAccess;
        AppConfig _settings;
        public ScriptCheck(IFileAccess<string> fileAccess, IOptions<AppConfig> settings) {
            _fileAccess = fileAccess;
            _settings = settings.Value;
        }        


        /// <summary>
        /// Gets the files from the script directory and compares it to the configuration file.
        /// </summary>
        /// <returns></returns>
        public List<string> GetScriptsNotYetRun()
        {
            string filesRun = _fileAccess.GetFileContents(_settings.ScriptHistoryFile);
            var filesInScriptDirectory = _fileAccess.GetFileNames(_settings.ScriptDirectory, FileExtensions.sql);

            List<string> fileNames = new List<string>();
            filesInScriptDirectory.ForEach(fileName => {
                if (!filesRun.Contains(Path.GetFileName(fileName)))
                {
                    fileNames.Add(Path.GetFileName(fileName));
                }
            });

            return fileNames;
        }

        /// <summary>
        /// Looks for related rules based off whats in script and the Rules Script Identifier
        /// </summary>
        /// <param name="sqlFileContent"></param>
        /// <returns></returns>
        public List<Rule> FindRulesRelatedToScript(string sqlFileContent, List<Rule> sQLPageRules)
        {
            List<Rule> relatedRules = new List<Rule>();
            List<string> indentifiers = new List<string>();
            foreach (Rule rule in sQLPageRules)
            {
                bool relatedTest = false;
                rule.ScriptIdentifier.ForEach(ident => {
                    if (sqlFileContent.Contains(ident))
                    {
                        relatedTest = true;
                    }
                });

                if (rule.ShouldNotContain != null)
                {
                    rule.ShouldNotContain.ForEach(baned =>
                    {
                        if (sqlFileContent.Contains(baned))
                        {
                            relatedTest = false;
                        }
                    });
                }
                if (relatedTest)
                {
                    relatedRules.Add(rule);
                }
            }

            return relatedRules;
        }

        /// <summary>
        /// Checks to see if the script is an existing script
        /// </summary>
        /// <param name="sqlTemplate"></param>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        public bool CheckIfScriptExists(SqlTemplate sqlTemplate, string fullFileName)
        {
            string fileName = Path.GetFileName(fullFileName);
            if (File.Exists($"{sqlTemplate.OutputDirectory}\\{fileName}"))
            {
                return true;
            }

            return false;
        }
    }
}
