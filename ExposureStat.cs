using System;

public class ExposureStat
{
    public DateTime Date { get; set; }
    public int Count { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ExposureStat stat &&
               Date == stat.Date &&
               Count == stat.Count;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Date, Count);
    }
}