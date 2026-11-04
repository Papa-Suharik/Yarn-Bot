using Microsoft.EntityFrameworkCore;
using YarnBot.Models;
using YarnBot.Data;

namespace YarnBot.Repo;

public class UserRepo(ApplicationContext context)
{
    public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        await context.AddAsync(user, cancellationToken);
        return user;
    }
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
    public async Task<User?> FindUserByPhone(string phone, CancellationToken cancellationToken)
    {
        return await context.Users.Include(x => x.Promocodes.Where(x => x.ExpiresAt > DateTime.UtcNow)).FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken);
    }
    public async Task<User?> FindUserByClientId(Guid id, CancellationToken cancellationToken)
    {
        return await context.Users.Include(x => x.Promocodes.Where(x => x.ExpiresAt > DateTime.UtcNow)).FirstOrDefaultAsync(x => x.ClientId == id, cancellationToken);
    }
    public async Task<User?> FindUserByChatId(long chatId, CancellationToken cancellationToken)
    {
        return await context.Users.Include(x => x.Promocodes.Where(x => x.ExpiresAt > DateTime.UtcNow)).FirstOrDefaultAsync(x => x.ChatId == chatId, cancellationToken);
    }
    public void RemovePromocodes(List<Promocode> codes)
    {
        context.RemoveRange(codes);
    }
    public async Task<List<Guid>> GetPromoIdsToFollowupAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var maxExpiry = now.AddDays(3);
        return await context.Promocodes.Where(x => x.ExpiresAt >= now && x.ExpiresAt <= maxExpiry && !x.Followuped).Select(x => x.PromoId).ToListAsync(cancellationToken);
    }
}