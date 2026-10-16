using Microsoft.EntityFrameworkCore;
using SaraTouch.Core.Exceptions;
using SaraTouch.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context) => _context = context;

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // תופסים רק את החריגה הספציפית, ומתרגמים אותה לחריגה של Core.
                // ככה שכבת Service לא צריכה להכיר את EF.
                throw new ConcurrencyConflictException("The record was modified by another user.", ex);
            }
        }
    }
}
