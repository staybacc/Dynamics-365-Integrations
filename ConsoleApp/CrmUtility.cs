using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using Microsoft.Xrm.Tooling.Connector;

namespace ConsoleApp
{
    public static class CrmUtility
    {
        public static List<Entity> GetEntities(CrmServiceClient service, string entityName, int topCount)
        {
            var query = new QueryExpression(entityName)
            {
                TopCount = topCount,
                ColumnSet = new ColumnSet(true)
            };

            return service.RetrieveMultiple(query).Entities.ToList();
        }
    }
}
