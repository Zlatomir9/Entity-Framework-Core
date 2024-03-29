﻿using BookShop.Data.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace BookShop.DataProcessor.ImportDto
{
    [XmlType("Book")]
    public class BookInputModel
    {
        [XmlElement("Name")]
        [Required, StringLength(30, MinimumLength = 3)]
        public string Name { get; set; }

        [XmlElement("Genre")]
        [Range(1,3)]
        public int Genre { get; set; }

        [XmlElement("Price")]
        [Range(0.01, Double.MaxValue)]
        public decimal Price { get; set; }

        [XmlElement("Pages")]
        [Range(50, 5000)]
        public int Pages { get; set; }

        [XmlElement("PublishedOn")]
        [Required]
        public string PublishedOn { get; set; }
    }
}
