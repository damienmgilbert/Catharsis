namespace Catharsis.Buffers;

///<summary>
///Represents the result status of a sequence parsing operation.
///</summary>
public enum SequenceParseStatus
{
    ///<summary>
    ///The parse operation completed successfully.
    ///</summary>
    Success,

    ///<summary>
    ///More data is needed to complete the parse operation.
    ///</summary>
    NeedMoreData,

    ///<summary>
    ///The data was malformed and could not be parsed.
    ///</summary>
    InvalidData,

    ///<summary>
    ///The parse operation was cancelled.
    ///</summary>
    Cancelled,

    ///<summary>
    ///The end of the sequence was reached unexpectedly.
    ///</summary>
    UnexpectedEnd
}
