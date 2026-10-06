using System.Threading.Tasks.Dataflow;

namespace Catharsis.Concurrency;

///<summary>
///Starts a fluent <see cref="DataflowPipelineBuilder{TInput,TCurrent}"/> chain, beginning with a ///<see
///cref="BufferBlock{T}"/> as the pipeline's head.
///</summary>
public static class DataflowPipelineBuilder
{
    #region Public methods

    ///<summary>
    ///Creates a new pipeline builder whose head is a <see cref="BufferBlock{T}"/> of <typeparamref name="T"/>.
    ///</summary>
    ///<typeparam name="T">The pipeline's input type.</typeparam>
    ///<param name="options">Options for the head block. Defaults to <see cref="DataflowBlockOptions"/> if not specified.</param>
    public static DataflowPipelineBuilder<T, T> Create<T>(DataflowBlockOptions? options = null)
    {
        BufferBlock<T> buffer = new(options ?? new DataflowBlockOptions());
        return new DataflowPipelineBuilder<T, T>(buffer, buffer);
    }
    #endregion
}
