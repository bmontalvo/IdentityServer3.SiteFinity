using Host.Config;
using IdentityServer.SiteFinity.Configuration;
using IdentityServer.SiteFinity.Models;
using IdentityServer.SiteFinity.Services;
using IdentityServer3.Core.Configuration;
using Owin;
using System.Collections.Generic;

namespace Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/core", coreApp =>
         {
             var factory = new IdentityServerServiceFactory()
    .UseInMemoryUsers(Users.Get())
    .UseInMemoryClients(Clients.Get())
    .UseInMemoryScopes(Scopes.Get());

             var options = new IdentityServerOptions
             {
                 SiteName = "IdentityServer3 with SiteFinity",

                 SigningCertificate = Certificate.Get(),
                 Factory = factory,
                 PluginConfiguration = ConfigurePlugins,
             };

             coreApp.UseIdentityServer(options);
         });
        }

        private void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var siteFinityOptions = new SiteFinityPluginOptions(options);

            // data sources for in-memory services
            siteFinityOptions.Factory.Register(new Registration<IEnumerable<SiteFinityRelyingParty>>(SiteFinityRelyingParties.Get()));
            siteFinityOptions.Factory.SiteFinityRelyingPartyService = new Registration<ISiteFinityRelyingPartyService>(typeof(InMemoryRelyingPartyService));

            pluginApp.UseSiteFinityPlugin(siteFinityOptions);
        }
    }
}
