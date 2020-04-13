using Microsoft.Extensions.Options;
using ScriptsForRedgateAdapter.Business;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Enums;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Moq;

namespace ScriptsForRedgateAdapter.Test
{
    [TestFixture]
    public class ScriptCheckTests
    {
        private Mock<IFileAccess<string>> _fileAccess;      
        private ScriptCheck _scriptCheck;
        private List<Rule> _rules;
        private IOptions<AppConfig> _settings = Options.Create(
            new AppConfig()
            {
                CRUDScriptLocation = "test",
                DataBaseList = new List<DataBaseDetails>() {
                new DataBaseDetails() { FolderLocation="test", Name="test1"}
            },
                RoleBackScriptLocation = "test",
                RulesJsonFile = "test",
                ScriptDirectory = "test",
                ScriptHistoryFile = "test",
                SqlTemplatesFile = "test",
                TicketNumber = "345"
            });


        [SetUp]
        public void Setup()
        {
            _fileAccess = new Mock<IFileAccess<string>>();
            _scriptCheck = new ScriptCheck(_fileAccess.Object, _settings);
            _rules = new List<Rule>() {
                new Rule(){ 
                    ScriptIdentifier = new List<string>(){"tblPage" },
                    ShouldNotContain = new List<string>()
                },
                new Rule(){ 
                    ScriptIdentifier = new List<string>(){"tblTest","tblPage" },
                    ShouldNotContain = new List<string>()},
                new Rule(){
                    ScriptIdentifier = new List<string>(){"tblCustomer"},
                    ShouldNotContain = null},
                new Rule(){
                    ScriptIdentifier = new List<string>(){"tblPage"},
                    ShouldNotContain = new List<string>(){ "PROCEDURE" }
                }
            };            
        }

        [Test]
        public void GetScriptsNotYetRun_Should_Return_2_FileNames()
        {
            // Arrange
            _fileAccess.Setup(method => method.GetFileContents(_settings.Value.ScriptHistoryFile))
               .Returns("mockscript1.sql\r\nmockscript2.sql");
            _fileAccess.Setup(method => method.GetFileNames(_settings.Value.ScriptDirectory, FileExtensions.sql))
                .Returns(new List<string>() { "mockscript1.sql", "mockscript2.sql", "mockscript3.sql", "mockscript4.sql" });

            // Act
            var result = _scriptCheck.GetScriptsNotYetRun();
            var content = result as List<string>;
            // Assert           

            content.Should().Contain("mockscript3.sql");
            content.Should().Contain("mockscript4.sql");
            content.Should().NotContain("mockscript1.sql");
            content.Should().NotContain("mockscript2.sql");
        }

        [Test]
        public void FindRulesRelatedToScript_Should_Return_3_Rules()
        {
            // Arrange
            string sql = "tblPage";

            // Act
            var result = _scriptCheck.FindRulesRelatedToScript(sql, _rules);
            var content = result as List<Rule>;

            // Assert
            content.Count.Should().Be(3);
            content[0].ScriptIdentifier.Should().Contain("tblPage");
        }

        [Test]
        public void FindRulesRelatedToScript_Should_Return_2_Rules()
        {
            // Arrange
            List<Rule> rules = new List<Rule>() {
                new Rule(){ ScriptIdentifier = new List<string>(){"tblPage" } },
                new Rule(){ ScriptIdentifier = new List<string>(){"tblTest","tblPage" } },
                new Rule(){ ScriptIdentifier = new List<string>(){"tblCustomer"} },
                new Rule(){
                    ScriptIdentifier = new List<string>(){"tblPage"},
                    ShouldNotContain = new List<string>(){ "PROCEDURE" }
                }
            };

            string sql = "tblPage procedere";

            // Act
            var result = _scriptCheck.FindRulesRelatedToScript(sql, rules);
            var content = result as List<Rule>;

            // Assert
            content.Count.Should().Be(3);
            content[0].ScriptIdentifier.Should().Contain("tblPage");
        }
    }
}
