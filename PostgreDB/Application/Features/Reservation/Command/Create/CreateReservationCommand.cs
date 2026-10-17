using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Services.Cache;
using Application.Services.Email;
using Application.Services.Repository;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Reservation.Command.Create
{
    public class CreateReservationCommand : IRequest<CreateReservationResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime ReservationDate { get; set; }
        public int NumberOfPeople { get; set; }

        public int? TableNumber { get; set; }

    }

    public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, CreateReservationResponse>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly IEmailSender _emailSender;

        public CreateReservationHandler(IReservationRepository reservationRepository, IMapper mapper, ICacheService cacheService, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender)
        {
            _reservationRepository = reservationRepository;
            _mapper = mapper;
            _cacheService = cacheService;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }

        async Task<CreateReservationResponse> IRequestHandler<CreateReservationCommand, CreateReservationResponse>.Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {

            string emailBody = $@"
<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <title>Rezervasyon Onayı</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f4f4f4; padding: 40px 0;'>
        <tr>
            <td align='center'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);'>
                    <tr>
                        <td align='center' style='background-color: #2c3e50; padding: 35px 20px;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 24px; letter-spacing: 2px;'>RESTORAN ADI</h1>
                            <p style='color: #c9a131; margin: 8px 0 0 0; font-size: 14px;'>Rezervasyon Onay Belgesi</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 40px 30px; color: #333333;'>
                            <h2 style='color: #2c3e50; font-size: 20px; margin-top: 0;'>Sayın {request.FirstName} {request.LastName},</h2>
                            <p style='font-size: 15px; line-height: 1.6; color: #555555;'>
                                Restoranımızda yerinizi ayıttığınız için teşekkür ederiz. Rezervasyon detaylarınız aşağıda yer almaktadır.
                            </p>
                            <table border='0' cellpadding='0' cellspacing='0' width='100%' style='background-color: #f9f9f9; border-left: 4px solid #c9a131; margin: 25px 0; border-radius: 4px;'>
                                <tr>
                                    <td style='padding: 20px;'>
                                        <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                            <tr>
                                                <td style='padding: 8px 0; font-size: 14px; color: #7f8c8d;' width='40%'><strong>Masa Numarası:</strong></td>
                                                <td style='padding: 8px 0; font-size: 14px; color: #2c3e50;' width='60%'>Masa {request.TableNumber}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; font-size: 14px; color: #7f8c8d;'><strong>Tarih ve Saat:</strong></td>
                                                <td style='padding: 8px 0; font-size: 14px; color: #2c3e50;'>{request.ReservationDate.AddHours(3):dd.MM.yyyy HH:mm}</td>
                                            </tr>
                                            <tr>
                                                <td style='padding: 8px 0; font-size: 14px; color: #7f8c8d;'><strong>Kişi Sayısı:</strong></td>
                                                <td style='padding: 8px 0; font-size: 14px; color: #2c3e50;'>{request.NumberOfPeople} Kişi</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                            <p style='font-size: 14px; color: #666666;'>Sizi ağırlamaktan mutluluk duyacağız!</p>
                        </td>
                    </tr>
                    <tr>
                        <td align='center' style='background-color: #fdfdfd; border-top: 1px solid #eeeeee; padding: 20px;'>
                            <p style='font-size: 12px; color: #95a5a6; margin: 0;'>&copy; 2026 Restoran Adı. Tüm hakları saklıdır.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";


            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                throw new UnauthorizedAccessException("Rezervasyon oluşturmak için geçerli bir oturumla giriş yapmalısınız.");
            }

            if (request.TableNumber > 0)
            {
                var cacheKey = $"TableLock_{request.TableNumber}_{request.ReservationDate.ToUniversalTime():yyyyMMddHHmm}";

                try
                {
                    var entity = _mapper.Map<Domain.Entities.Reservation>(request);
                    entity.UserId = currentUserId;

                    entity.CreatedAt = DateTime.UtcNow;

                    await _reservationRepository.AddAsync(entity);

                    var result = _mapper.Map<CreateReservationResponse>(entity);

                    if (result != null)
                        await _emailSender.SendEmailAsync(
                          email: request.Email,
                          subject: "Rezervasyonunuz Başarıyla Onaylandı",
                          htmlMessage: emailBody
                      );

                    return result;
                }
                finally
                {
                    await _cacheService.RemoveAsync(cacheKey);
                }
            }
            else
            {
                var entity = _mapper.Map<Domain.Entities.Reservation>(request);
                entity.UserId = currentUserId;

                entity.CreatedAt = DateTime.UtcNow;

                await _reservationRepository.AddAsync(entity);

                var result = _mapper.Map<CreateReservationResponse>(entity);

                if (result != null)
                    await _emailSender.SendEmailAsync(
                      email: request.Email,
                      subject: "Rezervasyonunuz Başarıyla Onaylandı",
                      htmlMessage: emailBody
                  );



                return result;
            }
        }
    }
}
