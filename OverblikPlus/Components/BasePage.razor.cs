using Microsoft.AspNetCore.Components;

namespace OverblikPlus.Components;

public abstract class BasePage : ComponentBase
{
    [Inject] protected ILogger<BasePage> Logger { get; set; } = default!;

    protected bool IsLoading { get; set; } = false;
    protected string? ErrorMessage { get; set; }
    protected string? SuccessMessage { get; set; }

    protected void SetLoading(bool loading)
    {
        IsLoading = loading;
        StateHasChanged();
    }

    protected void SetError(string? message)
    {
        ErrorMessage = message;
        SuccessMessage = null;
        StateHasChanged();
    }

    protected void SetSuccess(string? message)
    {
        SuccessMessage = message;
        ErrorMessage = null;
        StateHasChanged();
    }

    protected void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
        StateHasChanged();
    }

    protected void LogError(string message, Exception? ex = null)
    {
        if (ex != null)
        {
            Logger.LogError(ex, message);
        }
        else
        {
            Logger.LogError(message);
        }
    }

    protected void LogWarning(string message)
    {
        Logger.LogWarning(message);
    }

    protected void LogInfo(string message)
    {
        Logger.LogInformation(message);
    }
}

