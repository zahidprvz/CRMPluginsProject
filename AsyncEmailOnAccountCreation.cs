using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

public class AsyncEmailOnAccountCreation : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity account = (Entity)context.InputParameters["Target"];

            try
            {
                // Create Email Entity
                Entity email = new Entity("email");
                email["subject"] = "Welcome!";
                email["description"] = $"Account {account["name"]} has been created successfully.";
                email["directioncode"] = true;

                // Create Activity Parties (From & To)
                Entity fromParty = new Entity("activityparty");
                fromParty["partyid"] = new EntityReference("systemuser", context.UserId);

                Entity toParty = new Entity("activityparty");
                toParty["partyid"] = new EntityReference("account", account.Id);

                email["from"] = new Entity[] { fromParty };
                email["to"] = new Entity[] { toParty };

                // Create the email record
                Guid emailId = service.Create(email);

                // Send the email
                SendEmailRequest sendEmailRequest = new SendEmailRequest
                {
                    EmailId = emailId,
                    IssueSend = true
                };
                service.Execute(sendEmailRequest);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in sending email: " + ex.Message);
            }
        }
    }
}
