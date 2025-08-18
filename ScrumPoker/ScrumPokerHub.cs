using Microsoft.AspNetCore.SignalR;
using System;
using System.Text;

public class ScrumPokerHub : Hub
{
    private static readonly Dictionary<string, Dictionary<string, (string ConnectionId, string? Vote)>> Rooms = new();
    private static readonly Dictionary<string, Timer> RoomCleanupTimers = new();
    
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
        
        // Cancela qualquer cleanup agendado já que a sala tem usuários agora
        CancelRoomCleanup(roomCode);
        
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
                // Em vez de remover imediatamente, agenda remoção para daqui a 30 segundos
                ScheduleRoomCleanup(roomCode);
            }
            else
            {
                // Se a sala ainda tem usuários, cancela qualquer cleanup agendado
                CancelRoomCleanup(roomCode);
                await Clients.Group(roomCode).SendAsync("UpdateUsers", room.Keys);
                await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
            }
        }
    }

    public async Task<bool> ChangeUserName(string roomCode, string oldUserName, string newUserName)
    {
        if (!Rooms.ContainsKey(roomCode))
        {
            await Clients.Caller.SendAsync("RoomNotFound");
            return false;
        }

        var room = Rooms[roomCode];
        
        if (!room.ContainsKey(oldUserName))
        {
            return false;
        }

        // Verifica se o novo nome já existe na sala
        if (room.ContainsKey(newUserName))
        {
            return false;
        }

        // Remove o usuário com nome antigo das outras salas (se existir)
        foreach (var otherRoomCode in Rooms.Keys)
        {
            if (otherRoomCode != roomCode && Rooms[otherRoomCode].ContainsKey(newUserName))
            {
                Rooms[otherRoomCode].Remove(newUserName);
                await Clients.Group(otherRoomCode).SendAsync("UpdateUsers", Rooms[otherRoomCode].Keys);
                await Clients.Group(otherRoomCode).SendAsync("UpdateVotes", Rooms[otherRoomCode].ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
            }
        }

        // Pega os dados do usuário antigo (connectionId e voto)
        var userData = room[oldUserName];
        
        // Remove o nome antigo e adiciona o novo com os mesmos dados
        room.Remove(oldUserName);
        room[newUserName] = userData;
        
        // Atualiza todos os clientes da sala
        await Clients.Group(roomCode).SendAsync("UpdateUsers", room.Keys);
        await Clients.Group(roomCode).SendAsync("UpdateVotes", room.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        
        return true;
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
                    // Em vez de remover imediatamente, agenda remoção para daqui a 30 segundos
                    ScheduleRoomCleanup(roomCode);
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

    private static void ScheduleRoomCleanup(string roomCode)
    {
        // Cancela timer existente se houver
        CancelRoomCleanup(roomCode);
        
        // Cria novo timer para remover a sala em 30 segundos
        var timer = new Timer(_ => 
        {
            if (Rooms.ContainsKey(roomCode) && Rooms[roomCode].Count == 0)
            {
                Rooms.Remove(roomCode);
                CancelRoomCleanup(roomCode);
            }
        }, null, TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
        
        RoomCleanupTimers[roomCode] = timer;
    }

    private static void CancelRoomCleanup(string roomCode)
    {
        if (RoomCleanupTimers.TryGetValue(roomCode, out var timer))
        {
            timer.Dispose();
            RoomCleanupTimers.Remove(roomCode);
        }
    }
}
