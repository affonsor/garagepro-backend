namespace GaragePro.Application.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public IEnumerable<string> Errors { get; }
    public ResultStatus Status { get; }

    private Result(ResultStatus status, T? value = default, string? error = null, IEnumerable<string>? errors = null)
    {
        Status = status;
        Value = value;
        Error = error;
        Errors = errors ?? [];
        IsSuccess = status == ResultStatus.Success;
    }

    public static Result<T> Success(T value) => new(ResultStatus.Success, value);
    public static Result<T> Failure(string error) => new(ResultStatus.Failure, error: error);
    public static Result<T> Conflict(string error) => new(ResultStatus.Conflict, error: error);
    public static Result<T> ValidationFailure(IEnumerable<string> errors) => new(ResultStatus.ValidationFailure, errors: errors);
    public static Result<T> NotFound(string message) => new(ResultStatus.NotFound, error: message);
}

public enum ResultStatus
{
    Success,
    Failure,
    Conflict,
    ValidationFailure,
    NotFound
}
