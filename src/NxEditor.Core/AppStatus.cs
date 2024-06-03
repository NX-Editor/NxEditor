using CommunityToolkit.Mvvm.ComponentModel;

namespace NxEditor.Core;

public enum StatusType
{
    Static,
    Working
}

public partial class AppStatus : ObservableObject
{
    private const string DOT = ".";
    private readonly Timer _timer;

    [ObservableProperty]
    private string _status = StatusMsg.Ready;

    [ObservableProperty]
    private string _suffix = string.Empty;

    [ObservableProperty]
    private string _icon = Icons.READY;

    [ObservableProperty]
    private StatusType _type = StatusType.Static;

    public AppStatus()
    {
        _timer = new(UpdateLoadingStatus);
        _timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(0.3));
    }

    /// <summary>
    /// Reset the status.
    /// </summary>
    public void Reset()
    {
        Set(StatusMsg.Ready, Icons.READY, StatusType.Static);
    }

    /// <summary>
    /// Set a temporary status for 1.5 seconds.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    public void SetTemporaryShort(string status, string icon = Icons.READY, StatusType type = StatusType.Static)
    {
        Set(status, icon, type, duration: 1.5);
    }

    /// <summary>
    /// Set a temporary status for 3 seconds.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    public void SetTemporaryLong(string status, string icon = Icons.READY, StatusType type = StatusType.Static)
    {
        Set(status, icon, type, duration: 3);
    }

    /// <summary>
    /// Set a static or working status.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    /// <param name="type"></param>
    public void Set(string status, string icon = Icons.READY, StatusType type = StatusType.Static)
    {
        Status = status;
        Icon = icon;
        Type = type;
    }

    /// <summary>
    /// Set a temporary static or working status.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="icon"></param>
    /// <param name="type"></param>
    /// <param name="duration">The duration of the status in seconds</param>
    public void Set(string status, string icon = Icons.READY, StatusType type = StatusType.Static, double duration = double.NaN)
    {
        Set(status, icon, type);
        _ = Task.Run(async () => {
            await Task.Delay(TimeSpan.FromSeconds(duration));

            if (Status == status) {
                Reset();
            }
        });
    }

    private void UpdateLoadingStatus(object? _)
    {
        if (Type is StatusType.Static) {
            return;
        }

        Suffix = Suffix.Length switch {
            4 => DOT,
            _ => Suffix + DOT
        };
    }

    partial void OnTypeChanged(StatusType value)
    {
        switch (value) {
            case StatusType.Static:
                Suffix = string.Empty;
                break;
        }
    }
}
