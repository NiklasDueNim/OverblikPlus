namespace OverblikPlus.Common;

public class Result<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }

    public static Result<T> SuccessResult(T data) => new() { Success = true, Data = data };

    public static Result<T> ErrorResult(string error) => new() { Success = false, Error = error };
}

public class Result
{
    public bool Success { get; set; }
    public string Error { get; set; }

    public static Result SuccessResult() => new() { Success = true };

    public static Result ErrorResult(string error) => new() { Success = false, Error = error };
}