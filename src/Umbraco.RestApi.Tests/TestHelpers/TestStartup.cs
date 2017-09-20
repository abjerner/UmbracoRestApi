using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using AutoMapper;
using Examine.Providers;
using Owin;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.RestApi.Models;
using Umbraco.RestApi.Models.Mapping;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    /// <summary>
    /// A collection of services that tests can use that can be mutated prior to running the test
    /// </summary>
    public class TestServices
    {
        public HttpRequestMessage HttpRequestMessage { get; }
        public UmbracoContext UmbracoContext { get; }
        public ITypedPublishedContentQuery PublishedContentQuery { get; }
        public ServiceContext ServiceContext { get; }
        public BaseSearchProvider SearchProvider { get; }
        public IUmbracoSettingsSection UmbracoSettings { get; }

        public TestServices(HttpRequestMessage httpRequestMessage, UmbracoContext umbracoContext, ITypedPublishedContentQuery publishedContentQuery, ServiceContext serviceContext, BaseSearchProvider searchProvider, IUmbracoSettingsSection umbracoSettings)
        {
            HttpRequestMessage = httpRequestMessage;
            UmbracoContext = umbracoContext;
            PublishedContentQuery = publishedContentQuery;
            ServiceContext = serviceContext;
            SearchProvider = searchProvider;
            UmbracoSettings = umbracoSettings;
        }
    }

    /// <summary>
    /// OWIN startup class for the self-hosted web server
    /// </summary>
    public class TestStartup
    {
        private readonly Action<TestServices> _activator;

        public TestStartup(Action<TestServices> activator)
        {
            _activator = activator;
        }

        private void Activator(TestServices testServices)
        {
            _activator(testServices);

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
            httpConfig.Services.Replace(typeof(IHttpControllerActivator), new TestControllerActivator(Activator));
            httpConfig.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(httpConfig));

            //auth everything
            app.AuthenticateEverything();

            //Create routes

            UmbracoRestStartup.CreateRoutes(httpConfig);

            app.UseWebApi(httpConfig);
        }
    }

}