using BillingManagement.Api;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BillingManagement.IntegrationTests;

public sealed class ModelStateDictionaryExtensionsTests
{
    [Fact]
    public void AddErrors_accumulates_all_fields_and_messages_in_order()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("CompanyName", "Existing message.");
        var errors = new Dictionary<string, string[]>
        {
            ["CompanyName"] = ["Company name is required.", "Must not exceed 200 characters."],
            ["Email"] = ["Email format is invalid."]
        };

        modelState.AddErrors(errors);

        Assert.Equal(
            ["Existing message.", "Company name is required.", "Must not exceed 200 characters."],
            modelState["CompanyName"]!.Errors.Select(error => error.ErrorMessage));
        Assert.Equal(
            ["Email format is invalid."],
            modelState["Email"]!.Errors.Select(error => error.ErrorMessage));
    }
}
