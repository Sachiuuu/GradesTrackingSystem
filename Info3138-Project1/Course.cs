using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Info3138_Project1
{
    internal class Course
    {
        //Creating the properties of the Course class
        public string CourseCode { get; set; }  //Defines the course,class code (Ex. INFO3138, INFO4123)
        public List<Evaluation> Evaluations { get; set; } = new();  //A list that will hold all the properties of the activity, evaluation for the Course
    }
}
