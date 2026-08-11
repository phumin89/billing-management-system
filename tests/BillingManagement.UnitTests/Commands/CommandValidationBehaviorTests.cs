using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Handlers.Validation;
using BillingManagement.Application.Validation;

namespace BillingManagement.UnitTests.Commands;

public sealed class CommandValidationBehaviorTests
{
    [Fact]
    public async Task Handle_runs_validators_before_handler_and_invokes_handler_once()
    {
        var events = new List<string>();
        var behavior = CreateBehavior(events, new Dictionary<string, string[]>());
        var invocationCount = 0;

        var result = await behavior.Handle(
            new TestCommand(),
            (command, cancellationToken) =>
            {
                invocationCount++;
                events.Add("handler");
                return ValueTask.FromResult(CommandResult.Succeeded());
            },
            default);

        Assert.True(result.Success);
        Assert.Equal(["validator", "handler"], events);
        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public async Task Handle_aggregates_errors_and_does_not_invoke_handler()
    {
        var events = new List<string>();
        var behavior = new CommandValidationBehavior<TestCommand, CommandResult>(
        [
            new AnnotationCommandValidator<TestCommand>(),
            new RecordingValidator(events, new Dictionary<string, string[]>
            {
                ["Name"] = ["Name is too long."],
                ["Email"] = ["Email format is invalid."]
            })
        ]);
        var invocationCount = 0;

        var result = await behavior.Handle(
            new TestCommand(" "),
            (command, cancellationToken) =>
            {
                invocationCount++;
                return ValueTask.FromResult(CommandResult.Succeeded());
            },
            default);

        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        Assert.Equal(CommandErrorType.Validation, error.Key);
        Assert.Equal(
            ["Name is required.", "Name is too long.", "Email format is invalid."],
            error.Value);
        Assert.Equal(["validator"], events);
        Assert.Equal(0, invocationCount);
    }

    private static CommandValidationBehavior<TestCommand, CommandResult> CreateBehavior(
        List<string> events,
        IReadOnlyDictionary<string, string[]> errors) =>
        new([new RecordingValidator(events, errors)]);

    private sealed record TestCommand(
        [property: RequiredText("Name is required.")]
        string Name = "Valid") : ICommand;

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
