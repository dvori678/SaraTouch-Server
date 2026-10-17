using SaraTouch.Core.DTOs.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Core.Interfaces.Services
{
    public interface ITagService
    {
        Task<List<TagResponse>> GetAllAsync(CancellationToken ct = default);
        Task<Result<TagResponse>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<TagResponse>> CreateAsync(TagRequest request, CancellationToken ct = default);
    }
}
