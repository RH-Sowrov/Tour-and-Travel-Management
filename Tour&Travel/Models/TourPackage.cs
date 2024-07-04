using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tour_Travel.Models
{
    public class TourPackage
    {
        public int TourPackageId { get; set; }
        [Required]
        [ForeignKey("TourTransport")]
        public int TourTransportId { get; set; }
        [Required]
        [ForeignKey("TourHotel")]
        public int TourHotelId { get; set; }
        [Required]
        [ForeignKey("TourFood")]
        public int TourFoodId { get; set; }
        [Required]
        [ForeignKey("TourGuide")]
        public int TourGuideId { get; set; }
        [Required,
            Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        public ICollection<Tour> Tour { get; set; } = new List<Tour>();
        public TourTransport? TourTransport { get; set; }
        public TourHotel? TourHotel { get; set; }
        public TourFood? TourFood { get; set; } 
        public TourGuide? TourGuide { get; set; } 
    }
}
