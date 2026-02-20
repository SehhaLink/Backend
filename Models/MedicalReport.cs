using System.ComponentModel.DataAnnotations.Schema;

namespace Sehha360.Models
{
    public class MedicalReport
    {
        public int Id { get; set; }
        public string? ReportSummaryForPatient { get; set; }
        public string? ReportSummaryForDoctor { get; set; }
        public DateTime ReportDate { get; set; }
        public DateTime UploadedAt { get; set; }
        public string ReportType { get; set; } // AI must give me this
        public string DiseaseName { get; set; }
        [ForeignKey("Doctor")]
        public string? DoctorId { get; set; }
        public AppUser? Doctor { get; set; }
        [ForeignKey("Patient")]
        public string PatientId { get; set; }
        public AppUser Patient { get; set; }
    }
}
