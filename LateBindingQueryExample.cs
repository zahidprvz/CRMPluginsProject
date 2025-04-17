using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public class LateBindingQueryExample : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        // Retrieve services
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        tracingService.Trace("Starting Late Binding Query Execution.");

        // Ensure the Target Entity is an Account
        if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity account))
        {
            tracingService.Trace("Target parameter is missing or not an Entity.");
            return;
        }

        Guid accountId = account.Id;

        // Retrieve account data using late binding
        ColumnSet columns = new ColumnSet("name", "primarycontactid");
        Entity retrievedAccount = service.Retrieve("account", accountId, columns);

        tracingService.Trace($"Account Retrieved: {retrievedAccount.GetAttributeValue<string>("name")}");

        // Check if PrimaryContactId exists
        if (!retrievedAccount.Contains("primarycontactid") || retrievedAccount["primarycontactid"] == null)
        {
            tracingService.Trace("No primary contact associated with this account.");
            return;
        }

        Guid primaryContactId = ((EntityReference)retrievedAccount["primarycontactid"]).Id;

        // Using FetchXML to execute a late-bound query
        string fetchXml = $@"
            <fetch distinct='false' mapping='logical'>
              <entity name='contact'>
                <attribute name='fullname' />
                <attribute name='emailaddress1' />
                <filter type='and'>
                  <condition attribute='contactid' operator='eq' value='{primaryContactId}' />
                </filter>
              </entity>
            </fetch>";

        EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

        // Log the results
        foreach (Entity contact in results.Entities)
        {
            string fullName = contact.Contains("fullname") ? contact["fullname"].ToString() : "N/A";
            string email = contact.Contains("emailaddress1") ? contact["emailaddress1"].ToString() : "N/A";

            tracingService.Trace($"Contact Found: Name - {fullName}, Email - {email}");
        }

        tracingService.Trace("Late Binding Query Execution Completed.");
    }
}
