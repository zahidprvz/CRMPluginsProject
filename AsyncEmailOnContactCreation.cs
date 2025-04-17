using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

public class AsyncEmailOnContactCreation : IPlugin
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
                // Ensure contact has an email address
                if (!contact.Attributes.Contains("emailaddress1") || string.IsNullOrEmpty(contact["emailaddress1"].ToString()))
                {
                    return; // Exit if no email is provided
                }

                string contactEmail = contact["emailaddress1"].ToString();
                string contactName = contact.Contains("fullname") ? contact["fullname"].ToString() : "Customer";

                // Create Email Entity
                Entity email = new Entity("email");
                email["subject"] = "Welcome!";
                email["description"] = $"Dear {contactName},\n\nWelcome to our company! We are glad to have you on board.";
                email["directioncode"] = true;

                // Create Activity Parties (From & To)
                Entity fromParty = new Entity("activityparty");
                fromParty["partyid"] = new EntityReference("systemuser", context.UserId); // Email sender (logged-in user)

                Entity toParty = new Entity("activityparty");
                toParty["partyid"] = new EntityReference("contact", contact.Id); // Email recipient (new contact)

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
