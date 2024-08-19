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
        //IQueryable<Reservation> GetAllReservationsWithDetails(ProductRequestParameters p);
        //IQueryable<Reservation> GetShowcaseReservations(bool trackChanges);
        Reservation? GetOneReservation(int id, bool trackChanges);
        void DeleteOneReservation(Reservation reservation);
        void UpdateOneReservation(Reservation entity);
		//Task<Reservation> GetReservationByDateTimeAsync(DateTime date, string hour);

	}
}
