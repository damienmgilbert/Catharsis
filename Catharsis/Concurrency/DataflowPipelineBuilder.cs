using System.Threading.Tasks.Dataflow;

namespace Catharsis.Concurrency;

///<summary>
///Starts a fluent <see cref="DataflowPipelineBuilder{TInput,TCurrent}"/> chain, beginning with a
///<see cref="BufferBlock{T}"/> as the pipeline's head.
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

///<summary>
///A fluent builder chaining dataflow blocks (<see cref="BufferBlock{T}"/> → <see cref="TransformBlock{TInput,TOutput}"/>
///→ ... → <see cref="ActionBlock{T}"/>), automatically linking each stage to the next with
///<see cref="DataflowLinkOptions.PropagateCompletion"/> set, so completing or faulting the head block cascades all
///the way through to the final stage without the caller wiring each link by hand.
///</summary>
///<typeparam name="TInput">The pipeline's original input type.</typeparam>
///<typeparam name="TCurrent">The output type of the last stage added so far.</typeparam>
public sealed class DataflowPipelineBuilder<TInput, TCurrent>
{
    #region Fields
    readonly ITargetBlock<TInput> _head;
    readonly ISourceBlock<TCurrent> _tail;
    #endregion

    #region Constructors
    internal DataflowPipelineBuilder(ITargetBlock<TInput> head, ISourceBlock<TCurrent> tail)
    {
        _head = head;
        _tail = tail;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Appends a <see cref="TransformBlock{TInput,TOutput}"/> stage, linked to receive everything from the current tail.
    ///</summary>
    ///<typeparam name="TNext">The new stage's output type.</typeparam>
    ///<param name="transform">The transform to apply to each item.</param>
    ///<param name="options">Options for the new stage. Defaults to <see cref="ExecutionDataflowBlockOptions"/> if not specified.</param>
    ///<returns>A builder for continuing the chain from the new stage's output type.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="transform"/> is <c>null</c>.</exception>
    public DataflowPipelineBuilder<TInput, TNext> Transform<TNext>(Func<TCurrent, TNext> transform, ExecutionDataflowBlockOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(transform);

        TransformBlock<TCurrent, TNext> block = new(transform, options ?? new ExecutionDataflowBlockOptions());
        _tail.LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });

        return new DataflowPipelineBuilder<TInput, TNext>(_head, block);
    }

    ///<summary>
    ///Finishes the pipeline with an <see cref="ActionBlock{T}"/> as its terminal stage.
    ///</summary>
    ///<param name="action">The action to run for each item.</param>
    ///<param name="options">Options for the terminal stage. Defaults to <see cref="ExecutionDataflowBlockOptions"/> if not specified.</param>
    ///<returns>The completed pipeline: post items into it, then complete and await it.</returns>
    ///<exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public DataflowPipeline<TInput> ActionBlock(Action<TCurrent> action, ExecutionDataflowBlockOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        ActionBlock<TCurrent> block = new(action, options ?? new ExecutionDataflowBlockOptions());
        _tail.LinkTo(block, new DataflowLinkOptions { PropagateCompletion = true });

        return new DataflowPipeline<TInput>(_head, block.Completion);
    }
    #endregion
}

///<summary>
///A completed dataflow pipeline built by <see cref="DataflowPipelineBuilder"/>: post items via <see cref="Post"/>
///or <see cref="SendAsync"/>, then call <see cref="Complete"/> and await <see cref="Completion"/> once no more
///items will be posted.
///</summary>
///<typeparam name="TInput">The pipeline's input type.</typeparam>
public sealed class DataflowPipeline<TInput>
{
    #region Fields
    readonly ITargetBlock<TInput> _head;
    #endregion

    #region Constructors
    internal DataflowPipeline(ITargetBlock<TInput> head, Task completion)
    {
        _head = head;
        Completion = completion;
    }
    #endregion

    #region Public methods
    ///<summary>
    ///Signals that no more items will be posted. Completion cascades through every stage.
    ///</summary>
    public void Complete() => _head.Complete();

    ///<summary>
    ///Synchronously offers an item to the pipeline's head.
    ///</summary>
    ///<param name="item">The item to post.</param>
    ///<returns><c>true</c> if the item was accepted; otherwise <c>false</c>.</returns>
    public bool Post(TInput item) => _head.Post(item);

    ///<summary>
    ///Asynchronously offers an item to the pipeline's head, waiting if the head is applying backpressure.
    ///</summary>
    ///<param name="item">The item to post.</param>
    ///<param name="cancellationToken">A token to cancel the send.</param>
    ///<returns><c>true</c> if the item was accepted; otherwise <c>false</c>.</returns>
    public Task<bool> SendAsync(TInput item, CancellationToken cancellationToken = default) => _head.SendAsync(item, cancellationToken);
    #endregion

    #region Public properties
    ///<summary>
    ///Completes once every stage has finished processing all items following a call to <see cref="Complete"/>, or
    ///faults if any stage faults.
    ///</summary>
    public Task Completion { get; }
    #endregion
}
