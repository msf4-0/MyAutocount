using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Configuration;
using Nancy.Hosting.Self;

namespace MyAutocount
{
    class MyService
    {
        NancyHost nancyHost;
        internal class CustomBootstrapper : DefaultNancyBootstrapper
        {
            public override void Configure(INancyEnvironment environment)
            {
                environment.Tracing(enabled: false, displayErrorTraces: true);
            }
        }
        public bool Start()
        {
            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };

            Auth.Init();

            nancyHost = new NancyHost(new Uri($"http://{Auth.ipAddress}:{Auth.port}"), new CustomBootstrapper(), hostConfigs);
            nancyHost.Start();

            Utils.Log("Service started");
            Utils.Log($"Running on http://{Auth.ipAddress}:{Auth.port}");

            

            return true;
        }

        public bool Stop()
        {
            nancyHost.Stop();
            Utils.Log("Service stopped");
            return true;
        }
    }
}
