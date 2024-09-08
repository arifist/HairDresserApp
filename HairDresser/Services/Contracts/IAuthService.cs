using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Services.Contracts
{
    public interface IAuthService
    {
        IEnumerable<IdentityRole> Roles { get;}
        IEnumerable<AppUser> GetAllUsers();
        Task<AppUser> GetOneUser(string userName);
        Task<UserDtoForUpdate> GetOneUserForUpdate(string userName);
        Task<IdentityResult> CreateUser(UserDtoForCreation userDto);
        Task Update(UserDtoForUpdate userDto);
        Task<IdentityResult> ResetPassword(ResetPasswordDto model);
        Task<IdentityResult> DeleteOneUser(string userName);

        Task<UserDto> GetUserDtoAsync(ClaimsPrincipal user);
        Task<List<Reservation>> GetReservationsByUserAsync(ClaimsPrincipal user);

    }
}