using AutoMapper;
using UserManagement.Domain.Entities;
using UserManagement.DTOs;


namespace UserManagement.Mapping
{

    using static UserManagement.Domain.Entities.Request;
    using DomainUser = UserManagement.Domain.Entities.User;

    public class MappingProfile : Profile
    {

        public MappingProfile() 
        {

            CreateMap<DomainUser, UserReadDto>();


            CreateMap<RegisterDto, DomainUser>()
                .ForMember(d => d.PasswordHash, o => o.Ignore());

            CreateMap<UserCreateDto, DomainUser>()
                .ForMember(d => d.PasswordHash, o => o.Ignore());

            CreateMap<DomainUser, UserListDto>();

            CreateMap<RequestCreateDto, Request>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src =>
                ParsePriorityOrDefault(src.Priority)));

            CreateMap<Request, RequestListDto>()
                .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.Name))
                .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.TechnicianName, o => o.MapFrom(s =>
                s.Technician == null ? null : (s.Technician.FirstName + " " + s.Technician.LastName)));

            CreateMap<Request, RequestDetailsDto>()
                .ForMember(d => d.StatusName, o => o.MapFrom(s => s.Status.Name))
                .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
                .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy.FirstName + " " + s.CreatedBy.LastName))
                .ForMember(d => d.TechnicianName, o => o.MapFrom(s =>
                s.Technician == null ? null : (s.Technician.FirstName + " " + s.Technician.LastName)));

        }

        private static RequestPriority ParsePriorityOrDefault(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return RequestPriority.Normal;
            return Enum.TryParse<RequestPriority>(input, true, out var value)
                ?value
                : RequestPriority.Normal;
        }
    }
}
