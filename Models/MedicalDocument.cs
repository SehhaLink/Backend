using System.ComponentModel.DataAnnotations.Schema;
using Sehha360.Models.Enums;

namespace Sehha360.Models
{
    public class MedicalDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; } // S3 Key
        public DocumentType DocumentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public DocumentProcessingStatus ProcessingStatus { get; set; }

        [ForeignKey("Patient")]
        public string PatientId { get; set; }
        public AppUser Patient { get; set; }
    }
}
