using System;
using System.Security.Principal;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using SendEmailOnAccountCreation;

namespace SendEmailOnAccountCreation
{
    public class SendEmailPluginV2 : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // 🔹 Retrieve CRM Services
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("🔹 Email Plugin Execution Started.");

            try
            {
                // 🔹 Ensure Target Entity is an Account
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
                {
                    if (entity.LogicalName != Account.EntityLogicalName)
                    {
                        tracingService.Trace("⚠️ Target entity is not an Account. Exiting.");
                        return;
                    }

                    tracingService.Trace($"Processing Account: {entity.Id}");

                    // Convert to Early-Bound Account Entity
                    Account account = entity.ToEntity<Account>();

                    // Ensure Account Name Exists
                    string accountName = account.Name ?? "New Account";

                    tracingService.Trace($"✅ Account Name: {accountName}");

                    // 🔹 Get Account Owner
                    EntityReference ownerRef = account.OwnerId;
                    if (ownerRef == null || ownerRef.Id == Guid.Empty)
                    {
                        tracingService.Trace("❌ Account Owner is missing. Cannot send email.");
                        throw new InvalidPluginExecutionException("Account Owner cannot be null.");
                    }

                    tracingService.Trace($"✅ Account Owner: {ownerRef.Id}");

                    // 🔹 Create Email Entity
                    Email email = new Email
                    {
                        Subject = $"Welcome {accountName}!",
                        Description = $"Dear {accountName},\n\nWelcome to our company! We are excited to have you as a client.\n\nBest regards,\nYour Company",
                        DirectionCode = true, // Outgoing Email
                    };

                    tracingService.Trace("✅ Email entity created.");

                    // 🔹 Set "From" (Sender)
                    Entity fromParty = new Entity("activityparty");
                    fromParty["partyid"] = new EntityReference("systemuser", context.UserId); // Plugin Executor as Sender

                    // 🔹 Set "To" (Recipient)
                    Entity toParty = new Entity("activityparty");
                    toParty["partyid"] = new EntityReference("systemuser", ownerRef.Id); // Send to Account Owner

                    email["from"] = new EntityCollection(new Entity[] { fromParty });
                    email["to"] = new EntityCollection(new Entity[] { toParty });

                    tracingService.Trace("✅ Email sender and recipient set.");

                    // 🔹 Create Email in CRM
                    Guid emailId = service.Create(email);
                    tracingService.Trace($"✅ Email Created with ID: {emailId}");

                    // 🔹 Send Email Using CRM Action
                    var sendEmailRequest = new OrganizationRequest("SendEmail")
                    {
                        ["EmailId"] = emailId,
                        ["IssueSend"] = true
                    };

                    service.Execute(sendEmailRequest);
                    tracingService.Trace("✅ Email Sent Successfully!");
                }
                else
                {
                    tracingService.Trace("⚠️ No Target entity found in context.");
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace($"❌ CRM Service Fault: {ex.Message}");
                throw new InvalidPluginExecutionException($"CRM Service Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace($"❌ Unexpected Exception: {ex.Message}");
                throw new InvalidPluginExecutionException($"An error occurred in SendEmailPlugin: {ex.Message}", ex);
            }

            tracingService.Trace("🔹 Email Plugin Execution Ended.");
        }
    }
}
