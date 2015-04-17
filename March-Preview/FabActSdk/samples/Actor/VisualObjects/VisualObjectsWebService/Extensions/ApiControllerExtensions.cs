//-----------------------------------------------------------------------
// <copyright file="ApiControllerExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//      EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//      OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
//      The example companies, organizations, products, domain names,
//      e-mail addresses, logos, people, places, and events depicted
//      herein are fictitious.  No association with any real company,
//      organization, product, domain name, email address, logo, person,
//      places, or events is intended or should be inferred.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Web.Http
{
    using IO;
    using Net.Http;
    using Reflection;
    using Text;

    /// <summary>
    /// Extension methods for ApiController to help render HTML and JS files.
    /// This is here because we are using Web API but want to also render an HTML page.
    /// Normally we use ASP.NET MVC for this, but that still requires IIS (System.Web) at this time and we need to self-host,
    /// for which we use OWIN. 
    /// ASP.NET vNext will allow self-host with MVC, at which point we won't need these extensions any more.
    /// </summary>
    public static class ApiControllerExtensions
    {
        /// <summary>
        /// Creates an HttpResponseMessage from the specified view with the specified media type.
        /// The view must a fully-qualified assembly name of an embedded resources.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="view"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static HttpResponseMessage View(this ApiController instance, string view, string mediaType)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(view))
            using (var reader = new StreamReader(stream))
            {
                var message = new HttpResponseMessage();
                message.Content = new StringContent(reader.ReadToEnd(), Encoding.UTF8, mediaType);
                return message;
            }
        }

    }
}
