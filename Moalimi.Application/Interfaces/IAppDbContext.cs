using Microsoft.EntityFrameworkCore;
using Moalimi.Domain.Entites;

namespace Moalimi.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<PaymentRecord> Payments { get; }
    DbSet<WhatsAppLog> WhatsAppLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}