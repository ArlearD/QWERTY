using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QWERTYShop.Startup))]
namespace QWERTYShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
