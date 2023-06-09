﻿using Database.Entities;

namespace PetShare.Models.Reports;

public sealed class Report
{
    public required Guid Id { get; init; }

    public required Guid TargetId { get; init; }

    public required string Message { get; init; }

    public required ReportedEntityType TargetType { get; init; }

    public required ReportState State { get; init; }

    public required DateTime CreationTime { get; init; }

    public static Report FromRequest(ReportRequest request)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            TargetId = request.TargetId,
            Message = request.Message,
            TargetType = Enum.Parse<ReportedEntityType>(request.ReportType, true),
            State = ReportState.New,
            CreationTime = DateTime.Now
        };
    }

    public ReportResponse ToResponse()
    {
        return new ReportResponse
        {
            Id = Id,
            TargetId = TargetId,
            Message = Message,
            ReportType = TargetType.ToString(),
            State = State.ToString()
        };
    }

    public static Report FromEntity(ReportEntity entity)
    {
        return new Report
        {
            Id = entity.Id,
            TargetId = entity.TargetId,
            Message = entity.Message,
            State = entity.State,
            TargetType = entity.TargetType,
            CreationTime = entity.CreationTime
        };
    }

    public ReportEntity ToEntity()
    {
        return new ReportEntity
        {
            Id = Id,
            TargetId = TargetId,
            Message = Message,
            State = State,
            TargetType = TargetType,
            CreationTime = CreationTime
        };
    }
}
