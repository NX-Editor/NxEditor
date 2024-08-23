using System.Collections;

namespace NxEditor.Core.Components;

public interface IMenuFactory
{
    IEnumerable Items { get; }

    /// <summary>
    /// Add a new menu group from a menu model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="groupId"></param>
    /// <param name="source"></param>
    void AddMenuGroup<T>(string groupId, object? source = null);

    /// <summary>
    /// Remove an existing menu group.
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns><see langword="true"/> is the group was found, <see langword="false"/> is the <paramref name="groupId"/> could not be found.</returns>
    bool RemoveMenuGroup(string groupId);
}
