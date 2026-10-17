using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Repositories
{
    public interface ITagRepository
    {
        Task<List<Tag>> GetAllAsync(CancellationToken ct = default);
        Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Tag>> GetByIdsForUpdateAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
        void Add(Tag tag);
    }
}
