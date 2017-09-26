using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using AutoMapper;
using Microsoft.Owin.Security.Authorization.Infrastructure;
using Moq;
using Owin;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Models.Mapping;
using Umbraco.Web.WebApi;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class TestStartup
    {
        private readonly Action<TestServices> _serviceActivator;
        private readonly Action<IAppBuilder, ApplicationContext> _configBuilder;
        public ApplicationContext ApplicationContext { get; }

        public TestStartup(Action<TestServices> serviceActivator, Action<IAppBuilder, ApplicationContext> configBuilder = null)
        {
            _serviceActivator = serviceActivator;
            _configBuilder = configBuilder;

            var serviceContext = ServiceMocks.GetServiceContext();
            var mockedMigrationService = Mock.Get(serviceContext.MigrationEntryService);

            //set it up to return anything so that the app ctx is 'Configured'
            mockedMigrationService.Setup(x => x.FindEntry(It.IsAny<string>(), It.IsAny<SemVersion>())).Returns(Mock.Of<IMigrationEntry>());

            var dbCtx = ServiceMocks.GetDatabaseContext();

            //new app context
            ApplicationContext = ApplicationContext.EnsureContext(
                dbCtx,
                //pass in mocked services
                serviceContext,
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()),
                true);
        }

        private void Activator(TestServices testServices)
        {
            _serviceActivator(testServices);

            Mapper.Initialize(configuration =>
            {
                var contentRepresentationMapper = new ContentModelMapper();
                contentRepresentationMapper.ConfigureMappings(configuration, testServices.UmbracoContext.Application);

                var mediaRepresentationMapper = new MediaModelMapper();
                mediaRepresentationMapper.ConfigureMappings(configuration, testServices.UmbracoContext.Application);

                var memberRepresentationMapper = new MemberModelMapper();
                memberRepresentationMapper.ConfigureMappings(configuration, testServices.UmbracoContext.Application);

                var relationRepresentationMapper = new RelationModelMapper();
                relationRepresentationMapper.ConfigureMappings(configuration, testServices.UmbracoContext.Application);

                var publishedContentRepresentationMapper = new PublishedContentMapper();
                publishedContentRepresentationMapper.ConfigureMappings(configuration, testServices.UmbracoContext.Application);
            });
        }

        public void Configuration(IAppBuilder app)
        {
            var httpConfig = new HttpConfiguration();

            //this is here to ensure that multiple calls to this don't cause errors
            //httpConfig.MapHttpAttributeRoutes();
            
            httpConfig.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //TODO: enable this if strange things happen and you need to debug server errors
            //var traceWriter = httpConfig.EnableSystemDiagnosticsTracing();

            httpConfig.Services.Replace(typeof(IAssembliesResolver), new SpecificAssemblyResolver(new[] { typeof(UmbracoRestStartup).Assembly }));
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator(ApplicationContext, Activator));
            httpConfig.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(httpConfig));

            if (_configBuilder == null)
            {
                //auth everything
                app.AuthenticateEverything();
                app.ConfigureUmbracoRestApiAuthorizationPolicies(ApplicationContext);
            }
            else
            {
                //custom builder specified so will leave it up to the caller to configure the authz
                _configBuilder(app, ApplicationContext);
            }

            //Create routes
            UmbracoRestStartup.CreateRoutes(httpConfig);            

            app.UseWebApi(httpConfig);
        }
    }

}