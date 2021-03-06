﻿namespace Fabric.Terminology.API.MetaData
{
    using Swagger.ObjectModel;

    internal static class ParameterFactory
    {
        public static Parameter GetValueSetGuidArray()
        {
            return new Parameter
            {
                Name = "valueSetGuid",
                In = ParameterIn.Path,
                Required = true,
                CollectionFormat = CollectionFormats.Csv,
                Description = "A CSV string of ValueSetGuids",
                Type = "string"
            };
        }

        public static Parameter GetCodeSystemGuidsArray()
        {
            return new Parameter
            {
                Name = "$codesystems",
                In = ParameterIn.Query,
                Required = false,
                CollectionFormat = CollectionFormats.Csv,
                Description = "An array of Code System identifiers (CodeSystemGUID) - used to filter ValueSet Codes",
                Type = "string"
            };
        }

        public static Parameter GetValueSetReferenceId()
        {
            return new Parameter
            {
                Name = "referenceId",
                Description = "The published ValueSet (usually the OID)",
                Required = true,
                In = ParameterIn.Path,
                Type = "string"
            };
        }

        public static Parameter GetSummary()
        {
            return new Parameter
            {
                Name = "$summary",
                Description = "Indicates ValueSets returned should be partial summaries - e.g. codes listing is not complete and intended to be used for presentation.",
                Required = false,
                In = ParameterIn.Query,
                Type = "boolean"
            };
        }

        public static Parameter GetContentType()
        {
            return new Parameter
            {
                Name = "Content-Type",
                Description = "Content-Type Header",
                Required = true,
                Type = "string",
                In = ParameterIn.Header,
                Default = "application/json"
            };
        }

        public static Parameter GetSkip()
        {
            return new Parameter
            {
                Name = "$skip",
                Description = "Skip 'X' number of pages for the 'current' page",
                In = ParameterIn.Query,
                Required = false,
                Type = "integer"
            };
        }

        public static Parameter GetTop(int defaultItemsPerPage = 20)
        {
            return new Parameter
            {
                Name = "$top",
                Description = "The number of items to be included in a page",
                In = ParameterIn.Query,
                Required = false,
                Type = "integer"
            };
        }
    }
}