﻿namespace Fabric.Terminology.API
{
    using System;

    using AutoMapper;

    using Fabric.Terminology.API.Configuration;
    using Fabric.Terminology.API.Logging;
    using Fabric.Terminology.API.Models;
    using Fabric.Terminology.Domain.Models;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Nancy.Owin;
    using Serilog;
    using Serilog.Core;

    public class Startup
    {
        private readonly IAppConfiguration appConfig;

        public Startup(IHostingEnvironment env)
        {
            this.appConfig = new TerminologyConfigurationProvider().GetAppConfiguration(env.ContentRootPath);

            var logger = LogFactory.CreateLogger(new LoggingLevelSwitch());
            Log.Logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            loggerFactory.AddSerilog();

            Log.Logger.Information("Fabric.Terminology.API starting.");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Log.Logger.Information("Initializing AutoMapper");
            Mapper.Initialize(
                cfg =>
                    {
                        cfg.CreateMap<ICodeSetCode, CodeSetCodeApiModel>();
                        cfg.CreateMap<IValueSetCode, ValueSetCodeApiModel>();
                        cfg.CreateMap<IValueSetCodeCount, ValueSetCodeCountApiModel>();
                        cfg.CreateMap<IValueSetSummary, ValueSetItemApiModel>()
                            .ForMember(
                                dest => dest.Identifier,
                                opt => opt.MapFrom(
                                    src => src.ValueSetGuid.Equals(Guid.Empty)
                                               ? Guid.NewGuid().ToString()
                                               : src.ValueSetGuid.ToString()));

                        cfg.CreateMap<IValueSet, ValueSetApiModel>()
                            .ForMember(
                                dest => dest.Identifier,
                                opt => opt.MapFrom(
                                    src => src.ValueSetGuid.Equals(Guid.Empty)
                                               ? Guid.NewGuid().ToString()
                                               : src.ValueSetGuid.ToString()));
                    });

            app.UseStaticFiles()
                .UseOwin()
                .UseNancy(opt => opt.Bootstrapper = new Bootstrapper(this.appConfig, Log.Logger));

            Log.Logger.Information("Fabric.Terminology.API started!");
        }
    }
}
