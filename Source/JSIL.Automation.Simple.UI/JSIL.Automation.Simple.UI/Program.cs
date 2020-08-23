using Microsoft.Extensions.Configuration;
using System.IO;

namespace JSIL.Automation.Simple.UI
{
    class Program
    {
        private static readonly IConfiguration Configuration;

        static Program()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        static void Main(string[] args)
        {
            new ChromiumImplmentation(Configuration).Run();
        }
    }
}
