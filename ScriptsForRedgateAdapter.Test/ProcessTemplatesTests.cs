using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using ScriptsForRedgateAdapter.Business;
using ScriptsForRedgateAdapter.Interfaces.Business;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Enums;
using ScriptsForRedgateAdapter.Models.Templates;
using System.Collections.Generic;
using System.Linq;

namespace ScriptsForRedgateAdapter.Test
{
    [TestFixture]
    public class ProcessTemplatesTests
    {
        private Mock<IFileAccess<List<SqlTemplate>>> _fileAccess;
        private Mock<IReplaceLogic> _replaceLogic;
        private ProcessTemplate _processTemplate;        
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
                TicketNumber = "345",
                RoleBackPrefixCharCount = 3
            });

        [SetUp]
        public void Setup()
        {
            _fileAccess = new Mock<IFileAccess<List<SqlTemplate>>>();
            _replaceLogic = new Mock<IReplaceLogic>();
            _processTemplate = new ProcessTemplate(_replaceLogic.Object, _fileAccess.Object, _settings);
        }

        [Test]
        public void GenerateRollBackFileName_Should_Return_Valid_FileName()
        {
            // Arrange
            _fileAccess.Setup(method => method.GetFileNames(_settings.Value.RoleBackScriptLocation, FileExtensions.sql))
                .Returns(new List<string>() { "c:\\script\\1234mockscript1.sql", "c:\\script\\12mockscript2.sql", "c:\\script\\012_mockscript3.sql", "c:\\script\\mockscript4.sql" });
            string fileName = "test.sql";
            // Act
            var result = _processTemplate.GenerateRollBackFileName(fileName);

            //Assert
            result.Should().Be($"{1235}_{_settings.Value.TicketNumber}_{fileName}");
        }

        [Test]
        public void GenerateRollBackFileName_Should_Return_FileName_With_3_Numbers_At_Start()
        {
            // Arrange
            _fileAccess.Setup(method => method.GetFileNames(_settings.Value.RoleBackScriptLocation, FileExtensions.sql))
                .Returns(new List<string>() { "14mockscript1.sql", "12mockscript2.sql", "13_mockscript3.sql" });
            string fileName = "test.sql";
            // Act
            var result = _processTemplate.GenerateRollBackFileName(fileName);

            //Assert
            result.Should().Be($"{"015"}_{_settings.Value.TicketNumber}_{fileName}");
        }

        [Test]
        public void GetFileRollbackCount_Should_Return_Valid_Int()
        {
            // Arrange
            _fileAccess.Setup(method => method.GetFileNames(_settings.Value.RoleBackScriptLocation, FileExtensions.sql))
                .Returns(new List<string>() { "1234mockscript1.sql", "12mockscript2.sql", "012_mockscript3.sql", "mockscript4.sql" });

            // Act
            var result = _processTemplate.GetFileRollbackCount();

            // Assert
            result.Should().Be(1234);
        }

        [Test]
        public void GetTemplates_Should_Return_A_List_Of_Templates_With_Array_Sql_Populating_String()
        {
            // Arrange
            var codeTemplateArrayValues = new List<string>() { "this is a test", "to see if the lines", "join with return carriage", "###ReplaceMe###" };
            var replacmentChars = new List<string> { "###ReplaceMe###" };
            string name = "test template";
            string outputDir = "c:\\test\\";

            List<SqlTemplate> testTemplate = new List<SqlTemplate>() {
                new SqlTemplate(){
                    ExistingCodeTemplateArray = codeTemplateArrayValues,
                    SqlCodeTemplateArray = codeTemplateArrayValues,
                    Name = name,
                    OutputDirectory = outputDir,
                    ReplaceMentChars = replacmentChars                    
                }
            };
            _fileAccess.Setup(method => method.LoadJsonFile(_settings.Value.SqlTemplatesFile))
                .Returns(testTemplate);

            // Act
            var result = _processTemplate.GetTemplates().First();

            //Assert
            string answer = "this is a test\r\nto see if the lines\r\njoin with return carriage\r\n###ReplaceMe###";
            result.SqlCodeTemplate.Should().Be(answer);
            result.ExistingCodeTemplate.Should().Be(answer);
            result.Name.Should().Be(name);
            result.OutputDirectory.Should().Be(outputDir);
            result.ReplaceMentChars.First().Should().Be(replacmentChars.First());
        }

        [Test]
        public void ProcessTemplateArray_Should_Return_String()
        {
            // Arrange
            var codeTemplateArrayValues = new List<string>() { "this is a test", "to see if the lines", "join with return carriage", "###ReplaceMe###" };

            // Act
            var result = _processTemplate.ProcessTemplateArray(codeTemplateArrayValues);

            // Assert
            result.Should().Be("this is a test\r\nto see if the lines\r\njoin with return carriage\r\n###ReplaceMe###");
        }

        [Test]
        public void GetTemplateForSqlScript_Should_Return_Matching_Template()
        {
            // Arrange
            var codeTemplateArrayValues = new List<string>() { "this is a test", "to see if the lines", "join with return carriage", "###ReplaceMe###" };
            var replacmentChars = new List<string> { "###ReplaceMe###" };
            string name = "test template";
            string outputDir = "c:\\test\\";

            List<SqlTemplate> testTemplate = new List<SqlTemplate>() {
                new SqlTemplate(){
                    ExistingCodeTemplateArray = codeTemplateArrayValues,
                    SqlCodeTemplateArray = codeTemplateArrayValues,
                    Name = name,
                    OutputDirectory = outputDir,
                    ReplaceMentChars = replacmentChars
                },
                new SqlTemplate()
                {
                    Name = "DummyData",
                    OutputDirectory = "c:\\dummy",
                    ReplaceMentChars = new List<string>(){ "##Dummy##" },
                    ExistingCodeTemplateArray = new List<string>(){ "Dummy Data" },
                    SqlCodeTemplateArray =new List<string>(){ "Dummy Data" },
                },
                new SqlTemplate()
                {                    
                    OutputDirectory = "c:\\dummy2",
                    ReplaceMentChars = new List<string>(){ "##Dummy2##" },
                    ExistingCodeTemplateArray = new List<string>(){ "Dummy Data 2" },
                    SqlCodeTemplateArray =new List<string>(){ "Dummy Data 2" },
                }
            };

            _fileAccess.Setup(method => method.LoadJsonFile(_settings.Value.SqlTemplatesFile))
               .Returns(testTemplate);

            var rule = new Rule()
            {
                TemplateName = name,
                GetScriptNameFromFile = false,
                ScriptIdentifier = new List<string>() { "this is a test" },
                Replace = new List<string>() { "this is a test:This is another Test" }
            };

            // Act
            var result = _processTemplate.GetTemplateForSqlScript(rule);

            //Assert
            string answer = "this is a test\r\nto see if the lines\r\njoin with return carriage\r\n###ReplaceMe###";
            result.SqlCodeTemplate.Should().Be(answer);
            result.ExistingCodeTemplate.Should().Be(answer);
            result.Name.Should().Be(name);
            result.OutputDirectory.Should().Be(outputDir);
            result.ReplaceMentChars.First().Should().Be(replacmentChars.First());
        }
    }
}
