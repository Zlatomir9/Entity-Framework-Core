namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XmlSerializer serializer = new XmlSerializer(typeof(ProjectXmlImportModel[]), new XmlRootAttribute("Projects"));
            var projectsDtos = (ProjectXmlImportModel[])serializer.Deserialize(new StringReader(xmlString));

            foreach (var projectDto in projectsDtos)
            {
                if (!IsValid(projectDto))
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                var date = DateTime.TryParseExact(projectDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime dueDate);

                var project = new Project();

                if (date == false)
                {
                    project = new Project
                    {
                        Name = projectDto.Name,
                        OpenDate = DateTime.ParseExact(projectDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    };
                }
                else
                {
                    project = new Project
                    {
                        Name = projectDto.Name,
                        OpenDate = DateTime.ParseExact(projectDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        DueDate = dueDate
                    };
                }

                foreach (var taskDto in projectDto.Tasks)
                {
                    if (!IsValid(taskDto))
                    {
                        stringBuilder.AppendLine(ErrorMessage);
                        continue;
                    }

                    var taskOpenDate = DateTime.ParseExact(taskDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var taskDueDate = DateTime.ParseExact(taskDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    if (taskOpenDate < project.OpenDate)
                    {
                        stringBuilder.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (taskDueDate > project.DueDate)
                    {
                        stringBuilder.AppendLine(ErrorMessage);
                        continue;
                    }

                    var task = new Task
                    {
                        Name = taskDto.Name,
                        OpenDate = DateTime.ParseExact(taskDto.OpenDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        DueDate = DateTime.ParseExact(taskDto.DueDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ExecutionType = (ExecutionType)Enum.Parse(typeof(ExecutionType), taskDto.ExecutionType),
                        LabelType = (LabelType)Enum.Parse(typeof(LabelType), taskDto.LabelType)
                    };

                    project.Tasks.Add(task);
                }

                context.Projects.Add(project);
                context.SaveChanges();
                stringBuilder.AppendLine(String.Format(SuccessfullyImportedProject, project.Name, project.Tasks.Count));
            }

            return stringBuilder.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var employeesDtos = JsonConvert.DeserializeObject<IEnumerable<EmployeeJsonImportModel>>(jsonString);
            

            foreach (var employeeDto in employeesDtos)
            {
                var tasks = new List<Task>();

                if (!IsValid(employeeDto))
                {
                    stringBuilder.AppendLine(ErrorMessage);
                    continue;
                }

                var employee = new Employee
                {
                    Username = employeeDto.Username,
                    Email = employeeDto.Email,
                    Phone = employeeDto.Phone
                };

                foreach (var taskDto in employeeDto.Tasks)
                {
                    var task = context.Tasks.FirstOrDefault(x => x.Id == taskDto);

                    if (task == null)
                    {
                        stringBuilder.AppendLine(ErrorMessage);
                        continue;
                    }

                    tasks.Add(task);
                }

                var uniqueTasks = tasks.Distinct();

                foreach (var task in uniqueTasks)
                {
                    employee.EmployeesTasks.Add(new EmployeeTask { TaskId = task.Id });
                }

                context.Employees.Add(employee);
                context.SaveChanges();
                stringBuilder.AppendLine(String.Format(SuccessfullyImportedEmployee, employee.Username, employee.EmployeesTasks.Count));
            }

            return stringBuilder.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}