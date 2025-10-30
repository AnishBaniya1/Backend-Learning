using System;
using System.Collections.Concurrent;
using Backend.Data;
using Backend.DTOs;
using Backend.Extensions;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs;

public class ChatHub(UserManager<AppUser> userManager, AppDbContext context) : Hub
{
    public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var receiverId = httpContext?.Request.Query["receiverId"].ToString();
        var userName = Context.User!.Identity!.Name;
        var currentUser = await userManager.FindByNameAsync(userName!);
        var connectionId = Context.ConnectionId;

        if (onlineUsers.ContainsKey(userName!))
        {
            onlineUsers[userName!].ConnectionId = connectionId;
        }
        else
        {
            var user = new OnlineUserDto
            {
                ConnectionId = connectionId,
                UserName = userName,
                ProfilePicture = currentUser!.ProfileImage,
                FullName = currentUser!.FullName
            };

            onlineUsers.TryAdd(userName!, user);
            await Clients.AllExcept(connectionId).SendAsync("Notify", currentUser);
        }
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }

    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        var username = Context.User!.GetUserName();
        var onlineUsersSet = new HashSet<string>(onlineUsers.Keys);
    }
}
