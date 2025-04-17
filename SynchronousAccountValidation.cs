using System;
using Microsoft.Xrm.Sdk;

public class SynchronousAccountValidation : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity account = (Entity)context.InputParameters["Target"];

            // Ensure the "name" field exists and is not "Test"
            if (account.Attributes.Contains("name") && account["name"].ToString().ToLower() == "test")
            {
                throw new InvalidPluginExecutionException("The account name 'Test' is not allowed.");
            }
        }
    }
}
