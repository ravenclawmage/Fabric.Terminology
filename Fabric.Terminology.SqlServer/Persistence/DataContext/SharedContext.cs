﻿namespace Fabric.Terminology.SqlServer.Persistence.DataContext
{
    using System;

    using Fabric.Terminology.SqlServer.Configuration;
    using Fabric.Terminology.SqlServer.Models.Dto;

    using JetBrains.Annotations;

    using Microsoft.EntityFrameworkCore;

    internal class SharedContext : DbContext
    {
        public SharedContext(DbContextOptions options, TerminologySqlSettings settings)
            : base(options)
        {
            this.Settings = settings;
        }

        public DbSet<ValueSetCodeDto> ValueSetCodes { get; set; }

        public DbSet<ValueSetCodeCountDto> ValueSetCounts { get; set; }

        public DbSet<ValueSetDescriptionDto> ValueSetDescriptions { get; set; }

        // Used for testing
        internal bool IsInMemory { get; set; }

        internal TerminologySqlSettings Settings { get; }

        protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<ValueSetCodeDto>().ToTable("ValueSetCode", "Terminology");
            modelBuilder.Entity<ValueSetCodeDto>().HasKey(code => new { code.CodeCD, code.ValueSetGUID });

            modelBuilder.Entity<ValueSetCodeCountDto>().ToTable("ValueSetCodeCount", "Terminology");
            modelBuilder.Entity<ValueSetCodeCountDto>().HasKey(record => new { record.ValueSetGUID, record.CodeSystemGUID });

            modelBuilder.Entity<ValueSetDescriptionDto>().ToTable("ValueSetDescription", "Terminology");
            modelBuilder.Entity<ValueSetDescriptionDto>().HasKey(e => e.ValueSetGUID);

            base.OnModelCreating(modelBuilder);
        }
    }
}