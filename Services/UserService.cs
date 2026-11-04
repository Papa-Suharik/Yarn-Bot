using YarnBot.Dto;
using YarnBot.Models;

namespace YarnBot.Services;

public static class UserService
{
    public static List<Promocode> CreatePromos(List<PromocodeDto> codes)
    {
        var kyivTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        return codes.Select(x => 
        {
            var expiresLocal = x.ExpiresAt.Date.AddDays(1).AddTicks(-1);
            
            return new Promocode
            {
                ClientId = x.ClientId,
                PromoId = x.IssuedPromoId,
                Code = x.Code, 
                Description = x.Description, 
                ExpiresAt = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(expiresLocal, DateTimeKind.Unspecified), kyivTz) 
            };
        }).ToList();
    }
    public static List<Promocode> SyncPromocodes(this User user, List<Promocode> newPromos)
    {
        var existingIds = user.Promocodes.Select(x => x.PromoId).ToHashSet();
        var newIds = newPromos.Select(x => x.PromoId).ToHashSet();

        foreach(var promocode in newPromos)
        {
            if(!existingIds.Contains(promocode.PromoId))
            {
                user.Promocodes.Add(promocode);
            }
        }

        return user.Promocodes.Where(x => !newIds.Contains(x.PromoId)).ToList();
    }
    public static User CreateUser(ClientDto dto, long chatId)
    {
        var promcodes = CreatePromos(dto.Promocodes);
        return new User
        {
          ClientId = dto.ClientId,
          ChatId = chatId,
          Name = dto.Name,
          Phone = dto.Phone,
          UpdatedAt = DateTime.UtcNow,
          Promocodes = promcodes 
        };
    }
}