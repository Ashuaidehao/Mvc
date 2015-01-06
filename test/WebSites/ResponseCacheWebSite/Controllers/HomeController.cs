// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ResponseCacheWebSite
{
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 100, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
        public IActionResult Index()
        {
            return Content("Hello World!");
        }

        [ResponseCache(Duration = 100, Location = ResponseCacheLocation.Any)]
        public IActionResult PublicCache()
        {
            return Content("Hello World!");
        }

        [ResponseCache(Duration = 100, Location = ResponseCacheLocation.Client)]
        public IActionResult ClientCache()
        {
            return Content("Hello World!");
        }

        [ResponseCache(NoStore = true)]
        public IActionResult NoStore()
        {
            return Content("Hello World!");
        }
    }
}