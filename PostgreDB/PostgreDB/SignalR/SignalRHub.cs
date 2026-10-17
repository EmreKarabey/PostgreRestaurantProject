using Application.Features.Reservation.Queries.GetActiveList;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace PostgreDB.SignalR
{
    public class SignalRHub : Hub
    {
        private readonly IMediator _mediator;

        public SignalRHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task ViewActiveReservation()
        {
            var list = await _mediator.Send(new GetActiveReservationTableListQuery());
            await Clients.All.SendAsync("GetActiveTables", list);
        }

        public static async Task SendActiveTablesAsync(IHubContext<SignalRHub> hubContext, IMediator mediator)
        {
            var list = await mediator.Send(new GetActiveReservationTableListQuery());
            await hubContext.Clients.All.SendAsync("GetActiveTables", list);
        }
    }
}
