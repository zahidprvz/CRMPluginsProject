using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public class QueryExpressionPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)   
    {
        // 🔹 Step 1: Get Context, Tracing, and Service Objects
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        tracingService.Trace("QueryExpressionPlugin Execution Started");

        // 🔹 Step 2: Check if Contact is Created
        if (!context.OutputParameters.Contains("id") || context.OutputParameters["id"] == null)
        {
            tracingService.Trace("Error: No Contact ID found in Output Parameters.");
            return;
        }

        Guid contactId = (Guid)context.OutputParameters["id"];
        tracingService.Trace($"Contact Created: {contactId}");

        // 🔹 Step 3: Define Query Expression for Account Entity
        QueryExpression query = new QueryExpression("account") // Table Name
        {
            ColumnSet = new ColumnSet("name"), // Select only the "name" field
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("statecode", ConditionOperator.Equal, 0) // 0 = Active Accounts
                }
            }
        };

        // 🔹 Step 4: Execute the Query
        EntityCollection accounts = service.RetrieveMultiple(query);
        tracingService.Trace($"Total Active Accounts Found: {accounts.Entities.Count}");

        // 🔹 Step 5: Log Account Names in Trace
        foreach (Entity account in accounts.Entities)
        {
            string accountName = account.Contains("name") ? account["name"].ToString() : "(No Name)";
            tracingService.Trace($"Account: {accountName}");
        }

        tracingService.Trace("QueryExpressionPlugin Execution Completed");
    }
}
