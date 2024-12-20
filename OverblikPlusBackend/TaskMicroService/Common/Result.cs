namespace TaskMicroService.Common;

public class Result<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }

    public static Result<T> SuccessResult(T data)
    {
        return new Result<T> { Success = true, Data = data };
    }

    public static Result<T> ErrorResult(string error)
    {
        return new Result<T> { Success = false, Error = error };
    }
}

public class Result
{
    public bool Success { get; set; }
    public string Error { get; set; }

    public static Result SuccessResult()
    {
        return new Result { Success = true };
    }

    public static Result ErrorResult(string error)
    {
        return new Result { Success = false, Error = error };
    }
}