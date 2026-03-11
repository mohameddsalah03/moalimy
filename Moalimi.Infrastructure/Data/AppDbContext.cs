using Microsoft.EntityFrameworkCore;
using Moalimi.Application.Interfaces;
using Moalimi.Domain.Entites;

namespace Moalimi.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();
    public DbSet<WhatsAppLog> WhatsAppLogs => Set<WhatsAppLog>();
}