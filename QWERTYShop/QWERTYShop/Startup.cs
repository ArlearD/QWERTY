using Microsoft.Owin;
using Owin;
using QWERTYShop.Models;
using System.Data.Entity;

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
