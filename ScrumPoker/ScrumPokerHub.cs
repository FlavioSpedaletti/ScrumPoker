using Microsoft.AspNetCore.SignalR;

public class ScrumPokerHub : Hub
{
    private static readonly Dictionary<string, string?> Votes = new();

    public async Task JoinGame(string userName)
    {
        if (!Votes.ContainsKey(userName))
        {
            Votes[userName] = null;
            await Clients.All.SendAsync("UpdateUsers", Votes.Keys);
        }
    }

    public async Task SubmitVote(string userName, string vote)
    {
        if (Votes.ContainsKey(userName))
        {
            Votes[userName] = vote;
            await Clients.All.SendAsync("UpdateVotes", Votes);
        }
    }

    public async Task RevealVotes()
    {
        await Clients.All.SendAsync("RevealVotes", Votes);
    }

    public async Task ResetVotes()
    {
        foreach (var key in Votes.Keys)
        {
            Votes[key] = null;
        }
        await Clients.All.SendAsync("UpdateVotes", Votes);
    }
}
