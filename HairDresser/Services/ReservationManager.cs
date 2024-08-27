using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ReservationManager :IReservationService
    {

        private readonly IRepositoryManager _manager;
        private readonly IMapper _mapper;


        public ReservationManager(IRepositoryManager manager, IMapper mapper)
        {
            _manager = manager;
            _mapper = mapper;
        }



		public void CreateReservation(ReservationDtoForInsertion reservationDto)
        {
            Reservation reservation = _mapper.Map<Reservation>(reservationDto);
            _manager.Reservation.Create(reservation);
            _manager.Save();
        }



        public void DeleteOneReservation(int id)
        {
            Reservation reservation = GetOneReservation(id, false);
            if (reservation is not null)
            {
                _manager.Reservation.DeleteOneReservation(reservation);
                _manager.Save();
            }
        }

        public IEnumerable<Reservation> GetAllReservations(bool trackChanges)
        {
            return _manager.Reservation.GetAllReservations(trackChanges);
        }



        public IEnumerable<Reservation> GetLastestReservations(int n, bool trackChanges)
        {
            return _manager
                .Reservation
                .FindAll(trackChanges)
                .OrderByDescending(rsr => rsr.ReservationId)
                .Take(n);
        }

        public Reservation? GetOneReservation(int id, bool trackChanges)
        {
            var reservation = _manager.Reservation.GetOneReservation(id, trackChanges);
            if (reservation is null)
                throw new Exception("Reservation not found!");
            return reservation;
        }

        public ReservationDtoForUpdate GetOneReservationForUpdate(int id, bool trackChanges)
        {
            var reservation = GetOneReservation(id, trackChanges);
            var reservationDto = _mapper.Map<ReservationDtoForUpdate>(reservation);
            return reservationDto;
        }


        public void UpdateOneReservation(ReservationDtoForUpdate reservationDto)
        {
            var entity = _mapper.Map<Reservation>(reservationDto);
            _manager.Reservation.UpdateOneReservation(entity);
            _manager.Save();
        }


        //public bool IsReservationSlotAvailable(DateTime reservationDate)
        //{
        //	var existingReservation = _manager.Reservation.FindByCondition(
        //		r => r.ReservationDate == reservationDate,
        //		trackChanges: false
        //	);

        //	return existingReservation == null; // Null ise rezervasyon yapılabilir
        //}

        public bool IsReservationSlotAvailable(DateTime reservationDate, string hairCutType)
        {
            // Belirtilen tarihteki rezervasyonu kontrol et
            var existingReservation = _manager.Reservation.FindByCondition(
                r => r.ReservationDate == reservationDate,
                trackChanges: false
            );

            // Eğer HairCutType "saç ve sakal" ise ek kontrol yapılır
            if (hairCutType == "Saç ve Sakal")
            {
                // 30 dakika sonrasını kontrol et
                var reservationAfter30Minutes = _manager.Reservation.FindByCondition(
                    r => r.ReservationDate == reservationDate.AddMinutes(30),
                    trackChanges: false
                );

                // Eğer 30 dakika sonrası doluysa, false dön
                if (reservationAfter30Minutes != null)
                {
                    return false;
                }
            }

            // Eğer HairCutType "Saç" veya "Sakal" ise
            if (true)
            {
                // 30 dakika öncesini kontrol et
                var reservationBefore30Minutes = _manager.Reservation.FindByCondition(
                    r => r.ReservationDate == reservationDate.AddMinutes(-30) && r.HairCutTypes == "Saç ve Sakal",
                    trackChanges: false
                );

                // Eğer 30 dakika öncesi "Saç ve Sakal" doluysa, false dön
                if (reservationBefore30Minutes != null)
                {
                    return false;
                }
            }
            // Aynı saat veya 30 dakika sonrası boşsa true döner
            return existingReservation == null;
        }



    }
}
