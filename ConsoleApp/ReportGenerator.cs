using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace ConsoleApp
{
    internal class ReportGenerator
    {
        private readonly CrmServiceClient _service;
        private readonly Random _random = new Random();
        private const int PickupType = 239770000;
        private const int ReturnType = 239770001;

        public ReportGenerator(CrmServiceClient service)
        {
            _service = service;
        }

        public void GenerateReportForRent(Entity rent)
        {
            var carRef = (EntityReference)rent["sevent_car"];
            var pickupDate = (DateTime)rent["sevent_reserverpickup"];
            var returnDate = (DateTime)rent["sevent_reservedhandover"];
            var status = ((OptionSetValue)rent["sevent_rentstatus"]).Value;

            Entity pickupReport = null;
            Entity returnReport = null;

            if (!rent.Contains("sevent_pickupreporting") && (status == 239770002 || status == 239770003))
            {
                pickupReport = CreateReport(carRef, pickupDate, 0);
                rent["sevent_pickupreporting"] = pickupReport.ToEntityReference();
            }

            if (!rent.Contains("sevent_returnreporting") && status == 239770003)
            {
                returnReport = CreateReport(carRef, returnDate, 1);
                rent["sevent_returnreporting"] = returnReport.ToEntityReference();
            }

            if (pickupReport != null || returnReport != null)
            {
                _service.Update(rent);
            }
        }

        private Entity CreateReport(EntityReference car, DateTime date, int type)
        {
            var report = new Entity("sevent_cartranserreport");
            report["sevent_car"] = car;
            report["sevent_type"] = new OptionSetValue(type == 0 ? PickupType : ReturnType);
            report["sevent_date"] = date;

            bool hasDamage = _random.NextDouble() < 0.05;
            report["sevent_damages"] = hasDamage;

            if (hasDamage)
            {
                report["sevent_damagedescription"] = "damage";
            }

            report.Id = _service.Create(report);
            return report;
        }
    }
}
