using Microsoft.AspNetCore.SignalR;
using System;
using System.Text;

public class ScrumPokerHub : Hub
{
    private static readonly Dictionary<string, Dictionary<string, (string ConnectionId, string? Vote)>> Rooms = new();
    
    public static readonly List<string> ValidVotes = new() { "☕", "?", "0", "0.5", "1", "2", "3", "5", "8", "13", "20", "40", "100" };

    public List<string> GetValidVotes()
    {
        return ValidVotes;
    }

    public bool RoomExists(string roomCode)
    {
        return Rooms.ContainsKey(roomCode);
    }

    public string CreateRoom()
    {
        string roomCode = GenerateRoomCode();
        while (Rooms.ContainsKey(roomCode))
        {
            roomCode = GenerateRoomCode();
        }
        
        Rooms[roomCode] = new Dictionary<string, (string ConnectionId, string? Vote)>();
        
        return roomCode;
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        var code = new StringBuilder(5);
        
        for (int i = 0; i < 5; i++)
        {
            code.Append(chars[random.Next(chars.Length)]);
        }
        
        return code.ToString();
    }

    public async Task<bool> JoinRoom(string roomCode, string userName)
    {
        if (!Rooms.ContainsKey(roomCode))
        {
            await Clients.Caller.SendAsync("RoomNotFound");
            return false;
        }

        var room = Rooms[roomCode];
        
        foreach (var otherRoomCode in Rooms.Keys)
        {
            if (Rooms[otherRoomCode].ContainsKey(userName))
            {
                Rooms[otherRoomCode].Remove(userName);
                await Clients.Group(otherRoomCode).SendAsync("UpdateUsers", Rooms[otherRoomCode].Keys);
                await Clients.Group(otherRoomCode).SendAsync("UpdateVotes", Rooms[otherRoomCode].ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
            }
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        
        room[userName] = (Context.ConnectionId, null);
        
        await Clients.Group(roomCode).SendAsync("UpdateUsers", room.Keys);
        await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        
        return true;
    }

    public async Task LeaveRoom(string roomCode, string userName)
    {
        if (Rooms.ContainsKey(roomCode) && Rooms[roomCode].ContainsKey(userName))
        {
            var room = Rooms[roomCode];
            room.Remove(userName);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            
            if (room.Count == 0)
            {
                Rooms.Remove(roomCode);
            }
            else
            {
                await Clients.Group(roomCode).SendAsync("UpdateUsers", room.Keys);
                await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
            }
        }
    }

    public async Task SubmitVote(string roomCode, string userName, string vote)
    {
        if (Rooms.ContainsKey(roomCode) && 
            Rooms[roomCode].ContainsKey(userName) && 
            ValidVotes.Contains(vote))
        {
            var room = Rooms[roomCode];
            room[userName] = (room[userName].ConnectionId, vote);
            
            await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        }
    }

    public async Task RevealVotes(string roomCode)
    {
        if (Rooms.ContainsKey(roomCode))
        {
            var room = Rooms[roomCode];
            await Clients.Group(roomCode).SendAsync("RevealVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        }
    }

    public async Task ResetVotes(string roomCode)
    {
        if (Rooms.ContainsKey(roomCode))
        {
            var room = Rooms[roomCode];
            foreach (var key in room.Keys.ToList())
            {
                room[key] = (room[key].ConnectionId, null);
            }
            
            await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var roomCode in Rooms.Keys.ToList())
        {
            var room = Rooms[roomCode];
            var user = room.FirstOrDefault(x => x.Value.ConnectionId == Context.ConnectionId);
            
            if (!string.IsNullOrEmpty(user.Key))
            {
                room.Remove(user.Key);
                
                if (room.Count == 0)
                {
                    Rooms.Remove(roomCode);
                }
                else
                {
                    await Clients.Group(roomCode).SendAsync("UpdateUsers", room.Keys);
                    await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
