using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Application.Features.Reservation.Command.Create
{
    public class CreateReservationCommandValidator:AbstractValidator<CreateReservationCommand>
    {
        public CreateReservationCommandValidator()
        {
            RuleFor(n => n.FirstName).NotEmpty().WithMessage("FirstName cannot be blank");
            RuleFor(n => n.LastName).NotEmpty().WithMessage("LastName cannot be blank");
            RuleFor(n => n.Email).NotEmpty().WithMessage("Email cannot be blank");
            RuleFor(n => n.PhoneNumber).NotEmpty().WithMessage("PhoneNumber cannot be blank");
            RuleFor(n => n.ReservationDate).NotEmpty().WithMessage("ReservationDate cannot be blank");
            RuleFor(n => n.NumberOfPeople).NotEmpty().WithMessage("NumberOfPeople cannot be blank");
        }
    }
}
