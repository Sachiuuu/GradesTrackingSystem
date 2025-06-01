using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Info3138_Project1
{
    internal class Evaluation
    {
        //Creating the properties of the Evaluations for a course, class. (Ex. Assignments, Quizzes, Projects, etc)
        public string Description { get; set; } //A little description of what the Evaluation is about and which kind of evaluation is
        public double Weight { get; set; }  //The % of the course mark attributed to the evaluation
        public int OutOf { get; set; }  //The number of marks that represents a perfect score on the evaluation
        public double? EarnedMarks { get; set; }  //The student’s score on the evaluation
    }
}
