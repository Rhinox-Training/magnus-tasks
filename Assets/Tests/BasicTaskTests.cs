// using System;
// using System.Collections;
// using NUnit.Framework;
// using Rhinox.Lightspeed;
// using Rhinox.Magnus.Tasks;
// using Tests.Helpers;
// using UnityEngine;
// using UnityEngine.TestTools;
//
// namespace Tests
// {
//     [TestFixture]
//     public class BasicTaskTests
//     {
//         private TestCondition _condition;
//         private TaskBehaviour _task;
//
//         [OneTimeSetUp]
//         public void Setup()
//         {
//             TaskManager.FindOrCreate();
//             TaskManager.Instance.RunTaskOnStart = false;
//         }
//         
//         [OneTimeTearDown]
//         public void Teardown()
//         {
//         }
//         
//         [SetUp]
//         public void SetupSingle()
//         {
//             _condition = new TestCondition();
//             _task = TaskFactory.BuildBasicTaskFromConditions(_condition);
//             TaskManager.Instance.LoadTasks(_task);
//         }
//
//         [TearDown]
//         public void TeardownSingle()
//         {
//             _condition = null;
//             Utility.DestroyObject(_task);
//             _task = null;
//             TaskManager.Instance.ClearTasks();
//         }
//         
//         [TestCase]
//         public void TestTaskStartState()
//         {
//             TaskManager.Instance.StartCurrentTask();
//             Assert.AreEqual(TaskState.Running, _task.State);
//         }
//
//         [TestCase(ExpectedResult = true)]
//         public bool TestStartTaskReturn()
//         {
//             return TaskManager.Instance.StartCurrentTask();
//         }
//
//         [UnityTest]
//         public IEnumerator TestTaskCompleted()
//         {
//             TaskManager.Instance.StartCurrentTask();
//             
//             _condition.SetConditionMet();
//
//             for (int i = 0; i < 6; ++i)
//                 yield return new WaitForEndOfFrame();
//
//             Assert.AreEqual(TaskState.Finished, _task.State);
//         }
//
//         [UnityTest]
//         public IEnumerator TestTaskCompletedSuccess()
//         {
//             TaskManager.Instance.StartCurrentTask();
//             
//             _condition.SetConditionMet();
//
//             for (int i = 0; i < 6; ++i)
//                 yield return new WaitForEndOfFrame();
//
//             Assert.AreEqual(CompletionState.Success, _task.CompletionState);
//         }
//
//         [UnityTest]
//         public IEnumerator TestTaskCompletedFailure()
//         {
//             TaskManager.Instance.StartCurrentTask();
//             
//             _condition.SetConditionMet(true);
//
//             for (int i = 0; i < 6; ++i)
//                 yield return new WaitForEndOfFrame();
//
//             Assert.AreEqual(CompletionState.Failure, _task.CompletionState);
//         }
//     }
// }