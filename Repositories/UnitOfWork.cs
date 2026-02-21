using Sehha360.Data;
using Sehha360.Models;

namespace Sehha360.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IRepository<AppUser>? _appUsers;
        private IRepository<DoctorPatient>? _doctorPatients;
        private IRepository<MedicalReport>? _medicalReports;
        private IRepository<Medication>? _medications;
        private IRepository<MedicationReminders>? _medicationReminders;
        private IRepository<UserMedications>? _userMedications;
        private IRepository<UserRelationships>? _userRelationships;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<AppUser> AppUsers =>
            _appUsers ??= new Repository<AppUser>(_context);

        public IRepository<DoctorPatient> DoctorPatients =>
            _doctorPatients ??= new Repository<DoctorPatient>(_context);

        public IRepository<MedicalReport> MedicalReports =>
            _medicalReports ??= new Repository<MedicalReport>(_context);

        public IRepository<Medication> Medications =>
            _medications ??= new Repository<Medication>(_context);

        public IRepository<MedicationReminders> MedicationReminders =>
            _medicationReminders ??= new Repository<MedicationReminders>(_context);

        public IRepository<UserMedications> UserMedications =>
            _userMedications ??= new Repository<UserMedications>(_context);

        public IRepository<UserRelationships> UserRelationships =>
            _userRelationships ??= new Repository<UserRelationships>(_context);

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
