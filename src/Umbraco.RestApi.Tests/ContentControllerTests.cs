using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Examine;
using Examine.SearchCriteria;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Publishing;
using Umbraco.RestApi.Routing;
using Umbraco.RestApi.Tests.TestHelpers;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class ContentControllerTests : ControllerTests
    {
        [Test]
        public async Task Get_Root_With_OPTIONS()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {
                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(new[]
                    {
                        ModelMocks.SimpleMockedContent(123, -1),
                        ModelMocks.SimpleMockedContent(456, -1)
                    });

                    mockContentService.Setup(x => x.GetChildren(123)).Returns(new[] { ModelMocks.SimpleMockedContent(789, 123) });
                    mockContentService.Setup(x => x.GetChildren(456)).Returns(new[] { ModelMocks.SimpleMockedContent(321, 456) });
                });

            await Get_Root_With_OPTIONS(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Root_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {
                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);
                    mockContentService.Setup(x => x.GetRootContent()).Returns(new[]
                    {
                        ModelMocks.SimpleMockedContent(123, -1),
                        ModelMocks.SimpleMockedContent(456, -1)
                    });

                    mockContentService.Setup(x => x.GetChildren(123)).Returns(new[] { ModelMocks.SimpleMockedContent(789, 123) });
                    mockContentService.Setup(x => x.GetChildren(456)).Returns(new[] { ModelMocks.SimpleMockedContent(321, 456) });
                });

            var djson = await Get_Root_Result(startup, RouteConstants.ContentSegment);
            Assert.AreEqual(2, djson["_links"]["content"].Count());
            Assert.AreEqual(2, djson["_embedded"]["content"].Count());
        }

        [Test]
        public async Task Search_200_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    var mockSearchResults = new Mock<ISearchResults>();
                    mockSearchResults.Setup(results => results.TotalItemCount).Returns(10);
                    mockSearchResults.Setup(results => results.Skip(It.IsAny<int>())).Returns(new[]
                    {
                        new SearchResult() {Id = 789},
                        new SearchResult() {Id = 456},
                    });

                    var mockSearchProvider = Mock.Get(testServices.SearchProvider);
                    mockSearchProvider.Setup(x => x.CreateSearchCriteria()).Returns(Mock.Of<ISearchCriteria>());
                    mockSearchProvider.Setup(x => x.Search(It.IsAny<ISearchCriteria>(), It.IsAny<int>()))
                        .Returns(mockSearchResults.Object);

                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);
                    mockContentService.Setup(x => x.GetByIds(It.IsAny<IEnumerable<int>>()))
                        .Returns(new[]
                        {
                            ModelMocks.SimpleMockedContent(789),
                            ModelMocks.SimpleMockedContent(456)
                        });
                });

            await Search_200_Result(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Id_Result()
        {
            var startup = new TestStartup(
                 //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                 {
                     var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);

                     mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                     mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));

                     mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
                 });

            await Get_Id_Result(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Metadata_Result()
        {
            var startup = new TestStartup(
                 //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                 {
                     var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);

                     mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());
                     mockContentService.Setup(x => x.GetChildren(It.IsAny<int>())).Returns(new List<IContent>(new[] { ModelMocks.SimpleMockedContent(789) }));
                     mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);

                     var mockTextService = Mock.Get(testServices.ServiceContext.TextService);

                     mockTextService.Setup(x => x.Localize(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>()))
                         .Returns((string input, CultureInfo culture, IDictionary<string, string> tokens) => input);
                 });

            await Get_Metadata_Result(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Children_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) => { });

            await Get_Children_Is_200_Response(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Descendants_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) => { });

            await base.Get_Descendants_Is_200_Response(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Ancestors_Is_200_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) => { });

            await base.Get_Ancestors_Is_200_Response(startup, RouteConstants.ContentSegment);
        }

        [Test]
        public async Task Get_Children_Is_200_With_Params_Result()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);

                    mockContentService.Setup(x => x.GetById(It.IsAny<int>())).Returns(() => ModelMocks.SimpleMockedContent());

                    long total = 6;
                    mockContentService.Setup(x => x.GetPagedChildren(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), out total, It.IsAny<string>(), Direction.Ascending, It.IsAny<string>()))
                        .Returns(new List<IContent>(new[]
                        {
                            ModelMocks.SimpleMockedContent(789),
                            ModelMocks.SimpleMockedContent(456)
                        }));

                    mockContentService.Setup(x => x.HasChildren(It.IsAny<int>())).Returns(true);
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver/umbraco/rest/v1/{RouteConstants.ContentSegment}/123/children?page=2&size=2"),
                    Method = HttpMethod.Get,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(6, djson["totalResults"].Value<int>());
                Assert.AreEqual(2, djson["page"].Value<int>());
                Assert.AreEqual(2, djson["pageSize"].Value<int>());
                Assert.IsNotNull(djson["_links"]["next"]);
                Assert.IsNotNull(djson["_links"]["prev"]);
                
            }
        }

        [Test]
        public async Task Post_Is_201_Response()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    TestHelpers.ContentServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                });

            await base.Post_Is_201_Response(startup, RouteConstants.ContentSegment, new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": ""Home"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json"));
        }

        [Test]
        public async Task Post_Is_400_Validation_Required_Fields()
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
                //NOTE: it is missing
                request.Content = new StringContent(@"{
  ""contentTypeAlias"": """",
  ""parentId"": 456,
  ""templateId"": 9,
  ""name"": """",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(2, djson["totalResults"].Value<int>());
                Assert.AreEqual("content.ContentTypeAlias", djson["_embedded"]["errors"][0]["logRef"].Value<string>());
                Assert.AreEqual("content.Name", djson["_embedded"]["errors"][1]["logRef"].Value<string>());

            }
        }

        [Test]
        public async Task Post_Is_400_Validation_Property_Missing()
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
                //NOTE: it is missing
                request.Content = new StringContent(@"{
    ""name"": ""test"",  
    ""contentTypeAlias"": ""test"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""properties"": {
    ""thisDoesntExist"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(1, djson["totalResults"].Value<int>());
                Assert.AreEqual("content.properties.thisDoesntExist", djson["_embedded"]["errors"][0]["logRef"].Value<string>());

            }
        }

        [Test]
        public async Task Post_Is_400_Validation_Property_Required()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    TestHelpers.ContentServiceMocks.SetupMocksForPost(testServices.ServiceContext);

                    var mockPropertyEditor = Mock.Get(PropertyEditorResolver.Current);
                    mockPropertyEditor.Setup(x => x.GetByAlias("testEditor")).Returns(new ModelMocks.SimplePropertyEditor());
                });

            using (var server = TestServer.Create(builder => startup.Configuration(builder)))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string.Format("http://testserver/umbraco/rest/v1/{0}", RouteConstants.ContentSegment)),
                    Method = HttpMethod.Post,
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));
                //NOTE: it is missing
                request.Content = new StringContent(@"{
    ""name"": ""test"",  
    ""contentTypeAlias"": ""test"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""properties"": {
    ""TestProperty1"": """",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json");

                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

                var djson = JsonConvert.DeserializeObject<JObject>(json);

                Assert.AreEqual(1, djson["totalResults"].Value<int>());
                Assert.AreEqual("content.properties.TestProperty1.value", djson["_embedded"]["errors"][0]["logRef"].Value<string>());

            }
        }

        [Test]
        public async Task Put_Is_200_Response_Non_Published()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    TestHelpers.ContentServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                });

            await base.Put_Is_200_Response(startup, RouteConstants.ContentSegment, new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""published"": false,
  ""name"": ""Home"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json"));
        }

        [Test]
        public async Task Put_Is_200_Response_With_Published()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services
                (testServices) =>
                {
                    ContentServiceMocks.SetupMocksForPost(testServices.ServiceContext);
                    var mockContentService = Mock.Get(testServices.ServiceContext.ContentService);
                    mockContentService.Setup(x => x.SaveAndPublishWithStatus(It.IsAny<IContent>(), It.IsAny<int>(), It.IsAny<bool>()))
                        .Returns(Attempt<PublishStatus>.Succeed);
                });

            await base.Put_Is_200_Response(startup, RouteConstants.ContentSegment, new StringContent(@"{
  ""contentTypeAlias"": ""testType"",
  ""parentId"": 456,
  ""templateId"": 9,
  ""published"": true,
  ""name"": ""Home"",
  ""properties"": {
    ""TestProperty1"": ""property value1"",
    ""testProperty2"": ""property value2""
  }
}", Encoding.UTF8, "application/json"));
            
        }

    }

}