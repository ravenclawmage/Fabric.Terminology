﻿namespace Fabric.Terminology.SqlServer.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Fabric.Terminology.Domain.Models;
    using Fabric.Terminology.SqlServer.Caching;
    using Fabric.Terminology.SqlServer.Persistence.DataContext;

    using Microsoft.EntityFrameworkCore;

    using Serilog;

    using ValueSetCode = Fabric.Terminology.SqlServer.Models.Dto.ValueSetCode;

    internal class SqlValueSetCodeRepository : IValueSetCodeRepository
    {
        private readonly IValueSetCachingManager<IValueSetCode> cacheManager;

        private readonly ILogger logger;

        private readonly SharedContext sharedContext;

        public SqlValueSetCodeRepository(
            SharedContext sharedContext,
            ILogger logger,
            ICachingManagerFactory cachingManagerFactory)
        {
            this.sharedContext = sharedContext;
            this.logger = logger;
            this.cacheManager = cachingManagerFactory.ResolveFor<IValueSetCode>();
        }

        public IReadOnlyCollection<IValueSetCode> GetValueSetCodes(Guid valueSetGuid)
        {
            return this.cacheManager.GetMultipleOrQuery(valueSetGuid, this.QueryValueSetCodes)
                .OrderBy(code => code.Name)
                .ToList();
        }

        public Task<Dictionary<Guid, IReadOnlyCollection<IValueSetCode>>> BuildValueSetCodesDictionary(
            IEnumerable<Guid> valueSetGuids)
        {
            return this.cacheManager.GetCachedValueDictionary(valueSetGuids, this.QueryValueSetCodeLookup);
        }

        private IReadOnlyCollection<IValueSetCode> QueryValueSetCodes(Guid valueSetGuid)
        {
            try
            {
                return this.sharedContext.ValueSetCodes.Where(dto => dto.ValueSetGUID == valueSetGuid)
                    .Select(dto => new ValueSetCode(dto))
                    .ToList();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to query ValueSetCodes by ValueSetGUID");
                throw;
            }
        }

        private ILookup<Guid, IValueSetCode> QueryValueSetCodeLookup(IEnumerable<Guid> valueSetGuids)
        {
            try
            {
                return this.sharedContext.ValueSetCodes.Where(dto => valueSetGuids.Contains(dto.ValueSetGUID))
                    .AsNoTracking()
                    .ToLookup(vsc => vsc.ValueSetGUID, vsc => (IValueSetCode)new ValueSetCode(vsc));
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Failed to query for ValueSetCode lookup for collection of ValueSetGUIDs");
                throw;
            }
        }
    }
}