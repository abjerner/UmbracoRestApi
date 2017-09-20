using System;
using System.Net.Http;
using System.Web.Http;
using Examine.Providers;
using LightInject;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    public class TestControllerActivator : TestControllerActivatorBase
    {
        private readonly Action<TestServices> _onServicesCreated;

        public TestControllerActivator(Action<TestServices> onServicesCreated)
        {
            _onServicesCreated = onServicesCreated;
        }

        protected override ApiController CreateController(
            IServiceFactory container, Type controllerType,
            UmbracoHelper helper,
            TestServices testServices)
        {
            _onServicesCreated(testServices);

            return (ApiController)container.GetInstance(controllerType);
        }
    }
}