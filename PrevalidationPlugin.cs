using System;
using Microsoft.Xrm.Sdk;

public class PreValidationPrimaryContactCheck : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity account = (Entity)context.InputParameters["Target"];

            // Check if the Primary Contact field is missing or empty
            if (!account.Attributes.Contains("primarycontactid") || account["primarycontactid"] == null)
            {
                throw new InvalidPluginExecutionException("Primary Contact is required for creating an Account!");
            }
        }
    }
}
