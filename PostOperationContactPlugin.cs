using System;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk;

public class PostOperationContactPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        tracingService.Trace("Plugin Execution Started");

        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        if (!context.OutputParameters.Contains("id") || context.OutputParameters["id"] == null) 
        {
            tracingService.Trace("Error: OutputParameters[id] is null");
            return;
        }

        Guid contactId = (Guid)context.OutputParameters["id"];
        tracingService.Trace($"Contact ID retreived: {contactId}");

        // create the entity reference to update
        Entity contact = new Entity("contact")
        {
            Id = contactId
        };

        // retreive shared variables
        if (context.SharedVariables.Contains("SharedJobTitle"))
        {
            string sharedJobTitle = context.SharedVariables["SharedJobTitle"]?.ToString();
            tracingService.Trace($"Retreived Variable: SharedJobTitle = {sharedJobTitle}");

            // update contact description with Job Title
            contact["description"] = "Job Title: " + sharedJobTitle;
        }
        else
        {
            tracingService.Trace("Error: SharedVariable 'SharedJobTitle' was not found!");
            contact["description"] = "Job Title: (No Value Found)";
        }
        tracingService.Trace("Updating Contact Record");
        service.Update(contact);
        tracingService.Trace("Contact Updated Successfully!!");
    }
}