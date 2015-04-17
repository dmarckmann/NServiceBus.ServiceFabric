//-----------------------------------------------------------------------
// <copyright file="ScriptsController.cs" company="Microsoft">
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

namespace VisualObjects.WebService.Controllers
{
    using System.Net.Http;
    using System.Web.Http;

    /// <summary>
    /// Controller that serves up JavaScript files from the Scripts directory that are included as embedded assembly resources.
    /// You can also use the FileSystem and StaticFile middleware for OWIN to render script files,
    /// or wait for ASP.NET vNext when the full MVC stack will be available for self-hosting.
    /// </summary>
    public class ScriptsController : ApiController
    {
        /// <summary>
        /// Renders javascript files in the Scripts directory.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Get(string name)
        {
            return this.View("VisualObjects.WebService.Scripts." + name, "application/javascript");
        }
    }
}
