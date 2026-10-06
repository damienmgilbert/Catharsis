namespace Catharsis.Events;

///<summary>
///Represents an asynchronous handler for an <see cref="AsyncEvent{TArgs}"/>. Unlike a classic <c>EventHandler</c>, it
///returns a <see cref="Task"/> so the raiser can await it, and it receives a <see cref="CancellationToken"/>.
///</summary>
///<typeparam name="TArgs">The type of the event data.</typeparam>
///<param name="sender">The source of the event, or <c>null</c> for a static source.</param>
///<param name="args">The event data.</param>
///<param name="cancellationToken">A token that signals the raiser wants the handler to stop.</param>
///<returns>A task that completes when the handler has finished.</returns>
public delegate Task AsyncEventHandler<in TArgs>(object? sender, TArgs args, CancellationToken cancellationToken);
