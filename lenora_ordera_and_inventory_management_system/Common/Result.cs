namespace lenora_ordera_and_inventory_management_system.Common;

internal sealed class Result
{
    private Result(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }

    public string Message { get; }

    public static Result Ok(string message)
    {
        return new Result(true, message);
    }

    public static Result Fail(string message)
    {
        return new Result(false, message);
    }
}
