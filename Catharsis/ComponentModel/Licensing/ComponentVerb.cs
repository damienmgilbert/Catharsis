namespace Catharsis.ComponentModel.Licensing;

///<summary>
///Represents a named action (verb) that can be offered by a component designer at design time, typically displayed in a
///context menu or smart tag panel.
///</summary>
///<param name="Text">The display text of the verb.</param>
///<param name="Action">The action to execute when the verb is invoked.</param>
///<param name="Description">An optional description of the verb.</param>
///<param name="Enabled">
///Whether the verb is currently enabled. Defaults to <c>true</c>.
///</param>
public sealed record ComponentVerb(string Text, Action Action, string? Description = null, bool Enabled = true)
{
    #region Public methods

    ///<summary>
    ///Invokes the verb action if the verb is enabled.
    ///</summary>
    ///<exception cref="InvalidOperationException">
    ///The verb is not enabled.
    ///</exception>
    public void Invoke()
    {
        if(!Enabled)
        {
            throw new InvalidOperationException($"The verb '{Text}' is not currently enabled.");
        }

        Action();
    }
    #endregion
}
