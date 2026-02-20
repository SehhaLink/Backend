using System.ComponentModel.DataAnnotations.Schema;

namespace Sehha360.Models
{
    public class MedicationReminders
    {
        public int Id { get; set; }
        public DateTime ReminderTime { get; set; }
        public string Notes { get; set; }
        public bool IsTaken { get; set; }
        public DateTime TakenAt { get; set; }
        [ForeignKey("UserMedication")]
        public int UserMedicationId { get; set; }
        public UserMedications UserMedication { get; set; }
    }
}
