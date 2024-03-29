﻿using System.Xml.Serialization;

namespace CarDealer.DTO.Output
{
    [XmlType("part")]
    public class CarPartInfoOutputModel
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("price")]
        public decimal Price { get; set; }
    }
}
