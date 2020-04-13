
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

                ApplySqlPageRules(scriptName);
            }            
        }

        /// <summary>
        /// Starts applying SQL Rules to page
        /// </summary>
        public void ApplySqlPageRules(string scriptName)
        {
           string sqlFileContent = _fileAccess.GetFileContents($"{_settings.ScriptDirectory}{scriptName}");
           List<Rule> relatedRules = _scriptCheck.FindRulesRelatedToScript(sqlFileContent, _sQLPageRules);
            relatedRules.ForEach(rule =>
            {
                sqlFileContent = ApplyReplacementRules(rule.Replace, sqlFileContent);
            });

            SqlTemplate sqlTemplate = _processTemplate.GetTemplateForSqlScript(relatedRules.Where(rule => rule.TemplateName != null).FirstOrDefault());

            if (relatedRules.Count ==0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No Matching rules for this script type for {scriptName}");
                Console.ResetColor();
                return;
            }

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
