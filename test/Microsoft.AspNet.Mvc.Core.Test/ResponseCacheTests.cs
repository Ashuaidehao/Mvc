// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.PipelineCore.Collections;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ResponseCacheTests
    {
        public static IEnumerable<object[]> CacheControlData
        {
            get
            {
                yield return new object[] { new ResponseCache { NoStore = true }, "no-store" };
                // If no-store is set, then location cannot be set.
                yield return new object[] {
                    new ResponseCache { NoStore = true, Location = ResponseCacheLocation.Client },
                    "no-store"
                };
                yield return new object[] {
                    new ResponseCache { NoStore = true, Location = ResponseCacheLocation.Any },
                    "no-store"
                };
                // If no-store is set, then duration cannot be set.
                yield return new object[] { new ResponseCache { NoStore = true, Duration = 100 }, "no-store" };
                yield return new object[] { new ResponseCache { NoStore = true, Duration = 20 }, "no-store" };

                yield return new object[] { new ResponseCache { Location = ResponseCacheLocation.Client }, "private" };
                yield return new object[] { new ResponseCache { Location = ResponseCacheLocation.Any }, "public" };
                yield return new object[] {
                    new ResponseCache { Location = ResponseCacheLocation.Client, Duration=10 },
                    "private,max-age=10"
                };
                yield return new object[] {
                    new ResponseCache { Location = ResponseCacheLocation.Any, Duration = 31536000 },
                    "public,max-age=31536000"
                };
            }
        }

        [Theory]
        [MemberData(nameof(CacheControlData))]
        public void ResponseCacheCanSetCacheControlHeaders(ResponseCache cache, string output)
        {
            // Arrange
            var context = GetActionContext();

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal(output, context.HttpContext.Response.Headers.Get("Cache-control"));
        }

        public static IEnumerable<object[]> VaryData
        {
            get
            {
                yield return new object[] { new ResponseCache { VaryByHeader = "Accept" }, "Accept", null };
                yield return new object[] {
                    new ResponseCache { VaryByHeader = "Accept", NoStore = true },
                    "Accept",
                    "no-store"
                };
                yield return new object[] {
                    new ResponseCache { Location = ResponseCacheLocation.Client, VaryByHeader = "Accept" },
                    "Accept",
                    "private"
                };
                yield return new object[] {
                    new ResponseCache { Location = ResponseCacheLocation.Any, VaryByHeader = "Test" },
                    "Test",
                    "public"
                };
                yield return new object[] {
                    new ResponseCache { Location = ResponseCacheLocation.Client, Duration=10, VaryByHeader = "Test" },
                    "Test",
                    "private,max-age=10"
                };
                yield return new object[] {
                    new ResponseCache {
                        Location = ResponseCacheLocation.Any,
                        Duration = 31536000,
                        VaryByHeader = "Test"
                    },
                    "Test",
                    "public,max-age=31536000"
                };
            }
        }

        [Theory]
        [MemberData(nameof(VaryData))]
        public void ResponseCacheCanSetVary(ResponseCache cache, string varyOutput, string cacheControlOutput)
        {
            // Arrange
            var context = GetActionContext();

            // Act
            cache.OnActionExecuting(context);

            // Assert
            Assert.Equal(varyOutput, context.HttpContext.Response.Headers.Get("Vary"));
            Assert.Equal(cacheControlOutput, context.HttpContext.Response.Headers.Get("Cache-control"));
        }

        private ActionExecutingContext GetActionContext()
        {
            var httpContext = new Mock<HttpContext>();
            var headers = new HeaderDictionary(new Dictionary<string, string[]>());
            httpContext.Setup(c => c.Response.Headers).Returns(headers);
            return new ActionExecutingContext(
                new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor()),
                new IFilter[] { Mock.Of<IFilter>(), },
                new Dictionary<string, object>());
        }
    }
}