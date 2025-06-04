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
            Console.WriteLine("Grades Tracking System\n--------------------------");

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
                        // No JSON? No Party...
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

                                //Reading the json data into a list of CourseProperties objects, if the json data is invalid, it will create an empty list to prevent null reference exceptions
                                List<CourseProperties> courseList = JsonConvert.DeserializeObject<List<CourseProperties>>(jsonData) ?? new();

                                // Repeatedly prompt user for commands until user enters 'X' to quit
                                bool innerDone = false;
                                do
                                {
                                    if (courseList.Count == 0)
                                    {
                                        // No courses found, prompt to add a new course
                                        Console.Clear();
                                        Console.WriteLine("\t\t~ GRADES TRACKING SYSTEM ~\n");
                                        Console.WriteLine("No courses found.");
                                        Console.WriteLine("Press A to add a new course.");
                                        Console.WriteLine("Press X to quit.");
                                        Console.Write("\nEnter a command: ");

                                        string input = Console.ReadLine()?.Trim().ToUpper() ?? "";

                                        if (input == "A")
                                        {
                                            // Add a new course
                                            AddCourse(courseList, jsonSchema);
                                        }
                                        else if (input == "X")
                                        {
                                            // Exit the inner loop
                                            innerDone = true;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid input. Press any key...");
                                            Console.ReadKey(); 
                                        }
                                    }
                                    else
                                    {
                                        // Display the course summary and prompt for commands
                                        DisplayCourseSummary(courseList);

                                        string input = Console.ReadLine()?.Trim().ToUpper() ?? "";

                                        if (input == "A")
                                        {
                                            AddCourse(courseList, jsonSchema);
                                        }
                                        else if (input == "X")
                                        {
                                            innerDone = true;
                                        }
                                        else if (int.TryParse(input, out int selection) && selection >= 1 && selection <= courseList.Count) // Check if input is a valid course number
                                        {
                                            ManageCourse(courseList[selection - 1], courseList, jsonSchema);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Invalid input. Press any key...");
                                            Console.ReadKey();
                                        }
                                    }
                                } while (!innerDone);


                                // Save before quitting
                                File.WriteAllText(pathName, JsonConvert.SerializeObject(courseList, Formatting.Indented));
                                Console.WriteLine("Data saved. Goodbye.");

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
        private static void DisplayCourseSummary(List<CourseProperties> courseList)
        {
            Console.Clear();
            Console.WriteLine("\t\t~ GRADES TRACKING SYSTEM ~\n");

            Console.WriteLine("+-------------------------------------------------------------+");
            Console.WriteLine("|                        Grades Summary                       |");
            Console.WriteLine("+-------------------------------------------------------------+");

            Console.WriteLine("#. Course        Marks Earned    Out Of     Percent");

            for (int i = 0; i < courseList.Count; i++)
            {
                CourseProperties course = courseList[i];

                double totalEarned = 0;
                double totalOutOf = 0;

                foreach (var eval in course.Evaluations)
                {
                    if (eval.EarnedMarks.HasValue)
                    {
                        totalEarned += eval.EarnedMarks.Value;
                        totalOutOf += eval.OutOf;
                    }
                }

                double percent = totalOutOf > 0 ? (100.0 * totalEarned / totalOutOf) : 0;

                Console.WriteLine($"{i + 1}. {course.Code,-13} {totalEarned,8:0.0} {totalOutOf,12:0.0} {percent,10:0.0}");
            }

            Console.WriteLine("\n---------------------------------------------------------------");
            Console.WriteLine("Press # from the above list to view/edit/delete a specific course.");
            Console.WriteLine("Press A to add a new course.");
            Console.WriteLine("Press X to quit.");
            Console.WriteLine("---------------------------------------------------------------");
            Console.Write("\nEnter a command: ");
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

        private static void AddCourse(List<CourseProperties> courseList, string schema)
        {
            Console.Write("Enter course code (e.g., INFO-3138): ");
            string code = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("Invalid input. Course code cannot be empty.");
                return;
            }

            var newCourse = new CourseProperties
            {
                Code = code,
                Evaluations = new List<Evaluation>()
            };

            // Validate this course
            string tempJson = JsonConvert.SerializeObject(newCourse);
            string wrappedJson = "[" + tempJson + "]"; // Schema expects an array

            if (ValidateCourseData(wrappedJson, schema, out IList<string> messages))
            {
                courseList.Add(newCourse);
            }
            else
            {
                Console.WriteLine("ERROR: Course is invalid:");
                foreach (string msg in messages)
                    Console.WriteLine($" - {msg}");
            }
        }

        private static void ManageCourse(CourseProperties course, List<CourseProperties> allCourses, string schema)
        {
            bool done = false;

            while (!done)
            {
                Console.Clear();
                Console.WriteLine("\t\t~ GRADES TRACKING SYSTEM ~\n");

                Console.WriteLine("+---------------------------------------------------+");
                Console.WriteLine("|                                                   |");
                Console.WriteLine($"|               {course.Code} Evaluations".PadRight(52) + "|");
                Console.WriteLine("|                                                   |");
                Console.WriteLine("+---------------------------------------------------+\n");

                if (course.Evaluations.Count == 0)
                {
                    Console.WriteLine("There are currently no evaluations for " + course.Code + ".\n");
                }

                else
                {
                    Console.WriteLine("#. Evaluation        Earned / OutOf     %       Weight");
                    Console.WriteLine("------------------------------------------------------------");
                    for (int i = 0; i < course.Evaluations.Count; i++)
                    {
                        var eval = course.Evaluations[i];
                        string earnedDisplay = eval.EarnedMarks.HasValue ? eval.EarnedMarks.Value.ToString("0.0") : "N/A";
                        double percent = eval.EarnedMarks.HasValue ? 100.0 * eval.EarnedMarks.Value / eval.OutOf : 0;

                        Console.WriteLine($"{i + 1}. {eval.Description.PadRight(20)} {earnedDisplay}/{eval.OutOf}       {percent,5:0.0}%     {eval.Weight}%");
                    }
                }

                Console.WriteLine(new string('-', 70));
                Console.WriteLine("Press D to delete this course.");
                Console.WriteLine("Press A to add an evaluation.");
                Console.WriteLine("Press # from the above list to edit/delete a specific evaluation.");
                Console.WriteLine("Press X to return to the main menu.");
                Console.WriteLine(new string('-', 70));
                Console.Write("\nEnter a command: ");

                string input = Console.ReadLine()?.Trim().ToUpper() ?? "";

                if (input == "X")
                {
                    done = true;
                }
                else if (input == "D")
                {
                    Console.Write($"\nDelete {course.Code}? (y/n): ");
                    string confirm = Console.ReadLine()?.ToLower() ?? "n";
                    if (confirm == "y")
                    {
                        allCourses.Remove(course);
                        Console.WriteLine("Course deleted. Press any key...");
                        Console.ReadKey();
                        return;
                    }
                }
                else if (input == "A")
                {
                    AddEvaluation(course, schema);
                }
                else if (int.TryParse(input, out int index) && index >= 1 && index <= course.Evaluations.Count)
                {
                    // Optional future feature: edit/delete specific evaluation
                }
                else
                {
                    Console.WriteLine("Invalid input. Press any key...");
                    Console.ReadKey();
                }
            }
        }

        private static void AddEvaluation(CourseProperties course, string schema)
        {
            Console.Write("Enter a description: ");
            string desc = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter the 'out of' mark: ");
            if (!int.TryParse(Console.ReadLine(), out int outOf))
            {
                Console.WriteLine("Invalid 'out of' mark.");
                return;
            }

            Console.Write("Enter the % weight: ");
            if (!double.TryParse(Console.ReadLine(), out double weight))
            {
                Console.WriteLine("Invalid weight.");
                return;
            }

            Console.Write("Enter marks earned or press ENTER to skip: ");
            string earnedInput = Console.ReadLine()?.Trim() ?? "";
            double? earnedMarks = null;
            if (!string.IsNullOrEmpty(earnedInput))
            {
                if (double.TryParse(earnedInput, out double parsed))
                    earnedMarks = parsed;
                else
                {
                    Console.WriteLine("Invalid earned marks.");
                    return;
                }
            }

            // Build the new evaluation
            Evaluation eval = new Evaluation
            {
                Description = desc,
                OutOf = outOf,
                Weight = weight,
                EarnedMarks = earnedMarks
            };

            // Temporarily add to course for validation
            course.Evaluations.Add(eval);

            // Wrap current course into a list and validate
            string tempJson = JsonConvert.SerializeObject(new List<CourseProperties> { course });
            if (ValidateCourseData(tempJson, schema, out IList<string> messages))
            {
                Console.WriteLine("Evaluation added successfully.");
            }
            else
            {
                Console.WriteLine("ERROR: Evaluation is invalid:");
                foreach (string msg in messages)
                    Console.WriteLine($" - {msg}");
                course.Evaluations.Remove(eval); // Undo
            }
        }

    }

}

