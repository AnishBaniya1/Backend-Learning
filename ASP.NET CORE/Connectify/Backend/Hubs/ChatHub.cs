using System;
using System.Collections.Concurrent;
using Backend.Data;
using Backend.DTOs;
using Backend.Extensions;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Hubs;

public class ChatHub(UserManager<AppUser> userManager, AppDbContext context) : Hub
{
    //ConcurrentDictionary → Thread-safe dictionary (safe to use in multi-threaded environments like SignalR hubs).
    //Key = username (string)
    //Value = OnlineUserDto (contains info like connection ID, full name, profile picture, etc.)
    //Keeps track of all currently connected users.Static Dictionary of online users
    public static readonly ConcurrentDictionary<string, OnlineUserDto> onlineUsers = new();

    //s automatically called when a client connects to the hub.
    public override async Task OnConnectedAsync()
    {
        //Get the HTTP context:Sometimes you need query parameters or headers when a client connects.
        var httpContext = Context.GetHttpContext();
        //Get receiverId query param:ptional, maybe used for redirecting messages
        var receiverId = httpContext?.Request.Query["receiverId"].ToString();
        //SignalR sets Context.User automatically if your app uses authentication.
        var userName = Context.User!.Identity!.Name;
        //Fetches the AppUser object from the database.
        var currentUser = await userManager.FindByNameAsync(userName!);
        //Get connection ID:Each SignalR client connection has a unique ID.
        var connectionId = Context.ConnectionId;

        //Update onlineUsers dictionary:
        if (onlineUsers.ContainsKey(userName!))
        {
            //If the user already exists in onlineUsers → update their connection ID (reconnect scenario).
            onlineUsers[userName!].ConnectionId = connectionId;
        }
        else //If new user doesn't exist → create OnlineUserDto 
        {
            var user = new OnlineUserDto
            {
                ConnectionId = connectionId,
                UserName = userName,
                ProfilePicture = currentUser!.ProfileImage,
                FullName = currentUser!.FullName
            };
            //add to dictionary.
            onlineUsers.TryAdd(userName!, user);
            //Notify all other users that this user is online.
            await Clients.AllExcept(connectionId).SendAsync("Notify", currentUser);
        }
        //Send the updated list of online users to all clients:Calls GetAllUsers()→ sends the list of users with their online/offline status.
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }

    public async Task LoadMessage(string recipientId, int pageNumber = 1)
    {
        int pageSize = 10;
        var username = Context.User!.Identity!.Name;
        var currentUser = await userManager.FindByNameAsync(username!);

        if (currentUser is null)
        {
            return;
        }
        List<MessageResponseDto> messages = await context.Messages.Where(x => x.ReceiverId == currentUser!.Id
        && x.SenderId == recipientId
        || x.SenderId == currentUser!.Id && x.ReceiverId == recipientId).OrderByDescending(x => x.CreatedDate)
        .Skip((pageNumber - 1) * pageSize).Take(pageSize).OrderBy(x => x.CreatedDate)
        .Select(x => new MessageResponseDto
        {
            Id = x.Id,
            Content = x.Content,
            SenderId = x.SenderId,
            ReceiverId = x.ReceiverId,
            CreatedDate = x.CreatedDate
        }).ToListAsync();
    }

    //Purpose: Handle sending a message from one user to another.
    public async Task SendMessage(MessageRequestDto message)
    {
        //Get sender username → from authenticated Context.User.
        var senderId = Context.User!.Identity!.Name;
        //Get recipient ID → from MessageRequestDto.
        var recipientId = message.ReceiverId;

        //Create new message → set sender, receiver, content, timestamp.
        var newMsg = new Message
        {
            Sender = await userManager.FindByNameAsync(senderId!),
            Receiver = await userManager.FindByIdAsync(recipientId!),
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
            Content = message.Content
        };
        //Save message to database → context.Messages.Add + SaveChangesAsync.
        context.Messages.Add(newMsg);
        await context.SaveChangesAsync();
        //Send message to recipient
        await Clients.User(recipientId!).SendAsync("ReceiveNewMessage", newMsg);
    }

    public async Task NotifyTyping(string recipientUserName)
    {
        var senderUserName = Context.User!.Identity!.Name;
        if (senderUserName is null)
        {
            return;
        }
        var connectionId = onlineUsers.Values.FirstOrDefault(x => x.UserName == recipientUserName)?.ConnectionId;
        if (connectionId != null)
        {
            await Clients.Client(connectionId).SendAsync("NotifyTypingToUser", senderUserName);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User!.Identity!.Name;
        onlineUsers.TryRemove(username!, out _);
        await Clients.All.SendAsync("OnlineUsers", await GetAllUsers());
    }
    //Returns a list of all users, marking which ones are online and their unread messages count.
    private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
    {
        //Represents the current authenticated user connected to SignalR.
        var username = Context.User!.GetUserName();
        // Create a set of online usernames for fast lookup
        var onlineUsersSet = new HashSet<string>(onlineUsers.Keys);

        //Query all users and map to OnlineUserDto
        //Allows you to query users directly without loading them all into memory, and Maps each AppUser to an OnlineUserDto object
        var users = await userManager.Users.Select(u => new OnlineUserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            ProfilePicture = u.ProfileImage,
            //checks if the user is online.
            IsOnline = onlineUsersSet.Contains(u.UserName!),
            //counts messages that are sent to the current user but not read.
            UnreadCount = context.Messages.Count(x => x.ReceiverId == username && x.SenderId == u.Id && !x.IsRead)
        }).OrderByDescending(u => u.IsOnline).ToListAsync();//Sorts users so online users appear first.

        //Sends the list of all users with online status and unread count back to whoever called GetAllUsers()
        return users;
    }
}
