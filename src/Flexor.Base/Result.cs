namespace Flexor;

public record Result<TResult>(bool IsSuccess, TResult? Value, string? ErrorMessage)
{
    public static Result<TResult?> Success(TResult? value) => new(true, value, null);
    public static Result<TResult?> Failure(string errorMessage) => new(false, default, errorMessage);

    public static Result<TResult?> From(Func<TResult?> func)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure(ex.Message);
        }

    }
}