using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace CreateFollowUpTaskForLead
{
    public class CreateTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
            {
                if (entity.LogicalName != "lead")
                    return;

                try
                {
                    Entity followUp = new Entity("task");
                    followUp["subject"] = "Send email to the new customer.";
                    followUp["description"] = "Follow up with the new customer. Check if there are any issues.";
                    followUp["scheduledstart"] = DateTime.Now.AddDays(7); // FIX: Correct attribute name
                    followUp["scheduledend"] = DateTime.Now.AddDays(7);   // FIX: Correct attribute name
                    followUp["category"] = context.PrimaryEntityName;

                    // FIX: Use context.PrimaryEntityId instead of OutputParameters
                    followUp["regardingobjectid"] = new EntityReference("lead", context.PrimaryEntityId);

                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    service.Create(followUp);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowupPlugin: " + ex.ToString());
                }
            }
        }
    }
}
