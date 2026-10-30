namespace Entity_Framework_Practice.Services;

public enum ResultStatus { Success, NotFound, Conflict, Invalid }

public interface IServiceResult
{
    ResultStatus Status { get; }
    string? Error { get; }
}

// For operations that return nothing (delete)
public class ServiceResult : IServiceResult
{
    public ResultStatus Status { get; private init; } = ResultStatus.Success;
    public string? Error { get; private init; }
    public bool IsSuccess => Status == ResultStatus.Success;

    public static ServiceResult Ok() => new();
    public static ServiceResult NotFound(string error) => new() { Status = ResultStatus.NotFound, Error = error };
    public static ServiceResult Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    public static ServiceResult Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };
}

// For operations that return a value (create, update, borrow, return)
public class ServiceResult<T> : IServiceResult
{
    public ResultStatus Status { get; private init; } = ResultStatus.Success;
    public string? Error { get; private init; }
    public T? Value { get; private init; }
    public bool IsSuccess => Status == ResultStatus.Success;

    public static ServiceResult<T> Ok(T value) => new() { Value = value };
    public static ServiceResult<T> NotFound(string error) => new() { Status = ResultStatus.NotFound, Error = error };
    public static ServiceResult<T> Conflict(string error) => new() { Status = ResultStatus.Conflict, Error = error };
    public static ServiceResult<T> Invalid(string error) => new() { Status = ResultStatus.Invalid, Error = error };
}
