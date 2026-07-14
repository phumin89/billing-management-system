using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Application.Validation;
using BillingManagement.Domain;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileAnnotationValidationTests
{
    [Fact]
    public void Create_and_update_apply_identical_required_rules_and_messages()
    {
        var createErrors = new AnnotationCommandValidator<CreateOwnerCompanyProfileCommand>().Validate(
            new CreateOwnerCompanyProfileCommand(
                " ", " ", null, " ", " ", " ", null, null, null, null, null, null));
        var updateErrors = new AnnotationCommandValidator<UpdateOwnerCompanyProfileCommand>().Validate(
            new UpdateOwnerCompanyProfileCommand(
                " ", " ", null, " ", " ", " ", null, null, null, null, null, null));
        var expected = new Dictionary<string, string[]>
        {
            ["CompanyName"] = ["Company name is required."],
            ["AddressLine1"] = ["Address line 1 is required."],
            ["City"] = ["City / province / state is required."],
            ["PostalCode"] = ["Postal code is required."],
            ["Country"] = ["Country is required."]
        };

        AssertErrors(expected, createErrors);
        AssertErrors(expected, updateErrors);
    }

    [Fact]
    public void Create_and_update_apply_identical_shared_maximum_lengths_and_email_order()
    {
        var createErrors = new AnnotationCommandValidator<CreateOwnerCompanyProfileCommand>().Validate(
            new CreateOwnerCompanyProfileCommand(
                Over(OwnerCompanyProfileConstraints.CompanyNameMaxLength),
                Over(OwnerCompanyProfileConstraints.AddressLine1MaxLength),
                Over(OwnerCompanyProfileConstraints.AddressLine2MaxLength),
                Over(OwnerCompanyProfileConstraints.CityProvinceStateMaxLength),
                Over(OwnerCompanyProfileConstraints.PostalCodeMaxLength),
                Over(OwnerCompanyProfileConstraints.CountryMaxLength),
                Over(OwnerCompanyProfileConstraints.TaxIdMaxLength),
                Over(OwnerCompanyProfileConstraints.PhoneMaxLength),
                Over(OwnerCompanyProfileConstraints.EmailMaxLength),
                Over(OwnerCompanyProfileConstraints.WebsiteMaxLength),
                Over(OwnerCompanyProfileConstraints.LogoReferenceMaxLength),
                Over(OwnerCompanyProfileConstraints.RegistrationNumberMaxLength)));
        var updateErrors = new AnnotationCommandValidator<UpdateOwnerCompanyProfileCommand>().Validate(
            new UpdateOwnerCompanyProfileCommand(
                Over(OwnerCompanyProfileConstraints.CompanyNameMaxLength),
                Over(OwnerCompanyProfileConstraints.AddressLine1MaxLength),
                Over(OwnerCompanyProfileConstraints.AddressLine2MaxLength),
                Over(OwnerCompanyProfileConstraints.CityProvinceStateMaxLength),
                Over(OwnerCompanyProfileConstraints.PostalCodeMaxLength),
                Over(OwnerCompanyProfileConstraints.CountryMaxLength),
                Over(OwnerCompanyProfileConstraints.TaxIdMaxLength),
                Over(OwnerCompanyProfileConstraints.PhoneMaxLength),
                Over(OwnerCompanyProfileConstraints.EmailMaxLength),
                Over(OwnerCompanyProfileConstraints.WebsiteMaxLength),
                Over(OwnerCompanyProfileConstraints.LogoReferenceMaxLength),
                Over(OwnerCompanyProfileConstraints.RegistrationNumberMaxLength)));
        var expected = new Dictionary<string, string[]>
        {
            ["CompanyName"] = [MaxMessage(OwnerCompanyProfileConstraints.CompanyNameMaxLength)],
            ["AddressLine1"] = [MaxMessage(OwnerCompanyProfileConstraints.AddressLine1MaxLength)],
            ["AddressLine2"] = [MaxMessage(OwnerCompanyProfileConstraints.AddressLine2MaxLength)],
            ["City"] = [MaxMessage(OwnerCompanyProfileConstraints.CityProvinceStateMaxLength)],
            ["PostalCode"] = [MaxMessage(OwnerCompanyProfileConstraints.PostalCodeMaxLength)],
            ["Country"] = [MaxMessage(OwnerCompanyProfileConstraints.CountryMaxLength)],
            ["TaxId"] = [MaxMessage(OwnerCompanyProfileConstraints.TaxIdMaxLength)],
            ["Phone"] = [MaxMessage(OwnerCompanyProfileConstraints.PhoneMaxLength)],
            ["Email"] =
            [
                MaxMessage(OwnerCompanyProfileConstraints.EmailMaxLength),
                "Email format is invalid."
            ],
            ["Website"] = [MaxMessage(OwnerCompanyProfileConstraints.WebsiteMaxLength)],
            ["LogoReference"] = [MaxMessage(OwnerCompanyProfileConstraints.LogoReferenceMaxLength)],
            ["RegistrationNumber"] = [MaxMessage(OwnerCompanyProfileConstraints.RegistrationNumberMaxLength)]
        };

        AssertErrors(expected, createErrors);
        AssertErrors(expected, updateErrors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_and_update_accept_optional_blank_email(string? email)
    {
        var createErrors = new AnnotationCommandValidator<CreateOwnerCompanyProfileCommand>().Validate(
            ValidCreateCommand(email));
        var updateErrors = new AnnotationCommandValidator<UpdateOwnerCompanyProfileCommand>().Validate(
            ValidUpdateCommand(email));

        Assert.Empty(createErrors);
        Assert.Empty(updateErrors);
    }

    private static void AssertErrors(
        IReadOnlyDictionary<string, string[]> expected,
        IReadOnlyDictionary<string, string[]> actual)
    {
        Assert.Equal(expected.Keys, actual.Keys);
        foreach (var error in expected)
        {
            Assert.Equal(error.Value, actual[error.Key]);
        }
    }

    private static string Over(int maximumLength) => new('x', maximumLength + 1);

    private static string MaxMessage(int maximumLength) =>
        $"Must not exceed {maximumLength} characters.";

    private static CreateOwnerCompanyProfileCommand ValidCreateCommand(string? email) =>
        new("Acme", "1 Main Street", null, "Bangkok", "10110", "Thailand", null, null, email, null, null, null);

    private static UpdateOwnerCompanyProfileCommand ValidUpdateCommand(string? email) =>
        new("Acme", "1 Main Street", null, "Bangkok", "10110", "Thailand", null, null, email, null, null, null);
}
