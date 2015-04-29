using System;
using Microsoft.Owin.Extensions;
using Owin;

namespace Umbraco.Web.Rest.Tests.TestHelpers
{
    public static class AuthenticateEverythingExtensions
    {
        public static IAppBuilder AuthenticateEverything(this IAppBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            app.Use(typeof(AuthenticateEverythingMiddleware), (object)app, (object)new AuthenticateEverythingMiddleware.AuthenticateEverythingAuthenticationOptions());
            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }
    }
}