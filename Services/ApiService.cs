using System.Net;
using System.Net.Http.Json;
using YarnBot.Dto;

namespace YarnBot.Services;

public class ApiService(HttpClient httpClient, ILogger<ApiService> logger, IConfiguration configuration) : IApiService
{
    public async Task<ClientDto?> GetClientByPhoneAsync(string phoneNumber, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Requesting client info with {phoneNumber} from 1C", phoneNumber);

            var endpoint = configuration["OneC:ByPhoneEndpoint"];
            var response = await httpClient.GetAsync($"{endpoint}{Uri.EscapeDataString(phoneNumber)}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var client = await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct);

            return client;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get client with {phoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Requesting client info with {id} from 1C", id);

            var endpoint = configuration["OneC:ByIdEndpoint"];
            var response = await httpClient.GetAsync($"{endpoint}{id}", ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var client = await response.Content.ReadFromJsonAsync<ClientDto>(cancellationToken: ct);

            return client;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get client with {id}", id);
            throw;
        }
    }
    public async Task<ExpiringPromoResponseDto?> GetExpiringPromosAsync(ExpiringPromoRequestDto request, CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Requesting expiring promos");

            logger.LogInformation("Request payload: {@Request}", request);

            var endpoint = configuration["OneC:ExpPromosEndpoint"];
            var response = await httpClient.PostAsJsonAsync($"{endpoint}", request, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogInformation("Status: {StatusCode}, Body: {Body}", response.StatusCode, body);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ExpiringPromoResponseDto>(cancellationToken: ct);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to get expiring promocodes to followup");
            throw;
        }
    }
}