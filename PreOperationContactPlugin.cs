using System;
using Microsoft.Xrm.Sdk;

public class PreOperationContactPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

        tracingService.Trace("Plugin execution started");

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity contact)
        {
            tracingService.Trace("Target Entity: Contact Found!");

            // check if job title exists and store it in a shared variable
            if (contact.Contains("jobtitle"))
            {
                string jobTitle = contact["jobtitle"].ToString();
                context.SharedVariables["SharedJobTitle"] = jobTitle;
                tracingService.Trace($"SharedVariable set: SharedJobTitle : {jobTitle}");
            }
            else
            {
                tracingService.Trace("jobTitle is not present in the Contact.");
            }
        }
        else
        {
            tracingService.Trace("Target entity is missing or null!");
        }
    }
}