using System;
using Microsoft.Xrm.Sdk;

public class PreventInfiniteLoopPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        // 🔹 Check if Plugin Depth > 1 (to prevent recursion)
        if (context.Depth > 1)
        {
            return; // Exit if the plugin has already run before in this transaction
        }

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity account = (Entity)context.InputParameters["Target"];

            try
            {
                // 🔹 Update the Account's Description
                Entity updateAccount = new Entity("account");
                updateAccount.Id = account.Id;
                updateAccount["description"] = "Updated by Plugin to prevent infinite loop";

                service.Update(updateAccount);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error: " + ex.Message);
            }
        }
    }
}
