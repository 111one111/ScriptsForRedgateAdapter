using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using ScriptsForRedgateAdapter.Business;
using ScriptsForRedgateAdapter.Interfaces.DAL;
using ScriptsForRedgateAdapter.Models.Common;
using ScriptsForRedgateAdapter.Models.Enums;
using ScriptsForRedgateAdapter.Models.Templates;
using System.Collections.Generic;

namespace ScriptsForRedgateAdapter.Test
{
    [TestFixture]
    public class ProcessTemplatesTests
    {
        private Mock<IFileAccess<List<SqlTemplate>>> _fileAccess;
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
            _processTemplate = new ProcessTemplate(_fileAccess.Object, _settings);
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
    }
}
