using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class VictoryCondition : MonoBehaviour
{
    protected Player[] players;

    public void SetPlayers(Player[] players)
    {
        this.players = players;
    }

    public Player[] GetPlayers()
    {
        return players;
    }

    public virtual bool GameFinished()
    {
        return players == null || players.Any(PlayerMeetsConditions);
    }

    public Player GetWinner()
    {
        return players == null ? null : players.FirstOrDefault(PlayerMeetsConditions);
    }

    public abstract string GetDescription();

    public abstract bool PlayerMeetsConditions(Player player);
}
