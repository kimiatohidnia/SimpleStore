using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }

        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional properties
        public int? Stock { get; set; }

        public string Category { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}