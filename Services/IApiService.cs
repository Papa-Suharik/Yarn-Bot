using YarnBot.Dto;

namespace YarnBot.Services;

public interface IApiService
{
    Task<ClientDto?> GetClientByPhoneAsync(string phoneNumber, CancellationToken cancellationToken);
    Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ExpiringPromoResponseDto?> GetExpiringPromosAsync(ExpiringPromoRequestDto request, CancellationToken cancellationToken);
}