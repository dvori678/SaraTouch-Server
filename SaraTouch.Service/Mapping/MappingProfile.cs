using AutoMapper;
using SaraTouch.Core.DTOs.Appointments;
using SaraTouch.Core.DTOs.Auth;
using SaraTouch.Core.DTOs.Tags;
using SaraTouch.Core.DTOs.TimeSlots;
using SaraTouch.Core.DTOs.Treatments;
using SaraTouch.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaraTouch.Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Tags
            CreateMap<Tag, TagResponse>();
            CreateMap<TagRequest, Tag>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Treatments, o => o.Ignore());

            // Treatments
            CreateMap<Treatment, TreatmentResponse>();   // Tags ממופות אוטומטית דרך Tag -> TagResponse
            CreateMap<TreatmentRequest, Treatment>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.Tags, o => o.Ignore())          // את התגיות ה-Service טוען לפי TagIds
                .ForMember(d => d.Appointments, o => o.Ignore());

            // TimeSlots
            CreateMap<TimeSlot, TimeSlotResponse>();

            // Appointments: מבנה מקונן הופך ל-DTO שטוח
            CreateMap<Appointment, AppointmentResponse>()
                .ForMember(d => d.StartUtc, o => o.MapFrom(s => s.TimeSlot.StartUtc))
                .ForMember(d => d.EndUtc, o => o.MapFrom(s => s.TimeSlot.EndUtc))
                .ForMember(d => d.TreatmentName, o => o.MapFrom(s => s.Treatment.Name))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Treatment.Price))
                .ForMember(d => d.TreatmentTags, o => o.MapFrom(s => s.Treatment.Tags.Select(t => t.Name)))
                .ForMember(d => d.ClientName, o => o.MapFrom(s => s.User.FullName));

            // Auth: הסיסמה והתפקיד לא מגיעים מהלקוח, ה-Service קובע אותם
            CreateMap<RegisterRequest, User>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Role, o => o.Ignore())
                .ForMember(d => d.Appointments, o => o.Ignore());
        }
    }
}
