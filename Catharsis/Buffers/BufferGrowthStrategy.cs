namespace Catharsis.Buffers;

///<summary>
///Specifies the strategy used to grow a buffer when additional capacity is needed.
///</summary>
public enum BufferGrowthStrategy
{
    ///<summary>
    ///Doubles the current capacity on each expansion.
    ///</summary>
    Doubling,

    ///<summary>
    ///Increases capacity by a fixed increment.
    ///</summary>
    Linear,

    ///<summary>
    ///Increases capacity by the exact amount needed.
    ///</summary>
    Exact,

    ///<summary>
    ///Uses a factor of 1.5× to grow the buffer.
    ///</summary>
    OnePointFive
}
