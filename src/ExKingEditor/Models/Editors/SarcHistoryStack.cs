using System.Runtime.CompilerServices;

namespace ExKingEditor.Models.Editors;

public enum SarcChange
{
    Import,
    Remove,
    Rename
}

public class SarcHistoryStack
{
    private record HistoryItem(SarcChange Change, List<FileItemNode> Values);

    private readonly Stack<HistoryItem> _undoStack = new();
    private readonly Stack<HistoryItem> _redoStack = new();

    public bool HasChanges => _undoStack.Count > 0;

    public void InvokeLastAction(bool isRedo)
    {
        if (_undoStack.Count <= 0 || isRedo && _redoStack.Count <= 0) {
            return;
        }

        HistoryItem item = _undoStack.Pop();
        Action<HistoryItem, bool>? action = item.Change switch {
            SarcChange.Import => isRedo ? AddAction : RemoveAction,
            SarcChange.Remove => isRedo ? RemoveAction : AddAction,
            _ => null
        };

        action?.Invoke(item, isRedo);
    }

    public void StageChange(SarcChange change, List<FileItemNode> values)
    {
        if (_redoStack.Count > 0) {
            _redoStack.Clear();
        }

        _undoStack.Push(new(change, values));
    }

    private void RemoveAction(HistoryItem item, bool isRedo)
    {
        foreach (var node in item.Values) {
            node.Parent?.Children.Remove(node);
        }

        if (isRedo) {
            _undoStack.Push(item);
        }
        else {
            _redoStack.Push(item);
        }
    }

    private void AddAction(HistoryItem item, bool isRedo)
    {
        foreach (var node in item.Values) {
            node.Parent?.Children.Add(node);
        }

        if (isRedo) {
            _undoStack.Push(item);
        }
        else {
            _redoStack.Push(item);
        }
    }
}
