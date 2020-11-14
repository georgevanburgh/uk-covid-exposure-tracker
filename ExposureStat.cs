using System;

internal record ExposureStat
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}