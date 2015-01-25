using System;
using log4net.Config;
using Topshelf;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            HostFactory.Run(x =>
            {
                x.Service<Server>(s =>
                {
                    s.ConstructUsing(
                        name =>
                            new Server(new Configuration {Port = 2400, InteractiveMode = Environment.UserInteractive}));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.UseLog4Net();
                x.RunAsLocalSystem();
                x.SetDescription("Sample Server");
                x.SetDisplayName("SampleServer");
                x.SetServiceName("SampleServer");
            });
        }
    }
}