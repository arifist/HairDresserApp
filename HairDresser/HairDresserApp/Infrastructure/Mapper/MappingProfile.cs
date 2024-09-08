using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Repositories.Config;

namespace StoreApp.Infrastructure.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ReservationDtoForInsertion, Reservation>();
            CreateMap<UserDtoForCreation, AppUser>()
                  .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName)); // FullName'ý maple

            CreateMap<UserDtoForUpdate, AppUser>().ReverseMap();
            CreateMap<AppUser, UserDto>();
            CreateMap<WorkTimeDtoForUpdate, WorkTime>();
            CreateMap<WorkTimeDto, WorkTime>();


        }
    }
}