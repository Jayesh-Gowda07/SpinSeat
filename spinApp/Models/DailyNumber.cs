namespace spinApp.Models;
using System;


public class DailyNumber
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Number { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
}
