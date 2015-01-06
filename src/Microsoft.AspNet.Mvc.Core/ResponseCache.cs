// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An action filter which sets the appropriate headers related to Response caching.
    /// </summary>
    public class ResponseCache : ActionFilterAttribute
    {
        // A nullable-int cannot be used as an Attribute parameter.
        // Hence this nullable-int is present to back the Duration property.
        private int? duration = null;

        /// <summary>
        /// Initializes a new instance of <see cref="ResponseCache"/>
        /// </summary>
        public ResponseCache()
        {
            Location = ResponseCacheLocation.Default;
            NoStore = false;
            VaryByHeader = null;
        }

        /// <summary>
        /// Duration for which the data from a particular URL must be cached.
        /// </summary>
        public int Duration
        { 
            get
            {
                return (int)duration;
            }
            set
            {
                duration = value;
            }
        }

        /// <summary>
        /// Location where the data from a particular URL must be cached.
        /// </summary>
        public ResponseCacheLocation Location { get; set; }

        /// <summary>
        /// Boolean which determines whether the data should be stored or not.
        /// </summary>
        public bool NoStore { get; set; }

        /// <summary>
        /// Sets the "Vary" header in the response with this value.
        /// </summary>
        public string VaryByHeader { get; set; }

        // <inheritdoc />
        public override void OnActionExecuting([NotNull] ActionExecutingContext context)
        {
            if (VaryByHeader != null)
            {
                context.HttpContext.Response.Headers.Append("Vary", VaryByHeader);
            }

            if (NoStore)
            {
                context.HttpContext.Response.Headers.Append("Cache-control", "no-store");
            }
            else
            {
                if (Location != ResponseCacheLocation.Default)
                {
                    if (Location == ResponseCacheLocation.Any)
                    {
                        context.HttpContext.Response.Headers.Append("Cache-control", "public");
                    }
                    else if (Location == ResponseCacheLocation.Client)
                    {
                        context.HttpContext.Response.Headers.Append("Cache-control", "private");
                    }
                }

                if (duration != null)
                {
                    context.HttpContext.Response.Headers.Append("Cache-control", "max-age=" + Duration.ToString());
                }
            }
        }
    }
}