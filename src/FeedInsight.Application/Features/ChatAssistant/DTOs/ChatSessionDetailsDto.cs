using FeedInsight.Application.Features.ChatAssistant.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.ChatAssistant.DTOs;

public record ChatSessionDetailsDto(
    Guid Id,
    string Title,
    DateTime CreatedAt
);