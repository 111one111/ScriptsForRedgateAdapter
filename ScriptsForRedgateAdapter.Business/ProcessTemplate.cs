﻿using Microsoft.Extensions.Options;
using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Enums;
using ScriptsForRedgateAdapter.Models.Templates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScriptsForRedgateAdapter.Business
{
    public class ProcessTemplate : IProcessTemplate
    {
        private readonly IFileAccess<List<SqlTemplate>> _fileAccess;
        private readonly IReplaceLogic _replaceLogic;

        AppConfig _settings;

        public ProcessTemplate(IReplaceLogic replaceLogic, IFileAccess<List<SqlTemplate>> fileAccess, IOptions<AppConfig> settings)
        {
            _replaceLogic = replaceLogic;
            _fileAccess = fileAccess;
            _settings = settings.Value;
        }

        /// <summary>
        /// Gets a list of templates
        /// </summary>
        /// <returns></returns>
        public List<SqlTemplate> GetTemplates()
        {
            var templates = _fileAccess.LoadJsonFile(_settings.SqlTemplatesFile);
            templates.ForEach(template => {
                template.ExistingCodeTemplate = ProcessTemplateArray(template.ExistingCodeTemplateArray);
                template.SqlCodeTemplate = ProcessTemplateArray(template.SqlCodeTemplateArray);
            });
            return templates;
        }

        /// <summary>
        /// used to convert the string array of an sql template into a string
        /// </summary>
        /// <param name="sqlArray"></param>
        /// <returns></returns>
        public string ProcessTemplateArray(List<string> sqlArray)
        {
            string sql = "";
            sqlArray.ForEach(sqlLine =>
            {
                if (sqlLine == sqlArray[0])
                {
                    sql = sqlLine;
                }
                else
                {
                    sql = $"{sql}\r\n{sqlLine}";
                }
            });

            return sql;
        }

        /// <summary>
        /// Returns the Template related to the Rule By Filename
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public SqlTemplate GetTemplateForSqlScript(Rule rule)
        {
            if(rule == null || rule.TemplateName == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Rule and Template not Matching. No Template Name Specified.");               
                Console.ResetColor();
                return null;
            }

            return GetTemplates()
                .Where(temp => temp.Name == rule.TemplateName)
                .FirstOrDefault();
        }



        /// <summary>
        /// Creates the rollback filename using the ticket number and highest digit.
        /// </summary>
        /// <param name="originalFileName"></param>
        /// <returns></returns>
        public string GenerateRollBackFileName(string originalFileName)
        {
            string LatestFileNumber = (GetFileRollbackCount() + 1).ToString();
            LatestFileNumber.Count();
            for(int dig = 0; dig < 4; dig++)
            {
                if (LatestFileNumber.Count() < _settings.RoleBackPrefixCharCount)
                {
                    LatestFileNumber = $"0{LatestFileNumber}";
                }
            }

            return $"{LatestFileNumber}_{_settings.TicketNumber}_{originalFileName}";
        }

        /// <summary>
        /// Gets the highest number thats set before the FileName.
        /// </summary>
        /// <returns></returns>
        public int GetFileRollbackCount()
        {
            List<string> roleBackFiles = _fileAccess
                .GetFileNames(_settings.RoleBackScriptLocation, FileExtensions.sql);

            if (roleBackFiles.Count == 0)
            {
                return 1;
            }

            List<int> fileNumbers = new List<int>();
            roleBackFiles.ForEach(name => {
               string fileName = Path.GetFileName(name);
               string firstNumberOnly = Regex.Match(fileName, "^[0-9]*").Value;
                if(firstNumberOnly.Count() != 0)
                {
                    fileNumbers.Add(int.Parse(firstNumberOnly));
                }
            });

            if(fileNumbers.Count == 0)
            {
                return 1;
            }

            return fileNumbers
                .OrderBy(number => number)
                .LastOrDefault();
        }

        /// <summary>
        /// Gets the current file in the folder and uses it to create a rollback script.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sqlTemplate"></param>
        public void ApplyExistingCodeTemplate(string filename, SqlTemplate sqlTemplate)
        {
            string fileContents = _fileAccess.GetFileContents($"{sqlTemplate.OutputDirectory}{filename}");
            string rollBackFileName = GenerateRollBackFileName(filename);
            string sql = $"{sqlTemplate.ExistingCodeTemplate}{fileContents}";
            _fileAccess.WriteToFile($"{_settings.RoleBackScriptLocation}{rollBackFileName}", sql);
        }
        

        /// <summary>
        /// Applies the rollback template to create a rollback file. Used if there is no existing file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="newSql"></param>
        /// <param name="sqlTemplate"></param>
        public void ApplyRollBackTemplate(string filename,string newSql, SqlTemplate sqlTemplate)
        {
            string rollBackFileName = GenerateRollBackFileName(filename);           

            sqlTemplate = _replaceLogic.ReplaceDeclareValues(newSql, sqlTemplate);
            sqlTemplate = _replaceLogic.ReplaceSchemaNamesInTemplate(newSql, sqlTemplate);

            _fileAccess.WriteToFile($"{_settings.RoleBackScriptLocation}{rollBackFileName}", sqlTemplate.SqlCodeTemplate);
        }
    }
}
