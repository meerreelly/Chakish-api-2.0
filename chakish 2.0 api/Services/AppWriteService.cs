using System;
using System.Threading.Tasks;
using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using Microsoft.Extensions.Configuration;

namespace Repository.Services;

public class AppWriteService
{
    private readonly Client _client;
    private readonly Users _users;
    private readonly IConfiguration _configuration;
    public AppWriteService(IConfiguration config)
    {
        _configuration = config;
        _client = new Client()
            .SetEndpoint("https://cloud.appwrite.io/v1")
            .SetProject("67c1a0540031b05f473a")
            .SetKey("standard_549389ad7d7247160836719158184d3faab2fe3071e35899643f3610c121c1c1e3e21603898bc7bc71d40b515ebfeb9fe0ca2ea08d72d6f3e08e88cb31482d64cd0d7df91ba5904fb206f1d47c9c09d9c91200db62d81046a249c1ce91a65d8c32515ad30c0f1c2502d93b8d74cfa5917ff76e16cbef9350611571099074a5e6"); 
         _users = new Users(_client);
    }

    public async Task<bool> VerifyEmail(string userId)
    {
        try
        {
            await _users.UpdateEmailVerification(userId, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<User?> GetUser(string userId)
    {
        try
        {
            var user = await _users.Get(userId);
            return user;
        }catch
        {
            return null;
        }
    }
    

    public async Task<User?> ValidateJwt(string token)
    {
        var client = new Client()
            .SetEndpoint("https://cloud.appwrite.io/v1") 
            .SetProject("67c1a0540031b05f473a")
            .SetJWT(token);
        var account = new Account(client);

        try
        {
            var user = await account.Get();
            if(user == null) return null;
            return user;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}