using System.Collections.Generic;

namespace CarDealer.DTO
{
    public class ImportCarModel
    {
        public string Make { get; set; }

        public string Model { get; set; }

        public int TravelledDistance { get; set; }

        public IEnumerable<int> PartsId { get; set; }
    }
}
