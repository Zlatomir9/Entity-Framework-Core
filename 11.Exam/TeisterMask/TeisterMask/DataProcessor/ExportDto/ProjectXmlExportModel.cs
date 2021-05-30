using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Project")]
    public class ProjectXmlExportModel
    {
        [XmlAttribute("TasksCount")]
        public int TasksCount { get; set; }

        [Required]
        public string ProjectName { get; set; }

        [Required]
        public string HasEndDate { get; set; }

        [XmlArray("Tasks")]
        public TaskXmlExportModel[] Tasks { get; set; }
    }
}
