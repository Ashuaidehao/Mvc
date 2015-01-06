// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using ResponseCacheWebSite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ResponseCacheTests
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("ResponseCacheWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task ResponseCache_SetsAllHeaders()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/Index");

            // Assert
            var data = Assert.Single(response.Headers.GetValues("Cache-control"));
            Assert.Equal("public, max-age=100", data);
            data = Assert.Single(response.Headers.GetValues("Vary"));
            Assert.Equal("Accept", data);
        }

        public static IEnumerable<object[]> CacheControlData
        {
            get
            {
                yield return new object[] { "http://localhost/Home/PublicCache", "public, max-age=100" };
                yield return new object[] { "http://localhost/Home/ClientCache", "max-age=100, private" };
                yield return new object[] { "http://localhost/Home/NoStore", "no-store" };
            }
        }

        [Theory]
        [MemberData(nameof(CacheControlData))]
        public async Task ResponseCache_SetsDifferentCacheControlHeaders(string url, string expected)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            var data = Assert.Single(response.Headers.GetValues("Cache-control"));
            Assert.Equal(expected, data);
        }
    }
}