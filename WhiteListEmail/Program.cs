using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace WhiteListEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<WhiteListing>(s =>
                {
                    s.ConstructUsing(whitelisting => new WhiteListing());
                    s.WhenStarted(whitelisting => whitelisting.Start());
                    s.WhenStopped(whitelisting => whitelisting.Stop());
                });



                x.RunAsLocalSystem();

                x.SetServiceName("WhiteListService");
                x.SetDisplayName("White Listion Service");
                x.SetDescription("This service automatically puts white listed emails to elastic search.");


            });


            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
