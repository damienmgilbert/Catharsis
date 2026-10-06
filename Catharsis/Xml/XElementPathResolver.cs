using System.Xml.Linq;

namespace Catharsis.Xml;

///<summary>
///Resolves and assigns a value by a simplified, dotted-slash path over an <see cref="XElement"/> tree (e.g.
///<c>"Customer/@id"</c> for the <c>id</c> attribute of a <c>Customer</c> child element, or
///<c>"Customer/Name"</c> for that child element's text). The XML counterpart to
///<see cref="Catharsis.ComponentModel.PropertyPathResolver"/>.
///</summary>
public static class XElementPathResolver
{
    #region Private methods
    static string[] SplitPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return path.Split('/');
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Resolves the value at the specified path, relative to <paramref name="root"/>'s children.
    ///</summary>
    ///<param name="root">The element to resolve the path against.</param>
    ///<param name="path">
    ///A slash-separated path of child element names, with an optional trailing <c>@attributeName</c> segment to
    ///select an attribute instead of an element's text.
    ///</param>
    ///<returns>The resolved text, or <c>null</c> if the path traverses through a missing element or attribute.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace, or an <c>@attribute</c> segment appears before the last segment.</exception>
    public static string? GetValue(XElement root, string path)
    {
        ArgumentNullException.ThrowIfNull(root);

        string[] segments = SplitPath(path);
        XElement? current = root;

        for(int index = 0; index < segments.Length; index++)
        {
            string segment = segments[index];
            bool isLastSegment = index == (segments.Length - 1);

            if(segment.StartsWith('@'))
            {
                if(!isLastSegment)
                {
                    throw new ArgumentException("An attribute segment ('@name') must be the last segment in the path.", nameof(path));
                }

                return current?.Attribute(segment[1..])?.Value;
            }

            if(current is null)
            {
                return null;
            }

            current = current.Element(segment);
        }

        return current?.Value;
    }

    ///<summary>
    ///Assigns a value at the specified path, relative to <paramref name="root"/>'s children, creating any missing
    ///intermediate elements along the way.
    ///</summary>
    ///<param name="root">The element to resolve the path against.</param>
    ///<param name="path">
    ///A slash-separated path of child element names, with an optional trailing <c>@attributeName</c> segment to
    ///set an attribute instead of an element's text.
    ///</param>
    ///<param name="value">The value to assign.</param>
    ///<exception cref="ArgumentNullException"><paramref name="root"/> is <c>null</c>.</exception>
    ///<exception cref="ArgumentException"><paramref name="path"/> is <c>null</c>, empty, or whitespace, or an <c>@attribute</c> segment appears before the last segment.</exception>
    public static void SetValue(XElement root, string path, string? value)
    {
        ArgumentNullException.ThrowIfNull(root);

        string[] segments = SplitPath(path);
        XElement current = root;

        for(int index = 0; index < (segments.Length - 1); index++)
        {
            string segment = segments[index];

            if(segment.StartsWith('@'))
            {
                throw new ArgumentException("An attribute segment ('@name') must be the last segment in the path.", nameof(path));
            }

            XElement? child = current.Element(segment);

            if(child is null)
            {
                child = new XElement(segment);
                current.Add(child);
            }

            current = child;
        }

        string lastSegment = segments[^1];

        if(lastSegment.StartsWith('@'))
        {
            current.SetAttributeValue(lastSegment[1..], value);
            return;
        }

        XElement? existingChild = current.Element(lastSegment);

        if(existingChild is null)
        {
            current.Add(new XElement(lastSegment, value));
        } else
        {
            existingChild.Value = value ?? string.Empty;
        }
    }
    #endregion
}
