using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using CRMPlugins;
using Microsoft.Xrm.Sdk.Query;  // Ensure this matches the namespace of generated early-bound classes

public class UpdateContactsUsingLinq : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        tracingService.Trace("Starting plugin execution.");

        // Ensure Target Entity is an Account
        if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity account))
        {
            tracingService.Trace("Target parameter is missing or not an Entity.");
            return;
        }

        Guid accountId = account.Id;

        // Explicitly retrieve the account entity to ensure all attributes are loaded
        account = service.Retrieve("account", accountId, new ColumnSet(true));

        // Log all attributes of the account entity
        tracingService.Trace("Logging all attributes of the account entity:");
        foreach (var attribute in account.Attributes)
        {
            tracingService.Trace($"Attribute: {attribute.Key}, Value: {attribute.Value}");
        }

        // Check if PrimaryContactId attribute exists and is not null
        if (!account.Attributes.Contains("primarycontactid") || account["primarycontactid"] == null)
        {
            tracingService.Trace("PrimaryContactId attribute is missing or null.");
            return;
        }

        Guid primaryContactId = ((EntityReference)account["primarycontactid"]).Id;

        // **STEP 1: Create OrganizationServiceContext**
        using (ServiceContext svcContext = new ServiceContext(service))
        {
            // **STEP 2: LINQ Query with JOIN between Contact & Account**
            var query = from c in svcContext.ContactSet
                        join a in svcContext.AccountSet on c.AccountId.Id equals a.AccountId
                        where a.Name.Contains("ZARSHAH STORE")
                        where c.LastName.Contains("Parviz")
                        select new
                        {
                            account_name = a.Name,
                            contact_name = c.LastName
                        };



            // **STEP 3: Process Retrieved Data**
            foreach (var record in query)
            {
                tracingService.Trace($"Account: {record.account_name}, Contact: {record.contact_name}");
            }

            // Read secure and unsecure configuration
            string secureConfig = context.SharedVariables.Contains("SecureConfig") ? (string)context.SharedVariables["SecureConfig"] : "Not provided";
            string unsecureConfig = context.SharedVariables.Contains("UnsecureConfig") ? (string)context.SharedVariables["UnsecureConfig"] : "Not provided";

            tracingService.Trace($"Secure Config: {secureConfig}");
            tracingService.Trace($"Unsecure Config: {unsecureConfig}");
        }

        tracingService.Trace("Finished plugin execution.");
    }
}