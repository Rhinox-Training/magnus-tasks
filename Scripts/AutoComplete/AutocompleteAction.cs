using Rhinox.Utilities;

namespace Rhinox.Magnus.Tasks
{
    public class AutocompleteAction : IAutocompleteAction
    {
        public BaseCondition Condition;
        public AutocompleteBot TaskCreatorProvider;

        public AutocompleteAction(BaseCondition condition, AutocompleteBot taskCreatorProvider)
        {
            Condition = condition;
            TaskCreatorProvider = taskCreatorProvider;
        }

        public void Trigger(ManagedCoroutine.FinishedHandler autocompletedHandler)
        {
            if (Condition.IsMet)
            {
                autocompletedHandler?.Invoke(true);
                return;
            }
            
            var taskCreator = TaskCreatorProvider.FetchActionCreator(Condition.GetType());
            if (!Condition.IsStarted)
                Condition.Start();
            var currTask = taskCreator.CreateTask(Condition);
            
            currTask.OnFinished += autocompletedHandler;
            // currTask.Start(); // Task is autostarted
        }
        
    }

}