using System;

internal record ExposureStat
{
    public required DateOnly Date { get; init; }
    public required int Count { get; init; }
}
