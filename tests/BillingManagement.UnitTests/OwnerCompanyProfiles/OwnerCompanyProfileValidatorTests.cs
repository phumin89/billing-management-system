using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileValidatorTests
{
    [Fact]
    public void Create_and_update_apply_identical_rules_messages_and_error_accumulation()
    {
        var createErrors = new CreateOwnerCompanyProfileValidator().Validate(new CreateOwnerCompanyProfileCommand(
            " ", new string('x', 301), null, " ", new string('x', 51), " ",
            null, null, new string('x', 255), null, null, null));
        var updateErrors = new UpdateOwnerCompanyProfileValidator().Validate(new UpdateOwnerCompanyProfileCommand(
            " ", new string('x', 301), null, " ", new string('x', 51), " ",
            null, null, new string('x', 255), null, null, null));

        Assert.Equal(createErrors.Keys.Order(), updateErrors.Keys.Order());
        foreach (var field in createErrors.Keys)
        {
            Assert.Equal(createErrors[field], updateErrors[field]);
        }

        Assert.Equal(
            ["Must not exceed 254 characters.", "Email format is invalid."],
            createErrors["Email"]);
    }
}
