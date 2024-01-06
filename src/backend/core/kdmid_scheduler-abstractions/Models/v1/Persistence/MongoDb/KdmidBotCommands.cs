﻿using KdmidScheduler.Abstractions.Interfaces.Models;

using MongoDB.Bson;

using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;

namespace KdmidScheduler.Abstractions.Models.v1.Persistence.MongoDb;

public sealed class KdmidBotCommands : IKdmidBotCommand, IPersistentNoSql
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string ChatId { get; init; } = null!;
    public BotCommand Command { get; set; } = null!;
    public string DocumentVersion { get; set; } = "1.0.0";
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}
