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
            CreateMap<UserDtoForCreation, IdentityUser>();
            CreateMap<UserDtoForUpdate, IdentityUser>().ReverseMap();
            CreateMap<IdentityUser, UserDto>();
            CreateMap<WorkTimeDtoForUpdate, WorkTime>();


        }
    }
}