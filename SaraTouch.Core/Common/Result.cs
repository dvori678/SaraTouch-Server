using SaraTouch.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public ErrorType ErrorType { get; }
        public string? Error { get; }

        protected Result(bool isSuccess, ErrorType errorType, string? error)
        {
            IsSuccess = isSuccess;
            ErrorType = errorType;
            Error = error;
        }
        public static Result Success() => new(true, ErrorType.None, null);
        public static Result Failure(ErrorType type, string error) => new(false, type, error);

        public static Result Validation(string error) => Failure(ErrorType.Validation, error);
        public static Result Unauthorized(string error) => Failure(ErrorType.Unauthorized, error);
        public static Result Forbidden(string error) => Failure(ErrorType.Forbidden, error);
        public static Result NotFound(string error) => Failure(ErrorType.NotFound, error);
        public static Result Conflict(string error) => Failure(ErrorType.Conflict, error);

    }
}
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, ErrorType.None, null) => Value = value;
    private Result(ErrorType type, string error) : base(false, type, error) { }

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(ErrorType type, string error) => new(type, error);

    public static new Result<T> Validation(string error) => new(ErrorType.Validation, error);
    public static new Result<T> Unauthorized(string error) => new(ErrorType.Unauthorized, error);
    public static new Result<T> Forbidden(string error) => new(ErrorType.Forbidden, error);
    public static new Result<T> NotFound(string error) => new(ErrorType.NotFound, error);
    public static new Result<T> Conflict(string error) => new(ErrorType.Conflict, error);
}
