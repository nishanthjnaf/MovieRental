using MovieRentalAPI.Models.DTOs;

namespace MovieRentalAPI.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(TokenPayloadDto payloadDto);

    }
}
