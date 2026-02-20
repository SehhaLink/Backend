using System.ComponentModel.DataAnnotations.Schema;

namespace Sehha360.Models
{
    public class DoctorPatient
    {
        public int Id { get; set; }
        [ForeignKey("Doctor")]
        public string DoctorId { get; set; }
        public AppUser Doctor { get; set; }
        [ForeignKey("Patient")]
        public string PatientId { get; set; }
        public AppUser Patient { get; set; }
    }
}
