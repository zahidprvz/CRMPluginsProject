using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using SendEmailOnAccountCreation;
using System.Security.Principal;


namespace SendEmailOnAccountCreation
{
    public class SendEmailPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // 🔹 Get CRM Services
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

                    // 🔹 Validate Owner Exists in CRM
                    Entity ownerEntity = service.Retrieve("systemuser", ownerRef.Id, new ColumnSet("fullname", "internalemailaddress"));
                    if (ownerEntity == null)
                    {
                        tracingService.Trace("❌ Account Owner record not found in CRM.");
                        throw new InvalidPluginExecutionException("Account Owner not found.");
                    }

                    string ownerEmail = ownerEntity.GetAttributeValue<string>("internalemailaddress");
                    if (string.IsNullOrWhiteSpace(ownerEmail))
                    {
                        tracingService.Trace("❌ Account Owner does not have an email address.");
                        throw new InvalidPluginExecutionException("Account Owner does not have an email.");
                    }

                    tracingService.Trace($"✅ Account Owner Email: {ownerEmail}");

                    // 🔹 Create "From" Activity Party
                    Entity fromParty = new Entity("activityparty");
                    fromParty["partyid"] = new EntityReference("systemuser", context.UserId); // Plugin Executor as Sender

                    // 🔹 Create "To" Activity Party
                    Entity toParty = new Entity("activityparty");
                    toParty["partyid"] = ownerRef; // Send to Account Owner

                    // 🔹 Create Email Entity
                    Entity email = new Entity("email")
                    {
                        ["subject"] = $"Welcome {accountName}!",
                        ["description"] = $"Dear {accountName},\n\nWelcome to our company! We are excited to have you as a client.\n\nBest regards,\nYour Company",
                        ["directioncode"] = true, // Outgoing Email
                        ["from"] = new EntityCollection(new[] { fromParty }),
                        ["to"] = new EntityCollection(new[] { toParty }),
                    };

                    tracingService.Trace("✅ Email entity created.");

                    // 🔹 Set Email Status to Draft Before Sending
                    email["statuscode"] = new OptionSetValue(1); // Draft Status
                    Guid emailId = service.Create(email);

                    tracingService.Trace($"✅ Email Created with ID: {emailId}");

                    // 🔹 Send Email Using CRM Action
                    SendEmailRequest sendEmailRequest = new SendEmailRequest
                    {
                        EmailId = emailId,
                        TrackingToken = "",
                        IssueSend = true
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
