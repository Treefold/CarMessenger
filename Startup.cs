using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CarMessenger.Startup))]
namespace CarMessenger
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
