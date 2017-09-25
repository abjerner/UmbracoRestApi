using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Cors;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class CorsTests
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            ConfigurationManager.AppSettings.Set("umbracoPath", "~/umbraco");
            ConfigurationManager.AppSettings.Set("umbracoConfigurationStatus", UmbracoVersion.Current.ToString(3));            
        }

        [TearDown]
        public void TearDown()
        {
            //Hack - because Reset is internal
            typeof (PropertyEditorResolver).CallStaticMethod("Reset", true);
            UmbracoRestApiOptionsInstance.Options = new UmbracoRestApiOptions();
        }

        [Test]
        public async Task Default_Options_Allow_Any_Origin()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {
                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());
                });

            using (var server = TestServer.Create(builder =>
            {
                startup.Configuration(builder);

                //default options
                builder.ConfigureUmbracoRestApi(new UmbracoRestApiOptions(), startup.ApplicationContext);
            }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };
                //add the origin so Cors kicks in!
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent) result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());

                //looks like the mvc cors default is to allow the request domain instea of *
                Assert.AreEqual("http://localhost:12061", acao.First());
            }
        }

        [Test]
        public async Task Supports_Creds()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {
                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(Enumerable.Empty<IContent>());
                });

            using (var server = TestServer.Create(builder =>
            {
                startup.Configuration(builder);

                //default options
                builder.ConfigureUmbracoRestApi(new UmbracoRestApiOptions
                {
                    CorsPolicy = new CorsPolicy()
                    {
                        AllowAnyOrigin = true,
                        SupportsCredentials = true
                    }
                }, startup.ApplicationContext);
            }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Get,
                };
                //add the origin so Cors kicks in!
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());
                Assert.AreEqual("http://localhost:12061", acao.First());
            }
        }

        [Test]
        public async Task Supports_Post()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                   TestHelpers.ContentServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                request.Headers.Add("Origin", "http://localhost:12061");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                request.Content = new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": ""Home"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                //CORS
                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());
                Assert.AreEqual("http://localhost:12061", acao.First());

                //Creation
                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            }
        }


    }
}
