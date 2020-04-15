
using Microsoft.Extensions.Options;
using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptsForRedgateAdapter.Business
{
    public class ProcessRules : IProcessRules
    {
        private readonly IFileAccess<List<Rule>> _fileAccess;
        private readonly IScriptCheck _scriptCheck;
        private readonly IProcessTemplate _processTemplate;
        private readonly AppConfig _settings;
        private List<Rule> _sQLPageRules;
        private List<Rule> _generalRules;

        private const string _createTable = "CREATE OR ALTER PROCEDURE";
        private const string _creatSproc = "CREATE TABLE";
        public ProcessRules(IFileAccess<List<Rule>> fileAccess, IOptions<AppConfig> settings, IScriptCheck scriptCheck, IProcessTemplate processTemplate)
        {
            _fileAccess = fileAccess;
            _settings = settings.Value;
            _scriptCheck = scriptCheck;
            _processTemplate = processTemplate;
        }

       
        /// <summary>
        /// Run all templates with matching rules.
        /// </summary>        
        public void Run()
        {
            GetRulesByType();
            List<string> scriptNames = _scriptCheck.GetScriptsNotYetRun();

            foreach(string scriptName in scriptNames)
            {
                string sqlFileContent = _fileAccess.GetFileContents($"{_settings.ScriptDirectory}{scriptName}");

                ApplySqlPageRules(scriptName, sqlFileContent);
            }            
        }

        /// <summary>
        /// Apply the rules without a template name, used to cleanup code and apply to all scripts.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="sqlFileContent"></param>
        public void ApplyGeneralRules(string scriptName, string sqlFileContent)
        {
            _generalRules.ForEach(rule => {
                sqlFileContent = ApplyReplacementRules(rule.Replace, sqlFileContent);
            });
        }

        /// <summary>
        /// Starts applying SQL Rules to page
        /// </summary>
        public void ApplySqlPageRules(string scriptName, string sqlFileContent)
        {

            List<Rule> relatedRules = _scriptCheck.FindRulesRelatedToScript(sqlFileContent, _sQLPageRules);
            relatedRules.ForEach(rule =>            {
                sqlFileContent = ApplyReplacementRules(rule.Replace, sqlFileContent);
            });

            Rule templateRule = relatedRules.Where(rule => rule.TemplateName != null).FirstOrDefault();
            SqlTemplate sqlTemplate = _processTemplate.GetTemplateForSqlScript(templateRule);

            if (relatedRules.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No Matching rules for this script type for {scriptName}");
                Console.ResetColor();
                return;
            }

            ProcessRulesOutput(sqlTemplate, scriptName, sqlFileContent);
        }

        /// <summary>
        /// Used for the final save
        /// </summary>
        /// <param name="sqlTemplate"></param>
        /// <param name="scriptName"></param>
        /// <param name="sqlFileContent"></param>
        private void ProcessRulesOutput(SqlTemplate sqlTemplate, string scriptName, string sqlFileContent) {

            if (_scriptCheck.CheckIfScriptExists(sqlTemplate, scriptName))
            {
                _processTemplate.ApplyExistingCodeTemplate(scriptName, sqlTemplate);
                _fileAccess.WriteToFile($"{sqlTemplate.OutputDirectory}{scriptName}", sqlFileContent);
                _fileAccess.AddLineToFile(_settings.ScriptHistoryFile, scriptName);
            }
            else
            {
                _processTemplate.ApplyRollBackTemplate(scriptName, sqlFileContent, sqlTemplate);
                _fileAccess.WriteToFile($"{sqlTemplate.OutputDirectory}{scriptName}", sqlFileContent);                
                _fileAccess.AddLineToFile(_settings.ScriptHistoryFile, scriptName);
            }
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
                } else if (definition.Length == 2)
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
        /// Gets all Rules by type.
        /// </summary>
        /// <returns></returns>
        public void GetRulesByType()
        {
            var allRules = _fileAccess.LoadJsonFile(_settings.RulesJsonFile);
            _generalRules = allRules.Where(rule => rule.TemplateName == string.Empty).ToList();
            _sQLPageRules = allRules.Where(rule => rule.TemplateName != string.Empty).ToList();
        }       
    }
}
