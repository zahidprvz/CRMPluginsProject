using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

public class UpdateContactsOnAccountChange : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        tracingService.Trace("UpdateContactsOnAccountChange Plugin Execution Started");

        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        // Ensure Target Entity is an Account
        if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity account))
        {
            tracingService.Trace("Error: Target entity is missing or not an Account.");
            return;
        }

        tracingService.Trace("Target Entity: Account Found");

        // Retrieve Account ID
        if (!account.Attributes.Contains("accountid"))
        {
            tracingService.Trace("Error: Account ID is missing.");
            return;
        }

        Guid accountId = account.Id;
        tracingService.Trace($"Account ID Retrieved: {accountId}");

        // **STEP 1: Use QueryExpression to Retrieve All Contacts Related to This Account**
        QueryExpression query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("contactid", "fullname", "parentcustomerid"),
            Criteria =
            {
                Conditions =
                {
                    new ConditionExpression("parentcustomerid", ConditionOperator.Equal, accountId)
                }
            }
        };

        EntityCollection contacts = service.RetrieveMultiple(query);
        tracingService.Trace($"Total Contacts Retrieved: {contacts.Entities.Count}");

        // **STEP 2: Update Each Contact's Remarks Field**
        foreach (Entity contact in contacts.Entities)
        {
            Entity updateContact = new Entity("contact")
            {
                Id = contact.Id
            };
            updateContact["description"] = "Retrieved via Query Expression";

            service.Update(updateContact);
            tracingService.Trace($"Updated Contact: {contact.Id}");
        }

        tracingService.Trace("UpdateContactsOnAccountChange Plugin Execution Completed Successfully");
    }
}
