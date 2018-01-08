using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ifound.Startup))]
namespace Ifound
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
