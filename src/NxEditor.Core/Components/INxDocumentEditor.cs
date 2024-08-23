namespace NxEditor.Core.Components;

public interface INxDocumentEditor
{
    ValueTask Undo();
    ValueTask Redo();
    ValueTask SelectAll();
    ValueTask Cut();
    ValueTask Copy();
    ValueTask Paste();
}
