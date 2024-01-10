using System.Collections;
using System.Linq;
using NUnit.Framework;
using Rhinox.Lightspeed;
using Rhinox.Magnus.Tasks;
using Tests.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    [TestFixture]
    public class TaskManagerTests
    {
        private TaskBehaviour _task;
        private TaskBehaviour _task2;
        
        [OneTimeSetUp]
        public void Setup()
        {
            _task = TaskFactory.BuildBasicTask();
            _task2 = TaskFactory.BuildBasicTask();
            TaskManager.FindOrCreate();
        }
        
        [OneTimeTearDown]
        public void Teardown()
        {
            Utility.DestroyObject(_task);
            Utility.DestroyObject(_task2);
        }

        [TearDown]
        public void TeardownSingle()
        {
            TaskManager.Instance.ClearTasks();
        }
        
        [TestCase(ExpectedResult = true)]
        public bool AssertTaskManagerExists()
        {
            return TaskManager.HasInstance;
        }
        
        // [TestCase(ExpectedResult = true)]
        // public bool AssertAppendTask()
        // {
        //     TaskManager.Instance.AppendTasks(_task);
        //
        //     return TaskManager.Instance.GetTasks().Contains(_task);
        // }
        //
        // [TestCase(ExpectedResult = true)]
        // public bool AssertAppendTaskContains()
        // {
        //     return TaskManager.Instance.AppendTasks(_task);
        // }
        //
        // [TestCase(ExpectedResult = false)]
        // public bool AssertAppendTaskRepeating()
        // {
        //     TaskManager.Instance.AppendTasks(_task);
        //     return TaskManager.Instance.AppendTasks(_task);
        // }
        //
        // [TestCase(ExpectedResult = 2)]
        // public int AssertAppendTaskRepeatingContains()
        // {
        //     TaskManager.Instance.AppendTasks(_task, _task2);
        //     TaskManager.Instance.AppendTasks(_task);
        //
        //     return TaskManager.Instance.GetTasks().Count(x => x.EqualsOneOf(_task, _task2));
        // }
        //
        // [TestCase(ExpectedResult = true)]
        // public bool AssertLoadTaskCheckCurrent()
        // {
        //     TaskManager.Instance.LoadTasks(_task);
        //     return TaskManager.Instance.CurrentTask == _task;
        // }
        //
        // [TestCase(ExpectedResult = true)]
        // public bool AssertLoadTaskRepeating()
        // {
        //     TaskManager.Instance.LoadTasks(_task);
        //     return TaskManager.Instance.LoadTasks(_task);
        // }
        //
        // [TestCase(ExpectedResult = 1)]
        // public int AssertLoadTaskRepeatingContains()
        // {
        //     TaskManager.Instance.LoadTasks(_task);
        //     TaskManager.Instance.LoadTasks(_task2);
        //
        //     return TaskManager.Instance.GetTasks().Count(x => x.EqualsOneOf(_task, _task2));
        // }
        //
        // [TestCase(ExpectedResult = 0)]
        // public int AssertClearTasksEmpty()
        // {
        //     TaskManager.Instance.LoadTasks(_task);
        //     TaskManager.Instance.ClearTasks();
        //     return TaskManager.Instance.GetTasks().Length;
        // }
    }
}