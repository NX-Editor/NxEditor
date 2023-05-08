using Avalonia.Controls.Notifications;
using Dock.Model.Mvvm.Controls;
using ExKingEditor.Core;
using ExKingEditor.Dialogs;

namespace ExKingEditor.Models;

public abstract unsafe class ReactiveEditor : Document
{
    protected readonly string _file;
    protected readonly string _temp;
    protected bool _compressed;
    protected readonly FileStream _stream;

    private readonly byte* _data;
    private readonly int _length;

    public string FilePath => _file;

    public ReactiveEditor(string file)
    {
        Id = file;
        Title = Path.GetFileName(file);

        _file = file;
        _compressed = file.EndsWith(".zs");

        // Create temp directory
        _temp = Directory.CreateDirectory(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", Path.GetFileName(file), Guid.NewGuid().ToString())
        ).FullName;

        // keep the file open until the
        // tab closes
        _stream = File.Open(file, FileMode.Open);
        Span<byte> raw = new byte[_stream.Length];
        _stream.Read(raw);

        // Decompress if necessary
        Span<byte> data = _compressed ? TotkZstd.Decompress(file, raw) : raw;
        _length = data.Length;

        // Store a ptr on the heap to
        // avoid copying to an array
        fixed (byte* ptr = data) {
            _data = ptr;
        }
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

        Directory.Delete(_temp, true);
        _stream.Close();
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

    protected Span<byte> RawData()
    {
        return new(_data, _length);
    }

    protected void ToastSaveSuccess(string path)
    {
        App.Toast(
            $"Saved {(_compressed ? "and compressed " : "")}{Path.GetFileName(path)}", "Success",
            NotificationType.Success, TimeSpan.FromSeconds(1.2));
    }
}
