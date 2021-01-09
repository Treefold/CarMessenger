using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class CarModel
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [RegularExpression(@"[0-9A-Z][0-9A-Z]{3,8}[0-9A-Z]", ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [Index("UniquePlateNumber", 1, IsUnique = true)]
        public string Plate { get; set; }

        [Required(ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))] // from the plate
        [RegularExpression(@"[A-Z]{1,3}", ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))]
        [Index("UniquePlateNumber", 2, IsUnique = true)]
        public string CountryCode { get; set; }

        [Required]
        [RegularExpression(@"[A-Za-z0-9]{0,20}", ErrorMessage = ("Please enter the color"))]
        public string ModelName { get; set; }

        [RegularExpression(@"[A-Za-z0-9]{0,20}", ErrorMessage = ("Please enter the color"))]
        public string Color { get; set; }

        public CarModel(string plate, string countryCode, string modelName, string color)
        {
            Plate       = plate       ?? throw new ArgumentNullException(nameof(plate));
            CountryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode));
            ModelName   = modelName   ?? throw new ArgumentNullException(nameof(modelName));
            Color       = color;
        }

        public CarModel()
        {
            Plate       = "Unknown";
            CountryCode = "Unknown";
            ModelName   = "Unknown";
            Color       = "Unknown";
        }
    }
}