using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Contracts.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.Api.Controllers;

[ApiController]
[Route("api/owner-company-profile")]
public sealed class OwnerCompanyProfileController(
    ICommandDispatcher commandDispatcher,
    GetOwnerCompanyProfileHandler getHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OwnerCompanyProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var profile = await getHandler.Handle(new GetOwnerCompanyProfileQuery(), cancellationToken);

        return profile is null ? this.NotFound() : this.Ok(ToResponse(profile));
    }

    [HttpPost]
    public async Task<ActionResult<OwnerCompanyProfileResponse>> Create(
        CreateOwnerCompanyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send<CreateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>(
            new CreateOwnerCompanyProfileCommand(
            request.CompanyName ?? string.Empty,
            request.AddressLine1 ?? string.Empty,
            request.AddressLine2,
            request.CityProvinceState ?? string.Empty,
            request.PostalCode ?? string.Empty,
            request.Country ?? string.Empty,
            request.TaxId,
            request.Phone,
            request.Email,
            request.Website,
            request.LogoReference,
            request.RegistrationNumber), cancellationToken);

        if (!result.IsSuccess)
        {
            return this.ToProblemDetails(result.Error!);
        }

        return this.CreatedAtAction(nameof(Get), ToResponse(result.Value!));
    }

    [HttpPut]
    public async Task<ActionResult<OwnerCompanyProfileResponse>> Update(
        UpdateOwnerCompanyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send<UpdateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>(
            new UpdateOwnerCompanyProfileCommand(
            request.CompanyName ?? string.Empty,
            request.AddressLine1 ?? string.Empty,
            request.AddressLine2,
            request.CityProvinceState ?? string.Empty,
            request.PostalCode ?? string.Empty,
            request.Country ?? string.Empty,
            request.TaxId,
            request.Phone,
            request.Email,
            request.Website,
            request.LogoReference,
            request.RegistrationNumber), cancellationToken);

        if (!result.IsSuccess)
        {
            return this.ToProblemDetails(result.Error!);
        }

        return this.Ok(ToResponse(result.Value!));
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send<DeleteOwnerCompanyProfileCommand, bool>(
            new DeleteOwnerCompanyProfileCommand(), cancellationToken);

        if (!result.IsSuccess)
        {
            return this.ToProblemDetails(result.Error!);
        }

        return this.NoContent();
    }

    [HttpPut("cover")]
    public async Task<ActionResult> UploadCover(
        [FromForm] IFormFile? file,
        [FromServices] ICompanyProfileCoverService coverService,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return this.ToProblemDetails(BillingManagement.Application.Abstractions.Results.ApplicationError.Validation(
                "owner_company_profile.cover_invalid",
                "The company profile cover is invalid.",
                new Dictionary<string, string[]> { ["file"] = ["A cover image file is required."] }));
        }

        await using var content = file.OpenReadStream();
        var result = await coverService.UploadAsync(content, cancellationToken);
        return result.IsSuccess ? this.NoContent() : this.ToProblemDetails(result.Error!);
    }

    [HttpGet("cover")]
    public async Task<ActionResult> GetCover(
        [FromServices] ICompanyProfileCoverService coverService,
        CancellationToken cancellationToken)
    {
        var result = await coverService.OpenReadAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            return this.ToProblemDetails(result.Error!);
        }

        this.Response.Headers.XContentTypeOptions = "nosniff";
        return this.File(result.Value!.Content, result.Value.ContentType);
    }

    [HttpDelete("cover")]
    public async Task<ActionResult> ResetCover(
        [FromServices] ICompanyProfileCoverService coverService,
        CancellationToken cancellationToken)
    {
        var result = await coverService.ResetAsync(cancellationToken);
        return result.IsSuccess ? this.NoContent() : this.ToProblemDetails(result.Error!);
    }

    [HttpPut("icon")]
    public async Task<ActionResult> UploadIcon(
        [FromForm] IFormFile? file,
        [FromServices] ICompanyProfileIconService iconService,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return this.ToProblemDetails(BillingManagement.Application.Abstractions.Results.ApplicationError.Validation(
                "owner_company_profile.icon_invalid",
                "The company profile icon is invalid.",
                new Dictionary<string, string[]> { ["file"] = ["An icon image file is required."] }));
        }

        await using var content = file.OpenReadStream();
        var result = await iconService.UploadAsync(content, cancellationToken);
        return result.IsSuccess ? this.NoContent() : this.ToProblemDetails(result.Error!);
    }

    [HttpGet("icon")]
    public async Task<ActionResult> GetIcon(
        [FromServices] ICompanyProfileIconService iconService,
        CancellationToken cancellationToken)
    {
        var result = await iconService.OpenReadAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            return this.ToProblemDetails(result.Error!);
        }

        this.Response.Headers.XContentTypeOptions = "nosniff";
        return this.File(result.Value!.Content, result.Value.ContentType);
    }

    [HttpDelete("icon")]
    public async Task<ActionResult> ResetIcon(
        [FromServices] ICompanyProfileIconService iconService,
        CancellationToken cancellationToken)
    {
        var result = await iconService.ResetAsync(cancellationToken);
        return result.IsSuccess ? this.NoContent() : this.ToProblemDetails(result.Error!);
    }

    private static OwnerCompanyProfileResponse ToResponse(OwnerCompanyProfileRecord profile) =>
        new(
            profile.Id,
            profile.CompanyName,
            profile.AddressLine1,
            profile.AddressLine2,
            profile.City,
            profile.PostalCode,
            profile.Country,
            profile.TaxId,
            profile.Phone,
            profile.Email,
            profile.Website,
            profile.LogoReference,
            profile.RegistrationNumber);
}
