using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BillingManagement.Api;

public static class ModelStateDictionaryExtensions
{
    public static void AddErrors(
        this ModelStateDictionary modelState,
        IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            foreach (var message in error.Value)
            {
                modelState.AddModelError(error.Key, message);
            }
        }
    }
}
