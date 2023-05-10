using Avalonia.Controls.Notifications;
using Dock.Model.Mvvm.Controls;
using ExKingEditor.Dialogs;

namespace ExKingEditor.Models;

public abstract unsafe class ReactiveEditor : Document
{
    public static string CacheDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "ExKingEditor", "ReactiveEditor");

    public static List<ReactiveEditor> OpenEditors { get; } = new();

    protected readonly string _file;
    protected readonly string _temp;
    protected bool _compressed;
    protected readonly Stream _stream;
    protected readonly byte[] _data;

    public string FilePath => _file;

    public ReactiveEditor(string file, byte[] data, Stream? fs = null)
    {
        Id = file;
        Title = Path.GetFileName(file);

        _file = file;
        _data = data;
        _stream = fs ?? new MemoryStream(_data);

        // Create temp directory
        _temp = Directory.CreateDirectory(
            Path.Combine(CacheDirectory, Path.GetFileName(file), Guid.NewGuid().ToString())).FullName;

        // Cache the open editor
        OpenEditors.Add(this);
    }

    public override bool OnClose()
    {
        if (HasChanged() && new ContentDialog {
            Title = "Warning",
            Content = "You have unsaved changes.\nAre you sure you wish to exit?",
            PrimaryButtonContent = "Yes"
        }.ShowDialog() == ContentDialogResult.Secondary) {
            return false;
        }

        Dispose();
        return true;
    }

    public virtual void Save() => SaveAs(_file);
    public abstract void SaveAs(string path);
    public abstract bool HasChanged();

    public virtual void Undo() { }
    public virtual void Redo() { }

    public virtual void SelectAll() { }
    public virtual Task Cut() => Task.CompletedTask;
    public virtual Task Copy() => Task.CompletedTask;
    public virtual Task Paste() => Task.CompletedTask;

    public virtual void Find() { }
    public virtual Task FindAndReplace() => Task.CompletedTask;

    protected void ToastSaveSuccess(string path)
    {
        App.Toast(
            $"Saved {(_compressed ? "and compressed " : "")}{Path.GetFileName(path)}", "Success",
            NotificationType.Success, TimeSpan.FromSeconds(1.2));
    }

    public void Dispose()
    {
        OpenEditors.Remove(this);
        Directory.Delete(_temp, true);
        _stream.Close();
    }
}
