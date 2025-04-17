using Microsoft.Xrm.Sdk;
using System;

public class PreOperationPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity account = (Entity)context.InputParameters["Target"];

            if (account.LogicalName != "account") return;

            if (account.Attributes.Contains("name"))
            {
                string name = account.GetAttributeValue<string>("name");
                account["name"] = name.ToUpper(); // Convert name to uppercase before saving
            }
        }
    }
}
