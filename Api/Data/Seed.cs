using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Api;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

public class Seed 
{
    public static async Task SeedUsers(AppDataContext dataContext)
    {
        if(await dataContext.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

        foreach(var user in users)
        {
            using var hmac = new HMACSHA512();

            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("pa$$w0rd"));
            user.PasswordSalt = hmac.Key;

            dataContext.Users.Add(user);
        }

        await dataContext.SaveChangesAsync();
    }
}