using System.Net;
using System.Reflection;
using BillingManagement.Client.OwnerCompanyProfiles;
using BillingManagement.Client.Pages.CompanyProfile;
using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class CompanyProfileDeleteTests
{
    [Fact]
    public async Task Confirm_delete_blocks_duplicate_requests_while_deleting()
    {
        var response = new TaskCompletionSource<HttpResponseMessage>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var component = CreateComponent(_ =>
        {
            Interlocked.Increment(ref requestCount);
            return response.Task;
        });

        var firstDelete = ConfirmDelete(component);
        var duplicateDelete = ConfirmDelete(component);

        Assert.Equal(1, requestCount);
        Assert.True(Field<bool>(component, "isDeleting"));

        response.SetResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        await Task.WhenAll(firstDelete, duplicateDelete);

        Assert.False(Field<bool>(component, "isDeleting"));
        Assert.Equal("Empty", ReviewState(component));
        Assert.Null(Field<OwnerCompanyProfileResponse?>(component, "profile"));
    }

    [Theory]
    [InlineData(
        HttpStatusCode.Conflict,
        "Company profile is used by quotations or invoices and cannot be deleted.")]
    [InlineData(
        HttpStatusCode.InternalServerError,
        "Could not delete company profile. Try again.")]
    public async Task Confirm_delete_preserves_profile_when_delete_fails(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        var profile = ExistingProfile();
        var component = CreateComponent(
            _ => Task.FromResult(new HttpResponseMessage(statusCode)),
            profile);

        await ConfirmDelete(component);

        Assert.Equal("Existing", ReviewState(component));
        Assert.Same(profile, Field<OwnerCompanyProfileResponse?>(component, "profile"));
        Assert.Equal(expectedMessage, Field<string?>(component, "statusMessage"));
        Assert.False(Field<bool>(component, "showDeleteSnackbar"));
    }

    private static CompanyProfile CreateComponent(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync,
        OwnerCompanyProfileResponse? profile = null)
    {
        profile ??= ExistingProfile();
        var component = new CompanyProfile();
        var client = new OwnerCompanyProfileClient(new HttpClient(new StubHttpMessageHandler(sendAsync))
        {
            BaseAddress = new Uri("http://localhost")
        });

        Property(component, "Client").SetValue(component, client);
        SetField(component, "profile", profile);
        SetField(component, "reviewState", "Existing");
        SetField(component, "showDeleteSnackbar", true);
        return component;
    }

    private static Task ConfirmDelete(CompanyProfile component) =>
        (Task)Method(component, "ConfirmDelete").Invoke(component, null)!;

    private static string ReviewState(CompanyProfile component) =>
        FieldValue(component, "reviewState")!.ToString()!;

    private static T Field<T>(CompanyProfile component, string name) =>
        (T)FieldValue(component, name)!;

    private static object? FieldValue(CompanyProfile component, string name) =>
        FieldInfo(component, name).GetValue(component);

    private static void SetField(CompanyProfile component, string name, object? value)
    {
        var field = FieldInfo(component, name);
        field.SetValue(component, field.FieldType.IsEnum ? Enum.Parse(field.FieldType, (string)value!) : value);
    }

    private static FieldInfo FieldInfo(CompanyProfile component, string name) =>
        component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static PropertyInfo Property(CompanyProfile component, string name) =>
        component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static MethodInfo Method(CompanyProfile component, string name) =>
        component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static OwnerCompanyProfileResponse ExistingProfile() =>
        new(
            Guid.NewGuid(),
            "Acme Operations Co., Ltd.",
            "99 Rama IX Road",
            null,
            "Bangkok",
            "10310",
            "Thailand",
            null,
            null,
            null,
            null,
            null,
            null);

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            sendAsync(request);
    }
}
