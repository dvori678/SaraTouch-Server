using AutoMapper;
using Microsoft.Extensions.Logging;
using SaraTouch.Core.Common;
using SaraTouch.Core.DTOs.Treatments;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Core.Interfaces.Repositories;
using SaraTouch.Core.Interfaces.Services;
using SaraTouch.Service.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SaraTouch.Service.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly ITreatmentRepository _treatments;
        private readonly ITagRepository _tags;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClock _clock;
        private readonly ILogger<TreatmentService> _logger;

        public TreatmentService(ITreatmentRepository treatments, ITagRepository tags, IUnitOfWork unitOfWork,
            IMapper mapper, IClock clock, ILogger<TreatmentService> logger)
        {
            _treatments = treatments;
            _tags = tags;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _clock = clock;
            _logger = logger;
        }

        public async Task<PagedResult<TreatmentResponse>> GetPagedAsync(TreatmentQuery query, CancellationToken ct = default)
        {
            var page = await _treatments.GetPagedAsync(query, ct);
            return page.MapItems<Treatment, TreatmentResponse>(_mapper);
        }

        public async Task<Result<TreatmentResponse>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var treatment = await _treatments.GetByIdAsync(id, ct);
            return treatment is null
                ? Result<TreatmentResponse>.NotFound("הטיפול לא נמצא")
                : Result<TreatmentResponse>.Success(_mapper.Map<TreatmentResponse>(treatment));
        }

        public async Task<Result<TreatmentResponse>> CreateAsync(TreatmentRequest request, CancellationToken ct = default)
        {
            var tags = await _tags.GetByIdsForUpdateAsync(request.TagIds, ct);
            if (tags.Count != request.TagIds.Distinct().Count())
                return Result<TreatmentResponse>.Validation("אחת או יותר מהתגיות לא קיימות");

            var treatment = _mapper.Map<Treatment>(request);
            foreach (var tag in tags)
                treatment.Tags.Add(tag);

            _treatments.Add(treatment);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Treatment {TreatmentId} created", treatment.Id);
            return Result<TreatmentResponse>.Success(_mapper.Map<TreatmentResponse>(treatment));
        }

        public async Task<Result> UpdateAsync(int id, TreatmentRequest request, CancellationToken ct = default)
        {
            var treatment = await _treatments.GetForUpdateAsync(id, ct);
            if (treatment is null)
                return Result.NotFound("הטיפול לא נמצא");

            var tags = await _tags.GetByIdsForUpdateAsync(request.TagIds, ct);
            if (tags.Count != request.TagIds.Distinct().Count())
                return Result.Validation("אחת או יותר מהתגיות לא קיימות");

            _mapper.Map(request, treatment);   // מעדכן את השדות של הישות הקיימת
            treatment.Tags.Clear();
            foreach (var tag in tags)
                treatment.Tags.Add(tag);

            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Treatment {TreatmentId} updated", id);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var treatment = await _treatments.GetForUpdateAsync(id, ct);
            if (treatment is null)
                return Result.NotFound("הטיפול לא נמצא");

            if (await _treatments.HasFutureAppointmentsAsync(id, _clock.UtcNow, ct))
                return Result.Conflict("לא ניתן למחוק טיפול שיש עליו תורים עתידיים");

            // מחיקה רכה: לתורים מהעבר יש הפניה לטיפול, אז לא מוחקים אותו מבסיס הנתונים, רק מסתירים
            treatment.IsActive = false;
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Treatment {TreatmentId} deactivated", id);
            return Result.Success();
        }
    }
}
