namespace NxEditor;

public class NxeFrontend : INxeFrontend
{
    public async ValueTask<T?> PickOneAsync<T>(IAsyncEnumerable<T> values)
    {
        T[] valueArray = await values.ToArrayAsync();
        
        if (valueArray.Length == 0) {
            return default;
        }
        
        if (valueArray.Length == 1) {
            return valueArray[0];
        }

        // TODO: Picker dialog
        return valueArray[0];
    }
}
