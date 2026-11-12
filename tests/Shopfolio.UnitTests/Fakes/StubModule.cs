using System.Reflection;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>A stand-in for a module surface: the methods a test names answer with the given value (wrapped in a completed task when the method returns one); any other call fails the test.</summary>
internal sealed class StubModule<T>
    where T : class
{
    private readonly Dictionary<string, Func<object?[], object?>> _answers = [];

    public StubModule()
    {
        Object = DispatchProxy.Create<T, StubProxy>();
        ((StubProxy)(object)Object).Answers = _answers;
    }

    public T Object { get; }

    public void On(string method, Func<object?[], object?> answer) => _answers[method] = answer;
}

/// <summary>The proxy behind <see cref="StubModule{T}"/>; a proxy base type must be unsealed and visible, so it stands apart from the generic stub.</summary>
public class StubProxy : DispatchProxy
{
    internal Dictionary<string, Func<object?[], object?>> Answers { get; set; } = [];

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        if (!Answers.TryGetValue(targetMethod.Name, out Func<object?[], object?>? answer))
        {
            throw new InvalidOperationException($"The stub has no answer for {targetMethod.Name}.");
        }

        object? value = answer(args ?? []);
        Type returned = targetMethod.ReturnType;
        if (returned.IsGenericType && returned.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(returned.GetGenericArguments()[0]).Invoke(null, [value]);
        }

        return value;
    }
}
