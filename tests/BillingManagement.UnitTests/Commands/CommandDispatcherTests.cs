using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Commands;
using BillingManagement.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.UnitTests.Commands;

public sealed class CommandDispatcherTests
{
    [Fact]
    public async Task Send_runs_validators_before_handler_and_invokes_handler_once()
    {
        var events = new List<string>();
        var handler = new RecordingHandler(events);
        var dispatcher = CreateDispatcher(
            handler,
            new RecordingValidator(events, new Dictionary<string, string[]>()));

        var result = await dispatcher.Send<TestCommand, string>(new TestCommand());

        Assert.True(result.IsSuccess);
        Assert.Equal("handled", result.Value);
        Assert.Null(result.Error);
        Assert.Equal(["validator", "handler"], events);
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task Send_aggregates_errors_and_does_not_invoke_handler()
    {
        var events = new List<string>();
        var handler = new RecordingHandler(events);
        var dispatcher = CreateDispatcher(
            handler,
            new RecordingValidator(events, new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is required."]
            }),
            new RecordingValidator(events, new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is too long."],
                ["Email"] = ["Email format is invalid."]
            }));

        var result = await dispatcher.Send<TestCommand, string>(new TestCommand());

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotNull(result.Error);
        Assert.Equal(ApplicationErrorKind.Validation, result.Error.Kind);
        Assert.Equal("validation_failed", result.Error.Code);
        Assert.NotNull(result.Error.ValidationErrors);
        Assert.Equal(["Name", "Email"], result.Error.ValidationErrors.Keys);
        Assert.Equal(["Name is required.", "Name is too long."], result.Error.ValidationErrors["Name"]);
        Assert.Equal(["Email format is invalid."], result.Error.ValidationErrors["Email"]);
        Assert.Equal(["validator", "validator"], events);
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task Send_aggregates_annotation_and_explicit_errors_before_handler()
    {
        var events = new List<string>();
        var handler = new RecordingHandler(events);
        var dispatcher = CreateDispatcher(
            handler,
            new RecordingValidator(events, new Dictionary<string, string[]>
            {
                ["Name"] = ["Names must differ."]
            }));

        var result = await dispatcher.Send<TestCommand, string>(new TestCommand(" "));

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.ValidationErrors);
        Assert.Equal(
            ["Name is required.", "Names must differ."],
            result.Error.ValidationErrors["Name"]);
        Assert.Equal(["validator"], events);
        Assert.Equal(0, handler.InvocationCount);
    }

    private static ICommandDispatcher CreateDispatcher(
        RecordingHandler handler,
        params RecordingValidator[] validators)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<TestCommand, string>>(handler);
        services.AddSingleton<ICommandValidator<TestCommand>, AnnotationCommandValidator<TestCommand>>();
        foreach (var validator in validators)
        {
            services.AddSingleton<ICommandValidator<TestCommand>>(validator);
        }

        return new CommandDispatcher(services.BuildServiceProvider());
    }

    private sealed record TestCommand(
        [property: RequiredText("Name is required.")]
        string Name = "Valid");

    private sealed class RecordingHandler(List<string> events)
        : ICommandHandler<TestCommand, string>
    {
        public int InvocationCount { get; private set; }

        public Task<ApplicationResult<string>> Handle(
            TestCommand command,
            CancellationToken cancellationToken = default)
        {
            this.InvocationCount++;
            events.Add("handler");
            return Task.FromResult(ApplicationResult<string>.Success("handled"));
        }
    }

    private sealed class RecordingValidator(
        List<string> events,
        IReadOnlyDictionary<string, string[]> errors)
        : ICommandValidator<TestCommand>
    {
        public IReadOnlyDictionary<string, string[]> Validate(TestCommand command)
        {
            events.Add("validator");
            return errors;
        }
    }
}
