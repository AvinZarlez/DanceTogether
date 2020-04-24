using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using App.Networking;

namespace App.Utility
{
    /// <summary>
    /// Indexed Colors is a struct that can store an Index, a Name, and a Color.
    /// </summary>
    public struct IndexedColor
    {
        public int Index;
        public string Name;
        public Color Color;

        public IndexedColor(int _index, string _name, Color _color)
        {
            Index = _index;
            Name = _name;
            Color = _color;
        }
    }
    public class DanceTogetherUtility
    {
        #region Indexed Color
        /// <summary>
        /// An Index Color can be selected from 0 to 17.
        /// </summary>
        static public IndexedColor[] colors = new IndexedColor[]
        {
            new IndexedColor(0, "White", Color.white),
            new IndexedColor(1, "Blue", Color.blue),
            new IndexedColor(2, "Red", Color.red),
            new IndexedColor(3, "Green", Color.green),
            new IndexedColor(4, "Yellow", Color.yellow),
            new IndexedColor(5, "Cyan", Color.cyan),
            new IndexedColor(6, "Magenta", Color.magenta),
            new IndexedColor(7, "Orange", new Color(1f,0.5f,0.0f)),
            new IndexedColor(7, "Orchid", new Color(0.75f,0.0f,1.0f)),
            new IndexedColor(8, "Ocean", new Color(0.0f,0.5f,1.0f)),
            new IndexedColor(9, "Forest", new Color(0.0f,0.5f,0.07f)),
            new IndexedColor(10, "Midnight", new Color(0.0f,0.0f,0.5f)),
            new IndexedColor(11, "Purple", new Color(0.25f,0.0f,0.5f)),
            new IndexedColor(12, "Wine", new Color(0.5f,0.0f,0.0f)),
            new IndexedColor(13, "Olive", new Color(0.5f,0.5f,0.0f)),
            new IndexedColor(14, "Brown", new Color(0.25f,0.13f,0.0f)),
            new IndexedColor(15, "Wood", new Color(0.5f,0.25f,0.0f)),
            new IndexedColor(17, "Black", new Color(0f, 0f, 0f))
        };

        // random number generator for use as index knowledge
        /// <summary>
        /// Returns a random int from 0 to 17.
        /// </summary>
        static public int RandomColorIndexNumber()
        {
            return Mathf.RoundToInt(Random.Range(0f, 17f));
        }
        /// <summary>
        /// Return an index color that falls between int of value 0 to 17.
        /// </summary>
        static public IndexedColor GetIndexColor(int _index)
        {
            Assert.IsTrue(_index < colors.Length);

            return colors[_index];
        }
        /// <summary>
        /// Randomizes a Color and returns as an IndexedColor.
        /// </summary>
        static public IndexedColor GetRandomIndexedColor()
        {
            return GetIndexColor(RandomColorIndexNumber());
        }
        #endregion

