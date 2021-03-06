﻿namespace Fabric.Terminology.TestsBase.Mocks
{
    using System;
    using System.Collections.Generic;

    using Fabric.Terminology.API.Models;
    using Fabric.Terminology.Domain.Models;

    public class MockApiModelBuilder
    {
        public static ValueSetCreationApiModel ValueSetCreationApiModel(
            string name,
            int codeCount = 10)
        {
            return new ValueSetCreationApiModel
            {
                Name = name,
                AuthoringSourceDescription = "Test Authoring Source Description",
                DefinitionDescription = "Test Purpose Description",
                SourceDescription = "Test Source Description",
                CodeSetCodes = CodeSetCodeApiModelCollection(codeCount),
                VersionDate = DateTime.UtcNow
            };
        }

        public static IEnumerable<CodeSetCodeApiModel> CodeSetCodeApiModelCollection(int count = 10)
        {
            var codeSystems = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            }.ToArray();

            for (var i = 0; i < count; i++)
            {
                yield return CodeSetCodeApiModel($"Code.Set.Code.{i}", $"Code.Set.Code.{i} Name", i % 2 == 0 ? codeSystems[0] : codeSystems[1]);
            }
        }

        public static CodeSetCodeApiModel CodeSetCodeApiModel(string code, string name, Guid codeSystem)
        {
            return new CodeSetCodeApiModel
            {
                Code = code,
                Name = name,
                CodeSystemGuid = codeSystem,
                CodeSystemName = "Generated code system"
            };
        }
    }
}
