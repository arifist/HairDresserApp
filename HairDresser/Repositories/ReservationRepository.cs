using Entities.Dtos;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ReservationRepository : RepositoryBase<Reservation>, IReservationRepository
    {
        private readonly List<Reservation> _reservations = new List<Reservation>();

        public ReservationRepository(RepositoryContext context) : base(context)
        {
        }


		public void CreateOneReservation(Reservation reservation) =>Create(reservation);

        public void DeleteOneReservation(Reservation reservation) => Remove(reservation);

        public IQueryable<Reservation> GetAllReservations(bool trackChanges) => FindAll(trackChanges);

        public Reservation? GetOneReservation(int id, bool trackChanges)
        {
            return FindByCondition(r => r.ReservationId.Equals(id), trackChanges);
        }

		public void UpdateOneReservation(Reservation entity) => Update(entity);




    }
}

