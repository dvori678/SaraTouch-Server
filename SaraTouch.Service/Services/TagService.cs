using AutoMapper;
using Microsoft.Extensions.Logging;
using SaraTouch.Core.DTOs.Tags;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Service.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tags;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TagService> _logger;

        public TagService(ITagRepository tags, IUnitOfWork unitOfWork, IMapper mapper, ILogger<TagService> logger)
        {
            _tags = tags;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<TagResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var tags = await _tags.GetAllAsync(ct);
            return _mapper.Map<List<TagResponse>>(tags);
        }

        public async Task<Result<TagResponse>> CreateAsync(TagRequest request, CancellationToken ct = default)
        {
            var name = request.Name.Trim();

            // ולידציה עסקית: תלויה במצב הנתונים, ולכן היא כאן ולא ב-Controller
            if (await _tags.NameExistsAsync(name, ct))
                return Result<TagResponse>.Conflict($"תגית בשם '{name}' כבר קיימת");

            var tag = new Tag { Name = name };
            _tags.Add(tag);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Tag {TagId} created", tag.Id);
            return Result<TagResponse>.Success(_mapper.Map<TagResponse>(tag));
        }
    }
}
