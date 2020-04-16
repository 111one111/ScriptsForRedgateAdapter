
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
        private readonly IReplaceLogic _replaceLogic;
        private readonly AppConfig _settings;
        private List<Rule> _sQLPageRules;
        private List<Rule> _generalRules;

        public ProcessRules(IFileAccess<List<Rule>> fileAccess, IOptions<AppConfig> settings, IScriptCheck scriptCheck, IProcessTemplate processTemplate, IReplaceLogic replaceLogic)
        {
            _fileAccess = fileAccess;
            _settings = settings.Value;
            _scriptCheck = scriptCheck;
            _processTemplate = processTemplate;
            _replaceLogic = replaceLogic;
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
                sqlFileContent = ApplyGeneralRules(sqlFileContent);
                

                ApplySqlPageRules(scriptName, sqlFileContent);
            }            
        }

        /// <summary>
        /// Apply the rules without a template name, used to cleanup code and apply to all scripts.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="sqlFileContent"></param>
        public string ApplyGeneralRules(string sqlFileContent)
        {
            _generalRules.ForEach(rule =>
            {
                sqlFileContent = _replaceLogic.ApplyReplacementRules(rule.Replace, sqlFileContent);
            });
            return sqlFileContent;
        }

        /// <summary>
        /// Starts applying SQL Rules to page
        /// </summary>
        public void ApplySqlPageRules(string scriptName, string sqlFileContent)
        {

            List<Rule> relatedRules = _scriptCheck.FindRulesRelatedToScript(sqlFileContent, _sQLPageRules);
            relatedRules.ForEach(rule =>            {
                sqlFileContent = _replaceLogic.ApplyReplacementRules(rule.Replace, sqlFileContent);
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

            if (templateRule.GetScriptNameFromFile)
            {
                scriptName = _replaceLogic.ApplyNewScriptName(sqlFileContent);
            }

            if (string.IsNullOrEmpty(scriptName))
            {
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
