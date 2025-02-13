﻿using EVM.Data;
using EVM.Services.Exceptions;
using EVM.Services.Extensions;
using EVM.Services.Features.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EVM.Services.Features.Event.Commands;

public class DeleteEventCommandHandler
    (ILogger<DeleteEventCommandHandler> _logger, AppDbContext _appDbContext, IHttpContextAccessor _httpContextAccessor, IAuthorizationService _authorizationService)
{
    private readonly HttpContext _httpContext = _httpContextAccessor.HttpContext ?? throw new MissingHttpContextException();

    public async Task<ApiResponse<BaseResponse>> Handle(Guid eventId, CancellationToken cancellationToken)
    {
        await _authorizationService.CanPerformActionAsync(_httpContext.User, "Read", "Event");

        var userId = _httpContext.User?.GetId()
            ?? throw new UserNotFoundException();

        var user = await _appDbContext.Users
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UserNotFoundException();

        if (user.Role != Data.Enums.UserRole.Organizer)
        {
            user.Role = Data.Enums.UserRole.Organizer;
        }

        var deletedRows = await _appDbContext.Events
            .Where(x => x.Id == eventId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows == 0)
        {
            throw new BaseCustomException("Couldn't delete the data in the database. Please try again later!", HttpStatusCode.InternalServerError);
        }

        _logger.LogInformation("Event with ID: {eventId} was deleted successfully!", eventId);
        return new(new() { Id = eventId });
    }
}
