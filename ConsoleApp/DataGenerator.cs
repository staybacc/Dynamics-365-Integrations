using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace ConsoleApp
{
    internal class DataGenerator
    {
        private readonly CrmServiceClient _service;
        private readonly Random _random = new Random();
        public const int StatusCreated = 239770000;
        public const int StatusConfirmed = 239770001;
        public const int StatusRenting = 239770002;
        public const int StatusReturned = 239770003;
        public const int StatusCanceled = 239770004;

        public DataGenerator(CrmServiceClient service)
        {
            _service = service;
        }

        public void Generate(int totalRents)
        {
            var contacts = CrmUtility.GetEntities(_service, "contact", 1000);
            var cars = CrmUtility.GetEntities(_service, "sevent_car", 1000);
            var carClasses = CrmUtility.GetEntities(_service, "sevent_carclass", 19);

            for (int i = 0; i < totalRents; i++)
            {
                var contact = PickRandom(contacts);
                var carClass = PickRandom(carClasses);

                var matchingCars = new List<Entity>();
                foreach (var c in cars)
                {
                    if (c.Contains("sevent_carclass") &&
                        ((EntityReference)c["sevent_carclass"]).Id == carClass.Id)
                    {
                        matchingCars.Add(c);
                    }
                }


                if (matchingCars.Count == 0) continue;

                var car = PickRandom(matchingCars);
                var pickupDate = RandomDate(new DateTime(2025, 1, 1), new DateTime(2026, 11, 30));
                var returnDate = pickupDate.AddDays(_random.Next(1, 30));
                int status = GetStatus();
                bool paid = GetPaid(status);

                var rent = new Entity("sevent_rent");
                rent["sevent_car"] = car.ToEntityReference();
                rent["sevent_carclass"] = carClass.ToEntityReference();
                rent["sevent_customer"] = contact.ToEntityReference();
                rent["sevent_reserverpickup"] = pickupDate;
                rent["sevent_reservedhandover"] = returnDate;
                rent["sevent_rentstatus"] = new OptionSetValue(status);
                rent["sevent_price"] = new Money(RandomPrice());
                rent["sevent_paid"] = GetPaid(status);
                rent["sevent_pickuplocation"] = new OptionSetValue(_random.Next(239770000, 239770003));
                rent["sevent_returnlocation"] = new OptionSetValue(_random.Next(239770000, 239770003));

                _service.Create(rent);
            }
        }

        private T PickRandom<T>(List<T> list)
        {
            return list[_random.Next(list.Count)];
        }

        private DateTime RandomDate(DateTime start, DateTime end)
        {
            int range = (end - start).Days;
            return start.AddDays(_random.Next(range));
        }

        private int GetStatus()
        {
            double roll = _random.NextDouble();
            if (roll < 0.05) return StatusCreated;  
            if (roll < 0.10) return StatusConfirmed;  
            if (roll < 0.15) return StatusRenting;  
            if (roll < 0.90) return StatusReturned;  
            return StatusCanceled;                   
        }

        private bool GetPaid(int status)
        {
            double roll = _random.NextDouble();

            if (status == StatusConfirmed) return roll < 0.9;      
            if (status == StatusRenting) return roll < 0.999;     
            if (status == StatusReturned) return roll < 0.9998;    

            return false;
        }

        private decimal RandomPrice()
        {
            return (decimal)_random.Next(5000, 30000) / 100;
        }
    }

}

