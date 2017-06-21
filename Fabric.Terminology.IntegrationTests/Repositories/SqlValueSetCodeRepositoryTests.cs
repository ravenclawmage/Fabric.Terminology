﻿using System;
using System.Linq;
using Fabric.Terminology.Domain.Models;
using Fabric.Terminology.Domain.Persistence;
using Fabric.Terminology.SqlServer.Persistence;
using Fabric.Terminology.SqlServer.Persistence.DataContext;
using Fabric.Terminology.TestsBase;
using Fabric.Terminology.TestsBase.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Fabric.Terminology.IntegrationTests.Repositories
{
    public class SqlValueSetCodeRepositoryTests : RuntimeTestsBase
    {
        private readonly IValueSetCodeRepository _valueSetCodeRepository;

        public SqlValueSetCodeRepositoryTests(ITestOutputHelper output, ConfigTestFor testType = ConfigTestFor.Integration) : base(output, testType)
        {
            var factory = new SharedContextFactory(AppConfig.TerminologySqlSettings, Logger);
            var context = factory.Create();
            if (context.IsInMemory) throw new InvalidOperationException();
            _valueSetCodeRepository = new SqlValueSetCodeRepository(factory.Create(), new NullMemoryCacheProvider());
        }

        

        [Theory]
        [Trait(TestTraits.Category, TestCategory.LongRunning)]
        [InlineData("2.16.840.1.113883.3.464.1003.108.12.1011")]
        [InlineData("2.16.840.1.113762.1.4.1045.36")]
        [InlineData("2.16.840.1.113883.3.526.3.1459")]
        [InlineData("2.16.840.1.113883.3.67.1.101.1.278")]
        public void GetByValueSet(string valueSetId)
        {
            //// Arrange

            //// Act
            var codes = ExecuteTimed(() => _valueSetCodeRepository.GetByValueSet(valueSetId), $"Querying ValueSetId = {valueSetId}");
            Output.WriteLine($"Result count: {codes.Count}");

            //// Assert
            Assert.True(codes.Any());
            
        }

        [Theory]
        [Trait(TestTraits.Category, TestCategory.LongRunning)]
        [InlineData("2.16.840.1.113883.6.104", 1, 500)] // ICD9CM - approx 11827 rows
        [InlineData("2.16.840.1.113883.6.104", 2, 500)]
        [InlineData("2.16.840.1.113883.6.104", 3, 500)]
        [InlineData("2.16.840.1.113883.6.90", 1, 500)] // ICD10CM - approx 11
        [InlineData("2.16.840.1.113883.6.90", 3, 500)]
        public void GetByCodeSystem(string codeSystemCode, int currentPage, int itemsPerPage)
        {
            //// Arrange
            var settings = new PagerSettings {CurrentPage = currentPage, ItemsPerPage = itemsPerPage};
 
            //// Act
            var codesPage = ExecuteTimed(async () => await _valueSetCodeRepository.GetByCodeSystemAsync(codeSystemCode, settings), $"Querying code system code = {codeSystemCode} - Page {currentPage}").Result;
            Output.WriteLine($"Result count: {codesPage.Items.Count}");

            //// Assert
            Assert.Equal(currentPage, codesPage.PagerSettings.CurrentPage);
            Assert.Equal(itemsPerPage, codesPage.PagerSettings.ItemsPerPage);
            Assert.True(codesPage.TotalItems > 0);
            Output.WriteLine($"Last page {codesPage.TotalPages}");
        }

        [Theory]
        [InlineData("Low forceps", "2.16.840.1.113883.6.104", 1, 500)]
        [InlineData("Low forceps operation", "2.16.840.1.113883.6.104", 1, 500)]
        [InlineData("episiotomy", "2.16.840.1.113883.6.104", 1, 500)]
        //[InlineData("Low forceps operation", "NOT_A_REAL_CODE", 1, 500)]
        public void GetByCodeSystem_WithFilter_ShouldHaveResults(string codeDsc, string codeSystemCode, int currentPage, int itemsPerPage)
        {
            //// Arrange
            var settings = new PagerSettings { CurrentPage = currentPage, ItemsPerPage = itemsPerPage };

            //// Act
            var codesPage = ExecuteTimed(async () => await _valueSetCodeRepository.GetByCodeSystemAsync(codeDsc, codeSystemCode, settings), $"Querying code system code = {codeSystemCode} - Page {currentPage}").Result;
            Output.WriteLine($"Result count: {codesPage.Items.Count}");

            //// Assert
            Assert.Equal(currentPage, codesPage.PagerSettings.CurrentPage);
            Assert.Equal(itemsPerPage, codesPage.PagerSettings.ItemsPerPage);
            Assert.True(codesPage.TotalItems > 0);
            Output.WriteLine($"Last page {codesPage.TotalPages}");
        }
    }
}