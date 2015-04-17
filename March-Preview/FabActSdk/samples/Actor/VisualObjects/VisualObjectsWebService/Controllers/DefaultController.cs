//-----------------------------------------------------------------------
// <copyright file="DefaultController.cs" company="Microsoft">
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
    /// Default controller.
    /// </summary>
    public class DefaultController : ApiController
    {
        /// <summary>
        /// Default (index). Renders text/html view.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Index()
        {
            return this.View("VisualObjects.WebService.Views.Default.Index.html", "text/html");
        }
    }
}
