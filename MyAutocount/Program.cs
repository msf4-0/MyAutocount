using System;
using Topshelf;
using static MyAutocount.Utils;

namespace MyAutocount
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = HostFactory.New(x =>
            {
                x.Service<MyService>(s =>
                {
                    s.ConstructUsing(settings => new MyService());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                    //s.WhenShutdown(service => service.Stop());
                });
                x.StartAutomaticallyDelayed();
                x.SetServiceName("MyAutocountServer");
                x.SetDisplayName("MyAutocount API Server");
                x.SetDescription("Create REST API server for Autocount.");
                x.RunAsLocalSystem();   // NT AUTHORITY \ SYSTEM
                

            });

            host.Run();

        }

    }

}
