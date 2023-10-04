using Rhinox.Utilities;

namespace Rhinox.Magnus.Tasks
{
    public interface IAutocompleteAction
    {
        void Trigger(ManagedCoroutine.FinishedHandler autocompletedHandler);
    }
}