using System.ComponentModel.DataAnnotations.Schema;

namespace Sehha360.Models
{
    public class DocumentAccessLog
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public MedicalDocument Document { get; set; }
        
        [ForeignKey("AccessedByUser")]
        public string AccessedByUserId { get; set; }
        public AppUser AccessedByUser { get; set; }
        
        public DateTime AccessedAt { get; set; }
    }
}
