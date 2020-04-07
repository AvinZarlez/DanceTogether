using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.Utility;
using App.Networking;

namespace App.UnitTests
{
    public class UitlityUnitTester : MonoBehaviour
    {
        [Header("Manual Testing Vars")]
        public int NumberOfPlayers = 4;
        public int NumberOfSongs = 10;
        public int GroupSize = 2;

        public void TestDancePairing()
        {
            List<DanceTogetherPlayer> playerList = new List<DanceTogetherPlayer>();
            for(int i = 0; i < NumberOfPlayers; i++)
            {
                DanceTogetherPlayer newPlayer = new DanceTogetherPlayer();
                newPlayer.PlayerID = i;
                playerList.Add(newPlayer);
            }

            DanceTogetherUtility.AssignPairs(playerList, NumberOfSongs, GroupSize, false);
        }
    }
}
