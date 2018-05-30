﻿// MIT License
//
// Copyright(c) 2017 Boris Wilhelms
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Azure;
using Microsoft.Fx.Portability.Azure.Storage;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reports;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PortabilityService.Functions.DependencyInjection
{
    public class InjectConfiguration : IExtensionConfigProvider
    {
        public virtual void Initialize(ExtensionConfigContext context)
        {
            var services = new ServiceCollection();

            RegisterServices(services);

            var serviceProvider = services.BuildServiceProvider(true);

            context
                .AddBindingRule<InjectAttribute>()
                .Bind(new InjectBindingProvider(serviceProvider));

            var registry = context.Config.GetService<IExtensionRegistry>();
            var filter = new ScopeCleanupFilter();

            registry.RegisterExtension(typeof(IFunctionInvocationFilter), filter);
            registry.RegisterExtension(typeof(IFunctionExceptionFilter), filter);
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // TODO: retrieve setting from configuration service
            var connection = ConfigurationManager.AppSettings["CloudStorageConnectionString"];

            if (string.IsNullOrEmpty(connection))
            {
                throw new Exception("Analyze request storage connection string is not specified");
            }

            services.AddSingleton(CloudStorageAccount.Parse(connection));
            services.AddScoped<IStorage, AzureStorage>(CreateStorage);
            services.AddSingleton<ITargetMapper>(new TargetMapper());
            services.AddSingleton<IEnumerable<IReportWriter>>(CreateReportWriters);
            services.AddSingleton<ResultFormatInformation>(GetDefaultResultFormat);

            // TODO this for development only, don't use in production
            services.AddSingleton<IReportTokenValidator, ReversedIdTokenValidator>();
        }

        private static ResultFormatInformation GetDefaultResultFormat(IServiceProvider arg)
            => new ExcelReportWriter(null).Format;

        private static AzureStorage CreateStorage(IServiceProvider arg)
        {
            var account = arg.GetService<CloudStorageAccount>();

            return new AzureStorage(account);
        }

        private static IEnumerable<IReportWriter> CreateReportWriters(IServiceProvider arg)
        {
            var targetMapper = arg.GetService<ITargetMapper>();

            return new IReportWriter[]
            {
                new ExcelReportWriter(targetMapper),
                // TODO HtmlReportWriter fails, possibly because RazorEngine can't write temp files to disk
                //new HtmlReportWriter(s_targetMapper),
                new JsonReportWriter()
            };
        }
    }
}