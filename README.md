# Umbraco REST API

The Umbraco REST API is for content, media, members & relations. It's Based on the [HAL specification](http://stateless.co/hal_specification.html) ([GitHub link](https://github.com/mikekelly/hal_specification)) and is using a wonderful WebApi implementation of HAL which can be found on GitHub: [https://github.com/JakeGinnivan/WebApi.Hal](https://github.com/JakeGinnivan/WebApi.Hal)

__Main documentation can be found here: https://our.umbraco.org/Documentation/Implementation/Rest-Api/__

## Installation

Umbraco REST API can be installed via Nuget:

    Install-Package UmbracoCms.RestApi

This package will also install another add-on for Umbraco called [Identity Extensions](https://github.com/umbraco/UmbracoIdentityExtensions).

Once installed, a readme is displayed with a snippet of code to enable the REST API. This can be done using OWIN startup classes and because the IdentityExtensions package has
been installed, a few classes have been added to your ~/App_Startup folder. The easiest way to enable the REST API is to copy the code snippet from the readme that 
is displayed:

```
app.ConfigureUmbracoRestApi(new UmbracoRestApiOptions()
	{
		//Modify the CorsPolicy as required
		CorsPolicy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = true
            }
	});
```

then open the file: `~/App_Startup/UmbracoStandardOwinStartup.cs`

and paste this snippet just underneath this line of code: `base.Configuration(app);`

That's it, you've now enabled the REST API.

## Discovery

A great way to browse Umbraco's REST service is to use the great html/javascript [HAL Browser](https://github.com/mikekelly/hal-browser). The starting endpoints are:

* /umbraco/rest/v1/content
* /umbraco/rest/v1/media
* /umbraco/rest/v1/members
* /umbraco/rest/v1/relations


## Rest API V2 startup

There will be a number of changes in the upcoming rest api project - the startup own class has been streamlined - it could look like the sample below:

```
using Microsoft.Owin;
using Owin;
using TestSite.App_Startup;
using Umbraco.Web;
using Umbraco.RestApi;
using Umbraco.RestApi.Security;

[assembly: OwinStartup("UmbracoRestApiStartup", typeof(UmbracoRestApiStartup))]

namespace TestSite.App_Startup
{    
    public class UmbracoRestApiStartup : UmbracoDefaultOwinStartup
    {
        protected override void ConfigureMiddleware(IAppBuilder app)
        {
            base.ConfigureMiddleware(app);

            app.UseUmbracoRestApi(ApplicationContext);
            var authServerOptions = new UmbracoAuthorizationServerProviderOptions
            {
                //needed when not running on SSL and testing
                AllowInsecureHttp = true                
            };
            app.UseUmbracoTokenAuthentication(authServerOptions);

            //If you wish to allow the REST API endpoints to be authenticated with a user's back office cookie, enable this
            // https://our.umbraco.org/Documentation/Implementation/Rest-Api/#security
            //app.UseUmbracoCookieAuthenticationForRestApi(ApplicationContext);
        }
    }
}
```

