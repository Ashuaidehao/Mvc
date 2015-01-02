﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace LoggingWebSite
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {    
            var configuration = app.GetTestConfiguration();

            // Set up application services
            app.UseServices(services =>
            {
                services.AddElm(options =>
                {
                    options.Filter = (name, level) => level >= LogLevel.Verbose;
                });

                // Add MVC services to the services container
                services.AddMvc(configuration);
            });

            app.Map("/logs", (appBuilder) =>
            {
                appBuilder.UseMiddleware<ElmLogSerializerMiddleware>();
            });

            app.UseElmCapture();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action}/{id?}",
                        defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{id?}");
            });
        }
    }
}
