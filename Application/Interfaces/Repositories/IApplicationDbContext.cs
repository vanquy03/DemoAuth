using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Application.Interfaces.Repositories
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> ApplicationUsers { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}