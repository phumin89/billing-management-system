using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Commands;
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

        Assert.True(result.IsValid);
        Assert.Equal("handled", result.Result);
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

        Assert.False(result.IsValid);
        Assert.Null(result.Result);
        Assert.Equal(["Name is required.", "Name is too long."], result.ValidationErrors["Name"]);
        Assert.Equal(["Email format is invalid."], result.ValidationErrors["Email"]);
        Assert.Equal(["validator", "validator"], events);
        Assert.Equal(0, handler.InvocationCount);
    }

    private static ICommandDispatcher CreateDispatcher(
        RecordingHandler handler,
        params RecordingValidator[] validators)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<TestCommand, string>>(handler);
        foreach (var validator in validators)
        {
            services.AddSingleton<ICommandValidator<TestCommand>>(validator);
        }

        return new CommandDispatcher(services.BuildServiceProvider());
    }

    private sealed record TestCommand;

    private sealed class RecordingHandler(List<string> events)
        : ICommandHandler<TestCommand, string>
    {
        public int InvocationCount { get; private set; }

        public Task<string> Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            this.InvocationCount++;
            events.Add("handler");
            return Task.FromResult("handled");
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
