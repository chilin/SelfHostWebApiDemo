using Common.Logging;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SelfHostWebApiDemo
{
    partial class WebApiService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger<WebApiService>();

        private readonly SelfHostSettings settings;
        private IDisposable server;
        private bool interactive;

        public WebApiService(SelfHostSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            var options = new StartOptions();
            options.ServerFactory = "Microsoft.Owin.Host.HttpListener";
            Log.Info(m => m("Using ServerFactory {0}", options.ServerFactory));

            if (interactive)
            {
                options.Urls.Add("http://localhost:" + settings.Port + "/");
            }
            else
            {
                var urls = settings.Urls.ToArray();
                if (!urls.Any())
                {
                    options.Port = settings.Port;
                    urls = new[] { "http://*:" + options.Port + "/" };
                }
                ((List<string>)(options.Urls)).AddRange(urls);
            }

            server = WebApp.Start<Startup>(options);
            Log.Info(m => m("Listening for HTTP requests on address(es): {0}", string.Join(", ", options.Urls)));
        }

        protected override void OnStop()
        {
            Log.Info("Stopping HTTP server.");
            server.Dispose();
        }

        public void RunInteractivley()
        {
            interactive = true;

            OnStart(new string[0]);

            Console.WriteLine("Press <enter> to stop.");
            Console.ReadLine();

            OnStop();
        }
    }
}
