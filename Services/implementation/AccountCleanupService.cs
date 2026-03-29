using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sehha360.Models;

namespace Sehha360.Services.implementation
{
    public class AccountCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AccountCleanupService> _logger;

        public AccountCleanupService(IServiceProvider serviceProvider, ILogger<AccountCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Account Cleanup Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupDeactivatedAccountsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up deactivated accounts.");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Account Cleanup Service is stopping.");
        }

        private async Task CleanupDeactivatedAccountsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                
                var accountsToDelete = await userManager.Users
                    .Where(u => u.IsDeactivated && u.DeactivationDate != null && u.DeactivationDate < cutoffDate)
                    .ToListAsync();

                if (accountsToDelete.Any())
                {
                    _logger.LogInformation($"Found {accountsToDelete.Count} accounts to hard-delete.");

                    foreach (var user in accountsToDelete)
                    {
                        var result = await userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation($"Successfully deleted user: {user.Email}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to delete user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }
            }
        }
    }
}
