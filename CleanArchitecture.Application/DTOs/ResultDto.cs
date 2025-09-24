namespace CleanArchitecture.Application.DTOs;

public class ResultDto<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public ResultDto(T data, bool isSuccess = true)
    {
        Data = data;
        IsSuccess = isSuccess;
    }

    public ResultDto(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}
