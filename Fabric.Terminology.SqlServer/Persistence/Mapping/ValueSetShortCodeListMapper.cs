﻿namespace Fabric.Terminology.SqlServer.Persistence.Mapping
{
    using System.Collections.Generic;
    using System.Linq;
    using System;

    using Fabric.Terminology.Domain.Models;
    using Fabric.Terminology.SqlServer.Caching;
    using Fabric.Terminology.SqlServer.Models.Dto;
    using Fabric.Terminology.Domain.Persistence.Mapping;
    using Fabric.Terminology.Domain.Strategy;

    using JetBrains.Annotations;

    internal sealed class ValueSetShortCodeListMapper : ValueSetMapperBase, IModelMapper<ValueSetDescriptionDto, IValueSet>
    {
        private readonly IMemoryCacheProvider cache;

        private readonly ILookup<string, IValueSetCode> lookupCodes;

        private readonly IDictionary<string, IValueSet> stash;

        private readonly Func<string, string[], int> getCount;

        private readonly string[] codeSystemCds;

        public ValueSetShortCodeListMapper(
            IIsCustomValueStrategy isCustomValue,
            IMemoryCacheProvider memCache, 
            ILookup<string, IValueSetCode> lookup, 
            IDictionary<string, IValueSet> previouslyCached,
            Func<string, string[], int> getCount,
            IEnumerable<string> codeSystemCodes)
            : base(isCustomValue)
        {
            this.cache = memCache;
            this.lookupCodes = lookup;
            this.stash = previouslyCached;
            this.getCount = getCount;
            this.codeSystemCds = codeSystemCodes.ToArray();
        }

        [CanBeNull]
        public IValueSet Map(ValueSetDescriptionDto dto)
        {
            if (this.stash.ContainsKey(dto.ValueSetID))
            {
                return this.stash[dto.ValueSetID];
            }

            var cacheKey = CacheKeys.ValueSetKey(dto.ValueSetID, this.codeSystemCds);
            return (IValueSet)this.cache.GetItem(
                cacheKey, () =>
                {
                    var codes = this.lookupCodes[dto.ValueSetID].ToArray();
                    return this.Build(dto, codes, this.getCount.Invoke(dto.ValueSetID, this.codeSystemCds));
                },
                TimeSpan.FromMinutes(this.cache.Settings.MemoryCacheMinDuration),
                this.cache.Settings.MemoryCacheSliding);
        }
    }
}