﻿namespace Fabric.Terminology.UnitTests.IoC
{
    using System;

    using Fabric.Terminology.API.Configuration;
    using Fabric.Terminology.SqlServer.Caching;
    using Fabric.Terminology.SqlServer.Configuration;

    using Nancy.TinyIoc;

    public class ContainerFixture : IDisposable
    {
        public ContainerFixture()
        {
            var container = new TinyIoCContainer();
            this.Initialize(container);

            this.Container = container;
        }

        public TinyIoCContainer Container { get; }

        public void Dispose()
        {
            this.Container?.Dispose();
        }

        private void Initialize(TinyIoCContainer container)
        {
            var appSettings = new AppConfiguration
            {
                HostingOptions = new HostingOptions { UseIis = false },
                TerminologySqlSettings = new TerminologySqlSettings
                {
                    DefaultItemsPerPage = 100,
                    ConnectionString = "[ConnectionString]",
                    LogGeneratedSql = false,
                    MemoryCacheEnabled = true,
                    MemoryCacheMinDuration = 5,
                    MemoryCacheSliding = false
                }
            };

            container.Register<IAppConfiguration>(appSettings);
            container.Register<IMemoryCacheSettings>(appSettings.TerminologySqlSettings);
            container.Register<IMemoryCacheProvider, MemoryCacheProvider>().AsSingleton();

            container.Register<ICachingManagerFactory, CachingManagerFactory>();
        }
    }
}