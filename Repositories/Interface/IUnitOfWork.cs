using Sehha360.Models;

namespace Sehha360.Repositories.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<AppUser> AppUsers { get; }
        IRepository<DoctorPatient> DoctorPatients { get; }
        IRepository<MedicalReport> MedicalReports { get; }
        IRepository<Medication> Medications { get; }
        IRepository<MedicationReminders> MedicationReminders { get; }
        IRepository<UserMedications> UserMedications { get; }
        IRepository<UserRelationships> UserRelationships { get; }
        IRepository<MedicalDocument> MedicalDocuments { get; }
        IRepository<DocumentAccessLog> DocumentAccessLogs { get; }
        IOtpRepository OTPs { get; }
        Task<int> SaveChangesAsync();
    }
}
