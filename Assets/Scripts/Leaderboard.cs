using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Leaderboard : ScriptableObject
{

    
    [System.Serializable]
    public class LeaderboardData
    {
        public string name;
        public int score;

    }

    private List<LeaderboardData> scores;
    public int HighScore { get { Sort(); return scores[0].score; } }
    public void AddScore(string name, int score)
    {
        LeaderboardData data = new LeaderboardData();
        data.name = name;
        data.score = score;
        scores.Add(data);
    }

    public void Sort()
    {
        scores.Sort((a,b) => { return a.score > b.score ? a.score : b.score; });
    }
}
