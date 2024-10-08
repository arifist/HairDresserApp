﻿using AutoMapper;
using Entities.Dtos;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
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
            var reservation = new Reservation
            {
                ReservationDay = reservationDto.ReservationDay,
                ReservationHour = reservationDto.ReservationHour,
                ReservationDate = reservationDto.ReservationDate,
                ReservationMessage = reservationDto.ReservationMessage,
                ReservationName = reservationDto.ReservationName,
                HairCutTypes = reservationDto.HairCutTypes,
                UserId = reservationDto.UserId, // Kullanıcı ID
                UserName = reservationDto.UserName // Kullanıcı Adı
            };

            _manager.Reservation.CreateOneReservation(reservation);
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




        public IEnumerable<Reservation> GetReservationsByDay(DateTime day, bool trackChanges)
        {
            var reservations = _manager.Reservation.FindAll(trackChanges)
                .Where(r => r.ReservationDate.Date == day.Date); // Filtreleme işlemi

            return reservations.ToList(); // Listeye dönüştür
        }

        public async Task<List<Reservation>> GetReservationsByUserIdAsync(string userId)
        {
            return await _manager.Reservation.GetReservationsByUserIdAsync(userId);
        }

        public void DeletePastReservations()
        {
            var pastReservations = _manager.Reservation
                .FindAll(false)
                .Where(r => r.ReservationDate < DateTime.Now)
                .ToList();

            foreach (var reservation in pastReservations)
            {
                _manager.Reservation.DeleteOneReservation(reservation);
            }
            _manager.Save();
        }

        public async Task DeletePastReservationsAsync()
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-1);
            var pastReservations = await _manager.Reservation.GetPastReservationsAsync(thresholdDate);

            // Geçmiş rezervasyonları tespit et, örneğin bugünden önceki tüm rezervasyonlar
            //var pastReservations = await _manager.Reservation.GetPastReservationsAsync(DateTime.Now);

            // Eğer silinecek rezervasyon yoksa işlemi sonlandır
            if (pastReservations == null || pastReservations.Count == 0)
            {
                // Silinecek kayıt yok, loglama yapabilir veya direkt return edebilirsin.
                return;
            }

            // Bulunan geçmiş rezervasyonları sil
            await _manager.Reservation.DeleteReservationsAsync(pastReservations);
        }


        public async Task<bool> DeleteOneReservationByIdAsync(int reservationId)
        {
            // ID ile rezervasyonu çekiyoruz
            var reservation = await _manager.Reservation.GetOneReservationAsync(reservationId, trackChanges: false);

            // Rezervasyon bulunmazsa false döndürülüyor
            if (reservation == null)
                return false;

            // Rezervasyonu sil ve değişiklikleri kaydet
            _manager.Reservation.DeleteOneReservation(reservation);
            await _manager.SaveAsync();

            // Başarılı bir şekilde silindi
            return true;
        }
    }
}
