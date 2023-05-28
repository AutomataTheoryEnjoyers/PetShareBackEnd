using System.Diagnostics.CodeAnalysis;

namespace PetShare.Results;

public class Result
{
    public Result()
    {
        HasValue = true;
    }

    public Result(ResultState state)
    {
        State = state;
        HasValue = false;
    }

    [MemberNotNullWhen(false, nameof(State))]
    public virtual bool HasValue { get; }

    public ResultState? State { get; }

    public static Result Ok => new();

    public static implicit operator Result(ResultState state)
    {
        return new Result(state);
    }
}

public sealed class Result<T> : Result
{
    public Result(T value)
    {
        Value = value;
    }

    public Result(ResultState state) : base(state) { }

    [MemberNotNullWhen(true, nameof(Value))]
    public override bool HasValue => base.HasValue;

    public T? Value { get; }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(ResultState state)
    {
        return new Result<T>(state);
    }
}
