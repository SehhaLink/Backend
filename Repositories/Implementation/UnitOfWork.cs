using Sehha360.Data;
using Sehha360.Models;
using Sehha360.Repositories.Interface;

namespace Sehha360.Repositories.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IRepository<AppUser> AppUsers { get; }
        public IRepository<DoctorPatient> DoctorPatients { get; }
        public IRepository<MedicalReport> MedicalReports { get; }
        public IRepository<Medication> Medications { get; }
        public IRepository<MedicationReminders> MedicationReminders { get; }
        public IRepository<UserMedications> UserMedications { get; }
        public IRepository<UserRelationships> UserRelationships { get; }
        public IOtpRepository OTPs { get; }


        public UnitOfWork(AppDbContext context, IRepository<AppUser> appUsers, IRepository<DoctorPatient> doctorPatients,
            IRepository<MedicalReport> medicalReports,
            IRepository<Medication> medications,
            IRepository<MedicationReminders> medicationReminders,
            IRepository<UserMedications> userMedications,
            IRepository<UserRelationships> userRelationships, IOtpRepository oTPs)
        {
            _context = context;
            DoctorPatients = doctorPatients;
            MedicalReports = medicalReports;
            Medications = medications;
            MedicationReminders = medicationReminders;
            UserMedications = userMedications;
            UserRelationships = userRelationships;
            OTPs = oTPs;
            AppUsers = appUsers;
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
