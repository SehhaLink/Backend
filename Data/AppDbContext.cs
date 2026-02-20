using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sehha360.Models;

namespace Sehha360.Data
{
    public class AppDbContext:IdentityDbContext<AppUser>
    {
        public DbSet<DoctorPatient> DoctorPatients { get; set; }
        public DbSet<MedicalReport> MedicalReports { get; set; }
        public DbSet<UserMedications> UserMedications { get; set; }
        public DbSet<UserRelationships> UserRelationships { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<MedicationReminders> MedicationReminders { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
