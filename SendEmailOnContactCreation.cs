using System;
using CRMPluginsProject;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace CRMPlugins
{
    public class SendEmailOnContactCreation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity contact = (Entity)context.InputParameters["Target"];

                try
                {
                    if (!contact.Contains("emailaddress1"))
                        throw new InvalidPluginExecutionException("Email is required to send a welcome email.");

                    string recipientEmail = contact["emailaddress1"].ToString();

                    // Create Email Entity
                    Entity email = new Entity("email");
                    email["subject"] = "Welcome!";
                    email["description"] = $"Hello {contact["fullname"]}, welcome to {GlobalVariables.OrganizationName}!";
                    email["directioncode"] = true;

                    // Create Activity Parties
                    Entity fromParty = new Entity("activityparty");
                    fromParty["partyid"] = new EntityReference("systemuser", context.UserId);

                    Entity toParty = new Entity("activityparty");
                    toParty["partyid"] = new EntityReference("contact", contact.Id);

                    email["from"] = new Entity[] { fromParty };
                    email["to"] = new Entity[] { toParty };

                    // Create the email record
                    Guid emailId = service.Create(email);

                    // Send the email using the admin email from GlobalVariables
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
}
