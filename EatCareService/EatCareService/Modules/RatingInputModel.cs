using System.ComponentModel.DataAnnotations;

namespace EatCareService.Modules
{
    public class RatingInputModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        [Range(0.5, 5.0, ErrorMessage = "Rating value must be between 0.5 and 5.0")]
        public float RatingValue { get; set; }
    }

}
