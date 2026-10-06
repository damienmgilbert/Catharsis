using System.Reflection;

namespace Catharsis.ComponentModel;

///<summary>
///Resolves and assigns nested property paths on an object graph by dotted string (e.g. <c>"Address.City"</c>), for
///scenarios like dynamic data binding where the property to access is only known at runtime.
///</summary>
public static class PropertyPathResolver
{
    #region Private methods
    static PropertyInfo GetPropertyOrThrow(object instance, string segment, string path)
    {
        return instance.GetType().GetProperty(segment) ?? throw new ArgumentException($"Property '{segment}' was not found on type '{instance.GetType().Name}'.", nameof(path));
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Resolves the value at the specified property path.
    ///</summary>
    ///<param name="source">The root object to resolve the path against.</param>
    ///<param name="path">A dotted property path, e.g. <c>"Address.City"</c>.</param>
    ///<returns>The resolved value, or <c>null</c> if the path traverses through a <c>null</c> reference.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or names a property that does not exist.</exception>
    public static object? GetValue(object source, string path)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        object? current = source;

        foreach (string segment in path.Split('.'))
        {
            if (current is null)
            {
                return null;
            }

            current = GetPropertyOrThrow(current, segment, path).GetValue(current);
        }

        return current;
    }

    ///<summary>
    ///Assigns a value at the specified property path, traversing every segment but the last.
    ///</summary>
    ///<param name="source">The root object to resolve the path against.</param>
    ///<param name="path">A dotted property path, e.g. <c>"Address.City"</c>.</param>
    ///<param name="value">The value to assign to the final segment.</param>
    ///<exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or names a property that does not exist.</exception>
    ///<exception cref="InvalidOperationException">A segment before the last one resolved to <c>null</c>.</exception>
    public static void SetValue(object source, string path, object? value)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string[] segments = path.Split('.');
        object current = source;

        for (int index = 0; index < (segments.Length - 1); index++)
        {
            object? next = GetPropertyOrThrow(current, segments[index], path).GetValue(current);

            if (next is null)
            {
                throw new InvalidOperationException($"Cannot traverse through a null value at '{segments[index]}'.");
            }

            current = next;
        }

        GetPropertyOrThrow(current, segments[^1], path).SetValue(current, value);
    }
    #endregion
}
