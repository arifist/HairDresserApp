using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IReservationService
    {

        void CreateReservation(ReservationDtoForInsertion reservationDto);
        IEnumerable<Reservation> GetAllReservations(bool trackChanges);
        IEnumerable<Reservation> GetLastestReservations(int n, bool trackChanges);
        //IEnumerable<Reservation> GetAllReservationsWithDetails(ReservationRequestParameters p);
        //IEnumerable<Reservation> GetShowcaseReservations(bool trackChanges);
        Reservation? GetOneReservation(int id, bool trackChanges);
        void UpdateOneReservation(ReservationDtoForUpdate reservationDto);
        void DeleteOneReservation(int id);
        ReservationDtoForUpdate GetOneReservationForUpdate(int id, bool trackChanges);
		bool IsReservationSlotAvailable(DateTime reservationDate, string hairCutType);

        IEnumerable<Reservation> GetReservationsByDay(DateTime day, bool trackChanges);

        Task<List<Reservation>> GetReservationsByUserIdAsync(string userId);



    }
}
