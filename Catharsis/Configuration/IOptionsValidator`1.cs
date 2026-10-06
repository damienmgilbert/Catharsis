namespace Catharsis.Configuration;

///<summary>
///Validates an options instance, mirroring the shape of
public interface IOptionsValidator<in TOptions>
{
    #region Public methods

    ///<summary>
    ///Validates the specified options instance.
    ///</summary>
    ///<param name="options">The options instance to validate.</param>
    ///<returns>The validation outcome.</returns>
    OptionsValidationResult Validate(TOptions options);
    #endregion
}
