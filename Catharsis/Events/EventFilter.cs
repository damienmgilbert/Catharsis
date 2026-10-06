namespace Catharsis.Events;

#region Delegates
///<summary>
///Decides whether an event value should be delivered to a handler.
///</summary>
///<typeparam name="T">The type of the event data.</typeparam>
///<param name="value">The event data.</param>
///<returns><c>true</c> to deliver the event; <c>false</c> to skip it.</returns>
public delegate bool EventFilter<in T>(T value);
#endregion
