namespace YarnBot.Models;

public class User
{
    public Guid ClientId {get; set;}
    public long ChatId {get; set;}
    public string Name {get; set;} = null!;
    public string Phone {get; set;} = null!;
    public DateTime UpdatedAt {get; set;}
    public List<Promocode> Promocodes {get; set;} = [];
}

public class Promocode
{
    public Guid ClientId {get; set;}
    public Guid PromoId {get; set;}
    public string Code {get; set;} = null!;
    public string Description {get; set;} = null!;
    public DateTime ExpiresAt {get; set;}
    public bool Followuped {get; set;}
    public User? User {get; set;}
}