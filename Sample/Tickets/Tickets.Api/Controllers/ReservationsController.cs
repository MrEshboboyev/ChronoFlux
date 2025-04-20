using Core.Commands;
using Core.Ids;
using Core.Queries;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Requests;
using Tickets.Api.Responses;
using Tickets.Reservations.CancellingReservation;
using Tickets.Reservations.ChangingReservationSeat;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Reservations.CreatingTentativeReservation;
using Tickets.Reservations.GettingReservationAtVersion;
using Tickets.Reservations.GettingReservationById;
using Tickets.Reservations.GettingReservationHistory;
using Tickets.Reservations.GettingReservations;

namespace Tickets.Api.Controllers;

[Route("api/[controller]")]
public class ReservationsController(
    ICommandBus commandBus,
    IQueryBus queryBus,
    IIdGenerator idGenerator)
    : Controller
{
    [HttpGet("{id}")]
    public Task<ReservationDetails> Get(Guid id) =>
        queryBus.QueryAsync<GetReservationById, ReservationDetails>(new GetReservationById(id));

    [HttpGet]
    public async Task<PagedListResponse<ReservationShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var pagedList = await queryBus.QueryAsync<GetReservations, IPagedList<ReservationShortInfo>>(GetReservations.Create(pageNumber, pageSize));

        return PagedListResponse.From(pagedList);
    }


    [HttpGet("{id}/history")]
    public async Task<PagedListResponse<ReservationHistory>> GetHistory(Guid id)
    {
        var pagedList = await queryBus.QueryAsync<GetReservationHistory, IPagedList<ReservationHistory>>(GetReservationHistory.Create(id));

        return PagedListResponse.From(pagedList);
    }

    [HttpGet("{id}/versions")]
    public Task<ReservationDetails> GetVersion(Guid id, [FromQuery] GetReservationDetailsAtVersion request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return queryBus.QueryAsync<GetReservationAtVersion, ReservationDetails>(GetReservationAtVersion.Create(id, request.Version));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTentative([FromBody] CreateTentativeReservationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservationId = idGenerator.New();

        var command = CreateTentativeReservation.Create(
            reservationId,
            request.SeatId
        );

        await commandBus.SendAsync(command);

        return Created($"/api/Reservations/{reservationId}", reservationId);
    }


    [HttpPost("{id}/seat")]
    public async Task<IActionResult> ChangeSeat(Guid id, [FromBody] ChangeSeatRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = ChangeReservationSeat.Create(
            id,
            request.SeatId
        );

        await commandBus.SendAsync(command);

        return Ok();
    }

    [HttpPut("{id}/confirmation")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var command = ConfirmReservation.Create(
            id
        );

        await commandBus.SendAsync(command);

        return Ok();
    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var command = CancelReservation.Create(
            id
        );

        await commandBus.SendAsync(command);

        return Ok();
    }
}
