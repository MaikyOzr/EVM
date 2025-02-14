﻿namespace EVM.Services.Features.Event.Models.Requests;

public record CreateEventRequest
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public required string Location { get; set; }

    public required bool IsTicket { get; set; }

    public virtual CreateTicketRequest? TicketRequest { get; set; }
}
