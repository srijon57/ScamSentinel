using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ScamSentinel.Models.Scam
{
    public class ScamReportModel
    {
        [Required(ErrorMessage = "Please select a scam type")]
        [Display(Name = "Scam Type")]
        public int ScamTypeID { get; set; }

        public List<SelectListItem> AvailableScamTypes { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Please provide a title")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Display(Name = "Scammer Phone")]
        [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Please enter a valid phone number")]
        public string? ScammerPhone { get; set; }

        [Display(Name = "Scammer WhatsApp")]
        [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Please enter a valid WhatsApp number")]
        public string? ScammerWhatsApp { get; set; }

        [Display(Name = "Scammer Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? ScammerEmail { get; set; }

        [Display(Name = "Scammer Facebook")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string? ScammerFacebook { get; set; }

        [Display(Name = "Scammer Name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? ScammerName { get; set; }

        [Display(Name = "Scammer Organization")]
        [StringLength(100, ErrorMessage = "Organization name cannot exceed 100 characters")]
        public string? ScammerOrganization { get; set; }

        [Display(Name = "Financial Loss")]
        [Range(0, double.MaxValue, ErrorMessage = "Loss amount cannot be negative")]
        public decimal? LossAmount { get; set; }

        public string Currency { get; set; } = "USD";

        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters")]
        public string? Location { get; set; }

        [Display(Name = "Occurrence Date")]
        [DataType(DataType.Date)]
        public DateTime? OccurrenceDate { get; set; }

        [Required(ErrorMessage = "Please provide a description of the scam")]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
        public string Description { get; set; }

        [Display(Name = "Evidence Files (Max 5 files)")]
        public List<IFormFile>? EvidenceFiles { get; set; }
    }
}