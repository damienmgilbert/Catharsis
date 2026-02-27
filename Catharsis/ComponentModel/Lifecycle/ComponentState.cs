namespace Catharsis.ComponentModel.Lifecycle;

///<summary>
///Defines the lifecycle states a component can occupy within a <see cref="ComponentStateMachine"/>.
///</summary>
public enum ComponentState
{
    ///<summary>
    ///The component has been created but not yet initialized.
    ///</summary>
    Created = 0,

    ///<summary>
    ///The component is being initialized (resources allocated, configuration applied).
    ///</summary>
    Initializing = 1,

    ///<summary>
    ///The component has been initialized and is ready for activation.
    ///</summary>
    Initialized = 2,

    ///<summary>
    ///The component is being activated (starting operations).
    ///</summary>
    Activating = 3,

    ///<summary>
    ///The component is active and fully operational.
    ///</summary>
    Active = 4,

    ///<summary>
    ///The component is being deactivated (pausing or suspending operations).
    ///</summary>
    Deactivating = 5,

    ///<summary>
    ///The component is deactivated and idle but can be reactivated.
    ///</summary>
    Deactivated = 6,

    ///<summary>
    ///The component is being disposed (releasing resources).
    ///</summary>
    Disposing = 7,

    ///<summary>
    ///The component has been disposed and cannot be reused.
    ///</summary>
    Disposed = 8,

    ///<summary>
    ///The component is in a faulted state due to an error.
    ///</summary>
    Faulted = 9
}
