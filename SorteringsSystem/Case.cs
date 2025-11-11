using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SorteringsSystem
{
    public class Case
    {
        public enum Status
        {
            Rejected,      // Task was rejected
            InProgress,    // Task is being filled out or started
            Active,        // Task is currently being worked on
            Completed      // Task is finished
        }
        public enum Prioritizing
        {
            Low,
            Medium, 
            High
        }
        public enum CaseComplexity
        {
            Trivial,     // Næsten ingen indsats
            Simple,      // Let opgave
            Moderate,    // Gennemsnitlig sværhedsgrad
            Complex,     // Avanceret opgave
            Critical     // Ekstremt kompleks, kræver ekspertise
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime Deadline { get; set; }
        public int TaskAmount { get; set; }
        public string Note { get; set; }
    }
}
