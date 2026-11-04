namespace YarnBot.Dto;

public record ClientDto(
    Guid ClientId,
    string Name,
    string Phone,
    List<PromocodeDto> Promocodes
);

public record PromocodeDto(
    Guid ClientId,
    Guid IssuedPromoId,
    string Code,
    string Description,
    DateTime ExpiresAt
);
public record ExpiringPromoRequestDto(
    List<Guid> IdsToFollow
);
public record ExpiringPromoResponseDto(
    List<ExpiringPromocodeDto> Promos
);
public record ExpiringPromocodeDto(
    Guid ClientId,
    Guid IssuedPromoId,
    string PromoCode,
    DateTime ExpirationDate
);