using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
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
        var dispatchResult = await commandDispatcher.Send<CreateOwnerCompanyProfileCommand, CreateOwnerCompanyProfileResult>(
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

        if (!dispatchResult.IsValid)
        {
            this.ModelState.AddErrors(dispatchResult.ValidationErrors);
            return this.ValidationProblem(this.ModelState);
        }

        var result = dispatchResult.Result!;

        if (!result.Succeeded)
        {
            this.ModelState.AddErrors(result.Errors);
            return this.ValidationProblem(this.ModelState);
        }

        return this.CreatedAtAction(nameof(Get), ToResponse(result.Profile!));
    }

    [HttpPut]
    public async Task<ActionResult<OwnerCompanyProfileResponse>> Update(
        UpdateOwnerCompanyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var dispatchResult = await commandDispatcher.Send<UpdateOwnerCompanyProfileCommand, UpdateOwnerCompanyProfileResult>(
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

        if (!dispatchResult.IsValid)
        {
            this.ModelState.AddErrors(dispatchResult.ValidationErrors);
            return this.ValidationProblem(this.ModelState);
        }

        var result = dispatchResult.Result!;

        if (result.NotFound)
        {
            return this.NotFound();
        }

        if (!result.Succeeded)
        {
            this.ModelState.AddErrors(result.Errors);
            return this.ValidationProblem(this.ModelState);
        }

        return this.Ok(ToResponse(result.Profile!));
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
