using System;

internal record ExposureStat
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}