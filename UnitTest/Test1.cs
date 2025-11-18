using SorteringsSystem;
using SorteringsSystem.ApplicationLayer;
using SorteringsSystem.Models;
using SorteringsSystem.ViewModels;
using SorteringsSystem.Views;
using System.Runtime.ExceptionServices;
namespace UnitTest
{
    [TestClass]
    public sealed class Test1
    {
        TaskItem t;
        SubTask s;
        MainViewModel mvn;

        [TestMethod]
        public void CreateNewTaskTest()
        {
            Exception? threadEx = null;

            Thread stathread = new Thread(() => 
                {
                try {
                    // Arrange 
                    mvn = new MainViewModel();

                    // Act 
                    mvn.CreateNewTask();

                    // Assert 
                    // 2 Tasks are hardcoded as pre-condition
                    Assert.IsTrue(mvn.Tasks.Count > 2);
                }
                catch(Exception e)
                {
                    threadEx = e;
                }
            });

            stathread.SetApartmentState(ApartmentState.STA);
            stathread.IsBackground = false;
            stathread.Start();
            stathread.Join();
          
          
            if (threadEx != null)
            {
                ExceptionDispatchInfo.Capture(threadEx).Throw(); 
            }

          

          
        }
        [TestMethod]
        public void DoesNoteUpdate()
        {
            Exception? threadEx = null;

            Thread stathread = new Thread(() =>
            {
                try
                {
                    // Arrange 
                    mvn = new MainViewModel();
                    t = new TaskItem();
                    t.Note = string.Empty;

                    // Act 
                    mvn.CreateNewTask();
                


                    // Assert 

                    Assert.AreNotEqual(mvn.Tasks.ElementAt(2).Note, t.Note);
                }
                catch (Exception e)
                {
                    threadEx = e;
                }
            });

            stathread.SetApartmentState(ApartmentState.STA);
            stathread.IsBackground = false;
            stathread.Start();
            stathread.Join();


            if (threadEx != null)
            {
                ExceptionDispatchInfo.Capture(threadEx).Throw();
            }
        }

        [TestMethod]
        public void IsSubtaskAdded()
        {
            Exception? threadEx = null;

            Thread stathread = new Thread(() =>
            {
                try
                {
                    // Arrange 
                    mvn = new MainViewModel();
                    t = new TaskItem();

                    // Act 
                    mvn.CreateNewTask();

                    // Assert 

                    Assert.AreNotEqual(mvn.Tasks.ElementAt(2).SubTasks, t.SubTasks);
                }
                catch (Exception e)
                {
                    threadEx = e;
                }
            });

            stathread.SetApartmentState(ApartmentState.STA);
            stathread.IsBackground = false;
            stathread.Start();
            stathread.Join();


            if (threadEx != null)
            {
                ExceptionDispatchInfo.Capture(threadEx).Throw();
            }
        }
    } 


}
