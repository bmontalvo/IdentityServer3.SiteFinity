﻿using IdentityServer.SiteFinity.Configuration.Hosting;
using IdentityServer.SiteFinity.ResponseHandling;
using IdentityServer.SiteFinity.Services;
using IdentityServer.SiteFinity.Utilities;
using IdentityServer.SiteFinity.Validation;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Models;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace IdentityServer.SiteFinity
{
    /// <summary>
    /// The Controller that handles the SignRequest
    /// </summary>
    [NoCache]
    [HostAuthentication(Constants.PrimaryAuthenticationType)]
    [RoutePrefix("")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SiteFinityController : ApiController
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly SignInValidator _validator;
        private readonly SignInResponseGenerator _signInResponseGenerator;
        private readonly HttpUtility _httpUtility;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="validator">The validator class</param>
        /// <param name="signInResponseGenerator">The response generator</param>
        /// <param name="httpUtility">The utily class used for url encoding and url decoding</param>
        public SiteFinityController(SignInValidator validator, SignInResponseGenerator signInResponseGenerator, HttpUtility httpUtility)
        {
            _validator = validator;
            _signInResponseGenerator = signInResponseGenerator;
            _httpUtility = httpUtility;
        }
        /// <summary>
        /// The default Get route
        /// </summary>
        /// <param name="realm">The request realm</param>
        /// <param name="tokenType">The token type</param>
        /// <param name="redirect_uri">the url to redirect the user to</param>
        /// <param name="deflate">A flag that indicate if the request is defalted</param>
        /// <param name="sign_out">A flag that indicate if it is a sign out request</param>
        /// <returns>The response</returns>
        [Route("")]
        public async Task<IHttpActionResult> Get(string realm = "", string tokenType = "", string redirect_uri = "", bool deflate = false, bool sign_out = false)
        {
            var message = new SignInRequestMessage(realm, tokenType, redirect_uri, deflate, sign_out);

            var result = await _validator.ValidateAsync(Request.RequestUri.AbsoluteUri, message, User as ClaimsPrincipal);

            if (result.IsSignout)
            {
                var url = this.Request.GetOwinContext().Environment.GetIdentityServerLogoutUrl();
                return Redirect(url);
            }

            if (result.IsSignInRequired)
            {
                Logger.Info("Redirect to login page");
                return RedirectToLogin();
            }

            if (result.IsError)
            {
                Logger.Error(result.Error);
                return BadRequest(result.Error);
            }

            var responseMessage = await _signInResponseGenerator.GenerateResponseAsync(message, result, Request);

            return responseMessage;


        }

        private IHttpActionResult RedirectToLogin()
        {
            Uri publicRequestUri = GetPublicRequestUri();

            var message = new SignInMessage();
            message.ReturnUrl = publicRequestUri.ToString();

            var env = Request.GetOwinEnvironment();
            var url = env.CreateSignInRequest(message);

            return Redirect(url);
        }

        private Uri GetPublicRequestUri()
        {
            string identityServerHost = Request.GetOwinContext()
                                               .Environment
                                               .GetIdentityServerHost();

            string pathAndQuery = Request.RequestUri.PathAndQuery;
            string requestUriString = identityServerHost + pathAndQuery;
            var requestUri = new Uri(requestUriString);

            return requestUri;
        }
    }
}
