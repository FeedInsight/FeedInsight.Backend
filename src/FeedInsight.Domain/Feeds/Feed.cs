using FeedInsight.Domain.Common.Models;
using FeedInsight.Domain.Feeds.Enums;
using FeedInsight.Domain.Feeds.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Domain.Feeds;

public class Feed : Entity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Using the strongly-typed Value Object
    public FeedUrl? Url { get; set; }

    // Using the Enum
    public FeedStatus Status { get; set; } = FeedStatus.Active;

    // You can still add small helper methods here if you want!
    // But it's not strictly required like it is in DDD.
}