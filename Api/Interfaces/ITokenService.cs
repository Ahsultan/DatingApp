using Api.Entities;

public interface ITokenService {
    string CreateToken(AppUser user);
}