using Microsoft.Owin;
using Owin;
using System;
using System.Web.Http;
using System.Reflection;
using System.Web.Http.ExceptionHandling;
using SelfHostWebApiDemo.Log;
using System.Web.Http.Tracing;
using Microsoft.Owin.Diagnostics;

namespace SelfHostWebApiDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage(ErrorPageOptions.ShowAll);

            // Configure Web API. 
            var config = new HttpConfiguration();

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });
            // Configure help page
            //HelpPageConfig.Register(config);

            //Remove the XM Formatter from the web api
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            //Trace & Error log
            config.EnableSystemDiagnosticsTracing();
#if DEBUG
            config.Services.Replace(typeof(ITraceWriter), new TraceLogger());
#endif
            config.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());

            app.UseWebApi(config);
        }
    }
}
