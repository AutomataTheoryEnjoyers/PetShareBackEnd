using System.Diagnostics.CodeAnalysis;

namespace ShelterModule.Results;

public sealed class Result<T>
{
    public Result(T value)
    {
        Value = value;
        HasValue = true;
    }

    public Result(ResultState state)
    {
        State = state;
        HasValue = false;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(State))]
    public bool HasValue { get; }

    public T? Value { get; }

    public ResultState? State { get; }

    public static implicit operator Result<T>(T value)
    {
        return new(value);
    }

    public static implicit operator Result<T>(ResultState state)
    {
        return new(state);
    }
}
