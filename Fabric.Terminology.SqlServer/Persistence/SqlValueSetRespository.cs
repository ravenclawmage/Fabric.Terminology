﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fabric.Terminology.Domain;
using Fabric.Terminology.Domain.Models;
using Fabric.Terminology.Domain.Persistence;
using Fabric.Terminology.SqlServer.Caching;
using Fabric.Terminology.SqlServer.Models.Dto;
using Fabric.Terminology.SqlServer.Persistence.DataContext;
using Fabric.Terminology.SqlServer.Persistence.Mapping;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Fabric.Terminology.SqlServer.Persistence
{
    internal class SqlValueSetRespository : SqlPageableRepositoryBase<ValueSetDescriptionDto, IValueSet>, IValueSetRepository
    {
        private readonly IValueSetCodeRepository _valueSetCodeRepository;

        public SqlValueSetRespository(SharedContext sharedContext, IMemoryCacheProvider cache, ILogger logger, IValueSetCodeRepository valueSetCodeRepository) 
            : base(sharedContext, logger)
        {
            _valueSetCodeRepository = valueSetCodeRepository ?? throw new NullReferenceException(nameof(valueSetCodeRepository));
            Cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        protected virtual IMemoryCacheProvider Cache { get; }

        protected override Expression<Func<ValueSetDescriptionDto, string>> SortExpression => sortBy => sortBy.ValueSetNM;
        protected override SortDirection Direction { get; } = SortDirection.Ascending;
        protected override DbSet<ValueSetDescriptionDto> DbSet => SharedContext.ValueSetDescriptions;

        [CanBeNull]
        public IValueSet GetValueSet(string valueSetId)
        {
            var dto = DbSet.FirstOrDefault(q => q.ValueSetID == valueSetId);
            if (dto == null) return null;

            var mapper = new ValueSetMapper(Cache, _valueSetCodeRepository);

            var valueSet = mapper.Map(dto);
            ((ValueSet) valueSet).ValueSetCodes = _valueSetCodeRepository.GetValueSetCodes(valueSetId);
            return valueSet;
        }

        public Task<PagedCollection<IValueSet>> GetValueSetsAsync(IPagerSettings pagerSettings)
        {
            return FindValueSetsAsync(string.Empty, pagerSettings);
        }

        public Task<PagedCollection<IValueSet>> FindValueSetsAsync(string nameFilterText, IPagerSettings pagerSettings)
        {
            var dtos = !nameFilterText.IsNullOrWhiteSpace()
                ? DbSet.Where(dto => dto.ValueSetNM.Contains(nameFilterText))
                : DbSet;

            var ids = dtos.OrderBy(SortExpression).Select(dto => dto.ValueSetID).Skip((pagerSettings.CurrentPage - 1) * pagerSettings.ItemsPerPage).Take(pagerSettings.ItemsPerPage);

            return  CreatePagedCollectionAsync(dtos, pagerSettings, new ValueSetMapper(Cache, _valueSetCodeRepository));
        }

        //protected async Task<PagedCollection<IValueSet>> CreatePagedCollectionAsync(IQueryable<ValueSetDescriptionDto> source, IPagerSettings pagerSettings)
        //{
        //    EnsurePagerSettings(pagerSettings);

        //    var count = await source.CountAsync();
        //    var items = await source.OrderBy(SortExpression).Skip((pagerSettings.CurrentPage - 1) * pagerSettings.ItemsPerPage).Take(pagerSettings.ItemsPerPage).AsNoTracking().ToListAsync();

        //    var ids = items.Select(dto => dto.ValueSetID).ToList();

        //    return new PagedCollection<IValueSet>
        //    {
        //        TotalItems = count,
        //        PagerSettings = new PagerSettings { CurrentPage = pagerSettings.CurrentPage, ItemsPerPage = pagerSettings.ItemsPerPage },
        //        TotalPages = (int)Math.Ceiling((double)count / pagerSettings.ItemsPerPage),
        //        Items = items.Select(mapper.Map).ToList().AsReadOnly()
        //    };
        //}
    }
}