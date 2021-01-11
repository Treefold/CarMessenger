using Microsoft.Owin;
using Owin;
using System.Web.Routing;

[assembly: OwinStartupAttribute(typeof(CarMessenger.Startup))]
namespace CarMessenger
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //app.MapSignalR();

            //app.UseRouting()
            //app.MapSignalR();

            //app.MapHubs();
        }
    }
}
