using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IReservationRepository: IRepositoryBase<Reservation>
    {
        void CreateOneReservation(Reservation reservation);
        IQueryable<Reservation> GetAllReservations(bool trackChanges);

        Reservation? GetOneReservation(int id, bool trackChanges);
        void DeleteOneReservation(Reservation reservation);
        void UpdateOneReservation(Reservation entity);

        Task<List<Reservation>> GetReservationsByUserIdAsync(string userId);

        Task<List<Reservation>> GetPastReservationsAsync(DateTime date);
        Task DeleteReservationsAsync(List<Reservation> reservations);

        Task<Reservation?> GetOneReservationAsync(int id, bool trackChanges);

    }
}
