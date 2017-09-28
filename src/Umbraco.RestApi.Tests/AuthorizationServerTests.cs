using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Owin;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.RestApi.Security;
using Umbraco.RestApi.Tests.TestHelpers;
using Umbraco.Web.Security.Identity;

namespace Umbraco.RestApi.Tests
{
    [TestFixture]
    public class AuthorizationServerTests
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
            typeof(PropertyEditorResolver).CallStaticMethod("Reset", true);
            UmbracoRestApiOptionsInstance.Options = new UmbracoRestApiOptions();
        }

        [Test]
        public async Task DoThis()
        {
            var startup = new TestStartup(
                //This will be invoked before the controller is created so we can modify these mocked services,
                (testServices) =>
                {

                });

            var authServerOptions = new UmbracoAuthorizationServerProviderOptions
            {
                Secret = "abcdefghijklmnopqrstuvwxyz12345678909876543210",
                Audience = "test"
            };
            using (var server = TestServer.Create(app =>
            {
                var claimsFactory = new Mock<IClaimsIdentityFactory<BackOfficeIdentityUser, int>>();
                claimsFactory.Setup(x => x.CreateAsync(It.IsAny<UserManager<BackOfficeIdentityUser, int>>(), It.IsAny<BackOfficeIdentityUser>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new ClaimsIdentity(new[] { new Claim("test", "test") })));
                var userStore = Mock.Of<IUserStore<BackOfficeIdentityUser, int>>();
                var backOfficeUserManager = new Mock<BackOfficeUserManager>(userStore);
                backOfficeUserManager.Setup(x => x.FindAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(new BackOfficeIdentityUser(1, new IReadOnlyUserGroup[] { })));
                backOfficeUserManager.Object.ClaimsIdentityFactory = claimsFactory.Object;

                app.ConfigureUserManagerForUmbracoBackOffice<BackOfficeUserManager, BackOfficeIdentityUser>(
                    startup.ApplicationContext,
                    (options, context) => backOfficeUserManager.Object);

                app.UseUmbracoTokenAuthentication(authServerOptions);
                var httpConfig = startup.UseTestWebApiConfiguration(app);
                app.UseUmbracoRestApi(startup.ApplicationContext, new UmbracoRestApiOptions
                {
                    //customize the authz policies, in this case we want to allow everything
                    CustomAuthorizationPolicyCallback = (policyName, defaultPolicy) => (builder => builder.RequireAssertion(context => true))
                });
                app.UseWebApi(httpConfig);
            }))
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"http://testserver{authServerOptions.AuthEndpoint}"),
                    Method = HttpMethod.Post                    
                };
                request.Content = new StringContent("grant_type=password&username=YOURUSERNAME&password=YOURPASSWORD", Encoding.UTF8, "application/x-www-form-urlencoded");

                //grant_type=password&username=YOURUSERNAME&password=YOURPASSWORD

                //add the origin so Cors kicks in!
                request.Headers.Add("Origin", "http://localhost:12061");
                Console.WriteLine(request);
                var result = await server.HttpClient.SendAsync(request);
                Console.WriteLine(result);

                var json = await ((StreamContent)result.Content).ReadAsStringAsync();
                Console.Write(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented));

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);

                Assert.IsTrue(result.Headers.Contains("Access-Control-Allow-Origin"));
                var acao = result.Headers.GetValues("Access-Control-Allow-Origin");
                Assert.AreEqual(1, acao.Count());

                //looks like the mvc cors default is to allow the request domain instea of *
                Assert.AreEqual("http://localhost:12061", acao.First());
            }
        }
    }
}