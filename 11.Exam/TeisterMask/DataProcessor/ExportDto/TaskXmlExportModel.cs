using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Task")]
    public class TaskXmlExportModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Label { get; set; }
    }
}
