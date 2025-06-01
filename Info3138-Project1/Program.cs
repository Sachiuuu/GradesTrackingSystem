/*
    Name: Daniel Diaz/David Bonilla
    Student Number: 1231950/1207650
    Description:

 */

using System;

using System.Collections.Generic;   // IList<> class
using System.IO;                    // File class
using Newtonsoft.Json;              // JsonConvert class
using Newtonsoft.Json.Schema;       // JSchema  class
using Newtonsoft.Json.Linq;         // JObject class

namespace Info3138_Project1
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Grades Tracking System\n-----------------------------------");

            // Attempt to read the json schema file into a string variable
            if (ReadFile("course-grade-schema.json", out string jsonSchema))
            {
                // Repeatedly prompt user for data file path until user enters nothing
                bool done = false;
                do
                {
                    // Ask the user to input a json data file path
                    Console.Write("\nEnter the path name of a JSON team file (or press enter to quit): ");
                    string pathName = Console.ReadLine() ?? "";

                    if (string.IsNullOrEmpty(pathName))
                    {
                        // All done!
                        Console.WriteLine("\nNothing to do. Shutting down.\n");
                        done = true;
                    }
                    else
                    {
                        // Attempt to read the json data file into a string variable
                        if (ReadFile(pathName, out string jsonData))
                        {
                            // Validate the json data against the schema
                            if (ValidateCourseData(jsonData, jsonSchema, out IList<string> messages)) // Note: messages parameter is optional
                            {
                                Console.WriteLine($"\nData file is valid.");

                                DisplayCourseInfo(jsonData);
                            }
                            else
                            {
                                Console.WriteLine($"\nERROR:\tData file is invalid.\n");

                                // Report validation error messages
                                foreach (string msg in messages)
                                    Console.WriteLine($"\t{msg}");
                            }
                        }
                        else
                        {
                            // Read operation for data failed
                            Console.WriteLine("\nERROR:\tUnable to read the data file. Try another path or press enter to quit.");
                        }
                    }
                } while (!done);
            }
            else
            {
                // Read operation for schema failed
                Console.WriteLine("ERROR:\tUnable to read the schema file.");
            }
        }
        

        // Attempts to read the json file specified by 'path' into the string 'json'
        // Returns 'true' if successful or 'false' if it fails
        private static bool ReadFile(string path, out string json)
        {
            try
            {
                // Read JSON file data 
                json = File.ReadAllText(path);
                return true;
            }
            catch
            {
                json = "";
                return false;
            }
        } // end ReadFile()

        // Reports the information contained in the parameter 'json' in the console window
        private static void DisplayCourseInfo(string json)
        {
            try
            {
                // Deserialize the JSON into a list of CourseProperties
                List<CourseProperties> courseList = JsonConvert.DeserializeObject<List<CourseProperties>>(json) ?? new();

                // Loop through each course
                foreach (var course in courseList)
                {
                    Console.WriteLine($"\n\tCourse Code: {course.CourseCode}");
                    Console.WriteLine("\n\tEvaluations:");

                    if (course.Evaluations.Count == 0)
                    {
                        Console.WriteLine("\tNo evaluations found.");
                    }
                    else
                    {
                        foreach (Evaluation e in course.Evaluations)
                        {
                            Console.WriteLine($"\t- {e.Description}: Weight={e.Weight}%, OutOf={e.OutOf}, EarnedMarks={e.EarnedMarks}");
                        }
                    }

                    Console.WriteLine(new string('-', 50)); // Separator
                }
            }
            catch (JsonException)
            {
                Console.WriteLine("\nERROR: Failed to convert data in JSON file to Course objects.");
            }
        }


        // Validates the json data specified by the parameter 'jsonData' against the schema
        // specified by the parameter 'jsonSchema'
        // Returns 'true' if valid or 'false' if invalid
        // Also populates the out parameter 'messages' with validation error messages if invalid
        private static bool ValidateCourseData(string jsonData, string jsonSchema, out IList<string> messages)
        {
            messages = new List<string>();
            JSchema schema = JSchema.Parse(jsonSchema);

            try
            {
                JArray courseArray = JArray.Parse(jsonData);

                bool allValid = true;

                foreach (var course in courseArray)
                {
                    if (!course.IsValid(schema, out IList<string> courseMessages))
                    {
                        allValid = false;
                        foreach (string msg in courseMessages)
                            messages.Add(msg);
                    }
                }

                return allValid;
            }
            catch (JsonReaderException)
            {
                messages.Add("The JSON file format is invalid or malformed.");
                return false;
            }
        }

    }

}

