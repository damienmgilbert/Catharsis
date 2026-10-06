using System.Threading.Tasks.Dataflow;

namespace Catharsis.Concurrency;

///<summary>
///A completed dataflow pipeline built by <see cref="DataflowPipelineBuilder"/>: post items via <see cref="Post"/> or
///<see cref="SendAsync"/>, then call <see cref="Complete"/> and await <see cref="Completion"/> once no more items will
///be posted.
///</summary>
///<typeparam name="TInput">The pipeline's input type.</typeparam>
public sealed class DataflowPipeline<TInput>
{
    #region Fields
    private readonly ITargetBlock<TInput> _head;
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
