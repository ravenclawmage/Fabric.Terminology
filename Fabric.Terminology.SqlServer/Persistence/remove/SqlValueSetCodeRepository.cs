﻿namespace Fabric.Terminology.SqlServer.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Fabric.Terminology.Domain.Models;
    using Fabric.Terminology.Domain.Persistence;
    using Fabric.Terminology.Domain.Persistence.Mapping;
    using Fabric.Terminology.SqlServer.Models.Dto;
    using Fabric.Terminology.SqlServer.Persistence.DataContext;
    using Fabric.Terminology.SqlServer.Persistence.Mapping;

    using Microsoft.EntityFrameworkCore;

    using Serilog;

    internal class SqlValueSetCodeRepository : IValueSetCodeRepository
    {
        private readonly Lazy<ClientTermContext> clientTermContext;

        private readonly IPagingStrategy<ValueSetCodeDto, IValueSetCode> pagingStrategy;

        public SqlValueSetCodeRepository(
            SharedContext sharedContext,
            Lazy<ClientTermContext> clientTermContext,
            ILogger logger,
            IPagingStrategy<ValueSetCodeDto, IValueSetCode> pagingStrategy)
        {
            this.SharedContext = sharedContext;
            this.clientTermContext = clientTermContext;
            this.Logger = logger;
            this.pagingStrategy = pagingStrategy;
        }

        protected SharedContext SharedContext { get; }

        protected ILogger Logger { get; }

        protected Expression<Func<ValueSetCodeDto, string>> SortExpression => sortBy => sortBy.CodeDSC;

        protected DbSet<ValueSetCodeDto> DbSet => this.SharedContext.ValueSetCodes;

        protected DbSet<ValueSetCodeDto> CustomDbSet => this.clientTermContext.Value.ValueSetCodes;

        public int CountValueSetCodes(Guid valueSetGuid, IEnumerable<string> codeSystemCodes)
        {
            var systemCodes = codeSystemCodes as string[] ?? codeSystemCodes.ToArray();
            throw new NotImplementedException();
            //return systemCodes.Any()
            //           ? this.DbSet.Count(
            //               dto => dto.ValueSetUniqueID == valueSetUniqueId && systemCodes.Contains(dto.CodeSystemCD))
            //           : this.DbSet.Count(dto => dto.ValueSetUniqueID == valueSetUniqueId);
        }

        public IReadOnlyCollection<IValueSetCode> GetValueSetCodes(
            Guid valueSetGuid,
            IEnumerable<string> codeSystemCodes)
        {
            var dtos = this.DbSet.Where(dto => dto.ValueSetGUID == valueSetGuid);

            var systemCodes = codeSystemCodes as string[] ?? codeSystemCodes.ToArray();
            if (systemCodes.Any())
            {
                dtos = dtos.Where(dto => systemCodes.Contains(dto.CodeSystemCD));
            }

            dtos = dtos.OrderBy(this.SortExpression);

            var mapper = new ValueSetCodeMapper();

            return dtos.Select(dto => mapper.Map(dto)).ToList().AsReadOnly();
        }

        public Task<ILookup<Guid, IValueSetCodeCount>> LookupValueSetStats(IEnumerable<Guid> valueSetGuids, IEnumerable<string> codeSystemCodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an association between valueset and codes
        /// </summary>
        /// <remarks>
        /// Entity Framework does not support PARTITION BY and "will most likely generate the query using CROSS APPLY"
        /// Attempt to use straight also resulted in several warnings indicating that certain portions of the query could
        /// not be translated and the expression would be evaluated in the CLR after the execution (so no performance gain).
        /// </remarks>
        /// <seealso cref="https://stackoverflow.com/questions/43906840/row-number-over-partition-by-order-by-in-entity-framework"/>
        public Task<ILookup<string, IValueSetCode>> LookupValueSetCodes(
            IEnumerable<Guid> valueSetGuids,
            IEnumerable<string> codeSystemCodes,
            int count = 5)
        {
            var setIds = valueSetUniqueIds as string[] ?? valueSetUniqueIds.ToArray();
            if (!setIds.Any())
            {
                return Task.FromResult(Enumerable.Empty<IValueSetCode>().ToLookup(vs => vs.ValueSetUniqueId, vs => vs));
            }

            var mapper = new ValueSetCodeMapper();

            var escapedSetIds = string.Join(",", setIds.Select(EscapeForSqlString).Select(v => "'" + v + "'"));

            var innerSql =
                $@"SELECT vsc.BindingID, vsc.BindingNM, vsc.CodeCD, vsc.CodeDSC, vsc.CodeSystemCD, vsc.CodeSystemNM, vsc.CodeSystemVersionTXT,
vsc.LastLoadDTS, vsc.RevisionDTS, vsc.SourceDSC, vsc.ValueSetUniqueID, vsc.ValueSetID, vsc.ValueSetNM, vsc.ValueSetOID, vsc.VersionDSC,
ROW_NUMBER() OVER (PARTITION BY vsc.ValueSetUniqueID ORDER BY vsc.ValueSetUniqueID) AS rownum 
FROM [Terminology].[ValueSetCode] vsc WHERE vsc.ValueSetUniqueID IN ({escapedSetIds})";

            var systemCodes = codeSystemCodes as string[] ?? codeSystemCodes.ToArray();
            if (systemCodes.Any())
            {
                var escapedCodes = string.Join(
                    ",",
                    systemCodes.Select(EscapeForSqlString).Select(v => "'" + v + "'"));
                innerSql += $" AND vsc.CodeSystemCD IN ({escapedCodes})";
            }

            var sql =
                $@"SELECT vscr.BindingID, vscr.BindingNM, vscr.CodeCD, vscr.CodeDSC, vscr.CodeSystemCD, vscr.CodeSystemNM, vscr.CodeSystemVersionTXT,
vscr.LastLoadDTS, vscr.RevisionDTS, vscr.SourceDSC, vscr.ValueSetUniqueID, vscr.ValueSetID, vscr.ValueSetNM, vscr.ValueSetOID, vscr.VersionDSC, vscr.rownum
FROM ({innerSql}) vscr
WHERE vscr.rownum <= {count}
ORDER BY vscr.CodeDSC";

            return Task.Run(() => this.DbSet.FromSql(sql).ToLookup(vsc => vsc.ValueSetUniqueID, vsc => mapper.Map(vsc)));
        }

        //// Used for testing.  codeSystemCodes parameter not used by required for mapper.
        internal IReadOnlyCollection<IValueSetCode> GetCustomValueSetCodes(Guid valueSetGuid, IEnumerable<string> codeSystemCodes)
        {
            var all = this.CustomDbSet.ToList();

            var dtos = this.CustomDbSet.Where(dto => dto.ValueSetGUID == valueSetGuid).OrderBy(this.SortExpression).ToList();

            var mapper = new ValueSetCodeMapper();

            return dtos.Select(dto => mapper.Map(dto)).ToList().AsReadOnly();
        }

        private static string EscapeForSqlString(string input)
        {
            return input.Replace("'", "''");
        }
    }
}