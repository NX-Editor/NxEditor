using System.Runtime.CompilerServices;

namespace ExKingEditor.Core.Extensions;

public static class SafeTaskExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task SafeInvoke(this Task? task)
    {
        try {
            if (task is Task _task) {
                await _task;
            }
        }
        catch (Exception ex) {
            Logger.Write(ex);
        }
    }
}
