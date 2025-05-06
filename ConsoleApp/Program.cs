using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Tooling.Connector;
using System.ServiceModel;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Extensions.Configuration;


namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appconfig.json")
                .Build();

            var authString = config["CRMConfig:AuthString"];

            CrmServiceClient service = new CrmServiceClient(authString);

            if (!service.IsReady)
            {
                Console.WriteLine("Error: " + service.LastCrmError);
                Console.WriteLine("Detail: " + service.LastCrmException?.ToString());
                return;
            }
            else
            {
                Console.WriteLine("Connected");
            }

            var generator = new DataGenerator(service);
            generator.Generate(5000);

            Console.WriteLine("Generation is complete");


            var reportGenerator = new ReportGenerator(service);

            var rents = CrmUtility.GetEntities(service, "sevent_rent", 5000);


            foreach (var rent in rents)
            {
                var status = ((OptionSetValue)rent["sevent_rentstatus"]).Value;
                if (status == 239770002 || status == 239770003) 
                {
                    reportGenerator.GenerateReportForRent(rent);
                }
            }

            Console.WriteLine("Report generation complete");

            Console.Read();
        }
    }
}
