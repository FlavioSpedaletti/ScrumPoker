using Microsoft.AspNetCore.SignalR;

public class ScrumPokerHub : Hub
{
    private static readonly Dictionary<string, (string ConnectionId, string? Vote)> Votes = new();

    public async Task JoinGame(string userName)
    {
        var existingUser = Votes.FirstOrDefault(x => x.Key == userName);
        if (!string.IsNullOrEmpty(existingUser.Key))
        {
            Votes.Remove(existingUser.Key);
        }

        Votes[userName] = (Context.ConnectionId, null);
        await Clients.All.SendAsync("UpdateUsers", Votes.Keys);
        //await Clients.All.SendAsync("UpdateVotes", Votes);
    }

    public async Task LeaveGame(string userName)
    {
        if (Votes.ContainsKey(userName))
        {
            Votes.Remove(userName);
            await Clients.All.SendAsync("UpdateUsers", Votes.Keys);
            await Clients.All.SendAsync("UpdateVotes", Votes);
        }
    }

    public async Task SubmitVote(string userName, string vote)
    {
        if (Votes.ContainsKey(userName))
        {
            Votes[userName] = (Votes[userName].ConnectionId, vote);
            await Clients.All.SendAsync("UpdateVotes", Votes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        }
    }

    public async Task RevealVotes()
    {
        await Clients.All.SendAsync("RevealVotes", Votes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
    }

    public async Task ResetVotes()
    {
        foreach (var key in Votes.Keys.ToList())
        {
            Votes[key] = (Votes[key].ConnectionId, null);
        }
        await Clients.All.SendAsync("UpdateVotes", Votes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Votes.FirstOrDefault(x => x.Value.ConnectionId == Context.ConnectionId);
        if (!string.IsNullOrEmpty(user.Key))
        {
            Votes.Remove(user.Key);
            await Clients.All.SendAsync("UpdateUsers", Votes.Keys);
            await Clients.All.SendAsync("UpdateVotes", Votes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Vote));
        }

        await base.OnDisconnectedAsync(exception);
    }
}
