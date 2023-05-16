using ExKingEditor.ViewModels.Editors;

namespace ExKingEditor.Models.Editors;

public enum SarcChange
{
    Import,
    Remove,
    Rename
}

public class SarcHistoryStack
{
    private record HistoryItem(SarcChange Change, List<(FileItemNode node, object? data)> Values);

    private readonly SarcViewModel _editor;
    private readonly Stack<HistoryItem> _undoStack = new();
    private readonly Stack<HistoryItem> _redoStack = new();

    public SarcHistoryStack(SarcViewModel editor)
    {
        _editor = editor;
    }

    public bool HasChanges => _undoStack.Count > 0;

    public void InvokeLastAction(bool isRedo)
    {
        if (isRedo && _redoStack.Count <= 0 || !isRedo && _undoStack.Count <= 0) {
            return;
        }

        HistoryItem item = (isRedo ? _redoStack : _undoStack).Pop();
        Action<HistoryItem, bool>? action = item.Change switch {
            SarcChange.Import => isRedo ? AddAction : RemoveAction,
            SarcChange.Remove => isRedo ? RemoveAction : AddAction,
            SarcChange.Rename => RenameAction,
            _ => null
        };

        action?.Invoke(item, isRedo);
    }

    public void StageChange(SarcChange change, List<(FileItemNode, object?)> values)
    {
        if (_redoStack.Count > 0) {
            _redoStack.Clear();
        }

        _undoStack.Push(new(change, values));
    }

    private void RemoveAction(HistoryItem item, bool isRedo)
    {
        foreach ((var node, _) in item.Values) {
            (node.Parent ?? _editor.Root).Children.Remove(node);
            _editor.RemoveNodeFromMap(node);
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
        foreach ((var node, _) in item.Values) {
            (node.Parent ?? _editor.Root).Children.Add(node);
            _editor.AddNodeToTree(node);
        }

        if (isRedo) {
            _undoStack.Push(item);
        }
        else {
            _redoStack.Push(item);
        }
    }

    private void RenameAction(HistoryItem item, bool isRedo)
    {
        for (int i = 0; i < item.Values.Count; i++) {
            (var node, var data) = item.Values[i];
            node.PrevName = node.Header;
            item.Values[i] = (node, node.PrevName);
            node.Header = (data as string) ?? node.Header;
            _editor.RenameMapNode(node);
        }

        if (isRedo) {
            _undoStack.Push(item);
        }
        else {
            _redoStack.Push(item);
        }
    }
}
