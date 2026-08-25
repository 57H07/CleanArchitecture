using System.Text.Json;

namespace CleanArchitecture.Web.Models;

public class ToastMessage
{
    public string Header { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public const string SuccessKey = "ToastSuccess";
    public const string ErrorKey = "ToastError";
    public string Serialize() => JsonSerializer.Serialize(this);

    public static ToastMessage? Deserialize(object? value)
    {
        if (value is not string raw || string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ToastMessage>(raw);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
