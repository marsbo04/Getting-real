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
        [TestMethod]

        public void TestLoadingOfTextFile()
        {
            // Arange 
            // Create a tempfile and gives path to said file
            string tempfile = Path.GetTempFileName();


            try
            {
                // Arange 
                string line = "Title: Test, Description: beskrivelse, " +
                    "Mail: me@first.dk, Status: Under Indtastning, " +
                    "Priority: Høj, Complexity: Moderat, Note: Note, " +
                    "SubTasks: [0 Title: SubTask1, Text: Text]";
                // Arrange 
                // Inserts text into tempfile by following path and closes text file
                File.WriteAllText(tempfile, line);

                // Act 
                // Constructor for InMemeoryTaskRepository calls LoadTextfile method
                InMemoryTaskRepository repo = new InMemoryTaskRepository(tempfile);
                List<TaskItem> items = (List<TaskItem>)repo.GetAll();
                TaskItem testing = items[0];

                // Assert
                Assert.AreEqual("Test", testing.Title);
                Assert.AreEqual("beskrivelse", testing.Description);
                Assert.AreEqual("me@first.dk", testing.Mail);
                Assert.AreEqual("Under Indtastning", testing.Status);
                Assert.AreEqual("Høj", testing.Priority);
                Assert.AreEqual("Moderat", testing.Complexity);
                Assert.AreEqual("Note", testing.Note);
                Assert.AreEqual("SubTask1", testing.SubTasks[0].Title);
                Assert.AreEqual("Text", testing.SubTasks[0].Text);
            } 
            finally
            {
                File.Delete(tempfile);
            }
           




        }

        [TestMethod]
        public void TestUpdatingAndSavingTextFile()
        {
            // Arange 
            string tempfile = Path.GetTempFileName();

            TaskItem dummytask = new TaskItem
            {
                Title = "Test",
                Description = "Beskrivelse",
                Mail = "test@first.dk",
                Status = "Under Indtastning",
                Priority = "Høj",
                Complexity = "Moderat",
                Note = "Noteret"
                
            }; 



             
            try
            {
                // Act
                InMemoryTaskRepository imtr = new InMemoryTaskRepository(tempfile);
                imtr._store.Clear();
                imtr.Add(dummytask);
                imtr.UpdateTaskFile();
                List<TaskItem> items = (List<TaskItem>)imtr.GetAll();
                TaskItem testing = items[0];


                // Assert 
                Assert.AreEqual("Test", testing.Title);
                Assert.AreEqual("Beskrivelse", testing.Description);
                Assert.AreEqual("test@first.dk", testing.Mail);
                Assert.AreEqual("Under Indtastning", testing.Status);
                Assert.AreEqual("Høj", testing.Priority);
                Assert.AreEqual("Moderat", testing.Complexity);
                Assert.AreEqual("Noteret", testing.Note);


            }
            finally
            {
                File.Delete(tempfile);
            }







        }
        
    } 


}