        #region Pair Assignment 
        /// <summary>
        /// A utility used to Randomly match a song id to Randomized pairs of players.
        /// This ustility is intended to be used by the server only, and send updates to clients.
        /// Adjust group size to change pairing dynamics.
        /// 
        /// Note: A possible refactor could be we assign the group number back to the player for local reference.
        /// currently we are just using it to match players.
        /// </summary>
        /// <param name="playerList"></param>
        /// <param name="tracklistLength"></param>
        /// <param name="groupSize"></param>
        static public void AssignPlayerGroups(List<DanceTogetherPlayer> playerList, int tracklistLength, int groupSize, bool network = true)
        {
            // test player count
            if (playerList.Count%2 == 0)
            {
                Debug.Log("even number of players are available.");
            } else
            {
                Debug.Log("Odd number of players Available.");
            }

            //int reserveTrack = 0; // a reserve track to assign to the odd third wheel.

            List<int> AvailablePlayers = new List<int>();
            List<int> AvailableTracks = new List<int>();
            for (int i = 0; i < tracklistLength; i++)
            {
                AvailableTracks.Add(i);
            }
            for (int i = 0; i < playerList.Count; i++)
            {
                AvailablePlayers.Add(i);
            }

            Debug.Log("utility list count : " + playerList.Count + " - available player : " + AvailablePlayers.Count + " - avaialable tracks : " + AvailableTracks.Count + " - group size : " + groupSize);

            int groupCount = Mathf.FloorToInt(AvailablePlayers.Count / groupSize);

            Debug.Log("groups available : " + groupCount); // create grouping bucket count

            List<List<DanceTogetherPlayer>> playerGroupings = new List<List<DanceTogetherPlayer>>(); // simple list of player lists.

            for(int g = 0; g < groupCount; g++) // iterate the groups
            {
                List<DanceTogetherPlayer> curPlayerGroup = new List<DanceTogetherPlayer>();

                for (int j = 0; j < groupSize; j++) // iterate the group size
                {
                    if (AvailablePlayers.Count > 0) // check if any players are left, since we rounded up. there will be situations where there are odd remainder players. E.G. 2 groups of 8 and 15 players
                    {
                        int randomSelectPlayer = AvailablePlayers[Random.Range(0, AvailablePlayers.Count)]; // grab random player ref
                        curPlayerGroup.Add(playerList[randomSelectPlayer]); // add selected player to player group
                        //Debug.Log("add player - " + playerList[randomSelectPlayer].PlayerID + " - to group : " + j);

                        AvailablePlayers.Remove(randomSelectPlayer); // clean tracking list of random player's ref.
                    }
                }

                playerGroupings.Add(curPlayerGroup); // add current group to groups.
            }

            // Take care of remainder players.
            
            if (AvailablePlayers.Count > 0)
            {
                float gCount = groupCount;
                float aCount = AvailablePlayers.Count;
                float remainder = aCount / gCount;
                int remainderPerGroup = Mathf.CeilToInt(remainder); // determine how to split remainder 
                Debug.Log("remainders per group : " + remainder + " - remainder per group : " + remainderPerGroup);
                foreach (List<DanceTogetherPlayer> groupList in playerGroupings)
                {
                    for (int r = 0; r < remainderPerGroup; r++) // add remainder player/s to existing groups.
                    {
                        if (AvailablePlayers.Count > 0) // check if any players are left, since we rounded up. there will be situations where there are odd remainder players. E.G. 4 groups of 3 and 14 players - 2 groups will have 2 more than the other 2
                        {
                            int randomSelectPlayer = AvailablePlayers[Random.Range(0, AvailablePlayers.Count)]; // grab random player ref
                            groupList.Add(playerList[randomSelectPlayer]); // add selected player to player group
                            Debug.Log("add player - " + playerList[randomSelectPlayer].PlayerID + " - to group");

                            AvailablePlayers.Remove(randomSelectPlayer); // clean tracking list of random player's ref.
                        }
                    }
                }
            }
            

            // assign random songs to the group selections
            foreach (List<DanceTogetherPlayer> groupList in playerGroupings)
            {
                Debug.Log("Group list");
                // pick a random track id from available track ids
                int randomSelectTrack = Random.Range(0, AvailableTracks.Count); // save ref
                int newTrack = AvailableTracks[randomSelectTrack]; // select actual track
                AvailableTracks.Remove(randomSelectTrack); // remove ref.

                foreach(DanceTogetherPlayer player in groupList)
                {
                    if (network) // currently only set false by unit tester.
                    {
                        player.CmdSetSongID(newTrack);// assign previously selected random track to new random player.
                    }
                    else
                    {
                        Debug.Log("player : " + player.PlayerID + " - song id assigned to : " + newTrack);
                    }
                }
            }

            // single player testing. assign a var 
            if(playerList.Count == 1)
            {
                int randomSelectTrack = Random.Range(0, AvailableTracks.Count); // save ref
                playerList[0].CmdSetSongID(AvailableTracks[randomSelectTrack]); // set song id to random.
            }
        }
        #endregion
    }
}
