﻿using EVM.Data;
using EVM.Services.Exceptions;
using EVM.Services.Extensions;
using EVM.Services.Features.Event.Models.Responses;
using EVM.Services.Features.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVM.Services.Features.Event.Query;

public class GetAllEventsQueryHandler
  (ILogger<GetAllEventsQueryHandler> _logger, AppDbContext _appDbContext, IHttpContextAccessor _httpContextAccessor, IAuthorizationService _authorizationService)
{
    private readonly HttpContext _httpContext = _httpContextAccessor.HttpContext ?? throw new MissingHttpContextException();

    public async Task<ApiResponse<List<GetEventResponse>>> Handle(CancellationToken cancellationToken)
    {
        await _authorizationService.CanPerformActionAsync(_httpContext.User, "Read", "Event");
        var userId = _httpContext.User.GetId() ?? throw new UserNotFoundException();

        var events = await _appDbContext.Events
            .Where(x => x.UserId == userId)
            .Select(x => new GetEventResponse
            {
                Id = x.Id,
                Name = x.Title,
                Description = x.Description,
                ETask = x.EventTasks.ToList(),
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken) ?? throw new EntityNotFoundException("Event");

        _logger.LogInformation("All events for the user with ID: {UserId} have been successfully retrieved and sent!", userId);
        return new(events);
    }
}
