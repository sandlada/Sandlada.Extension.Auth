namespace Sandlada.Extension.Auth.Domain.Commons;

public interface IResult {
    DomainError Error { get; }
    bool IsSuccess { get; }
    bool IsFailure { get; }
}

public interface IResult<out T> : IResult {
    T Value { get; }
}

public class Result : IResult {
    public DomainError Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !this.IsSuccess;

    protected Result(bool isSuccess, DomainError error) {
        if (isSuccess && error != DomainError.General.None || !isSuccess && error == DomainError.General.None) {
            throw new ArgumentException("Invalid error combination.", nameof(error));
        }
        this.IsSuccess = isSuccess;
        this.Error = error;
    }

    public static Result Success() => new(true, DomainError.General.None);
    public static Result Failure(DomainError error) => new(false, error);

    public static IResult<T> Success<T>(T value) => new Result<T>(value, true, DomainError.General.None);
    public static IResult<T> Failure<T>(DomainError error) => new Result<T>(default, false, error);
};

public class Result<TValue> : Result, IResult<TValue> {
    private readonly TValue? _value;

    public TValue Value => this.IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    protected internal Result(TValue? value, bool isSuccess, DomainError error) : base(isSuccess, error) {
        _value = value;
    }
}
