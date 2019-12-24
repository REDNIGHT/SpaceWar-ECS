using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class ScoreInfo
    {
        public string name;
        public int score;
    }

    public class ScoresPanel : UI.SubPanel<ScoresPanel>
    {
        public void setScores(List<ScoreInfo> playerScores, List<ScoreInfo> myTeamScores)
        {
            playerScores.Sort((x, y) => y.score - x.score);
            myTeamScores.Sort((x, y) => y.score - x.score);

            setScores(0, playerScores);
            setScores(1, myTeamScores);
        }

        void setScores(int childIndex, List<ScoreInfo> scores)
        {
            var i = 0;
            foreach (Transform rt in transform.GetChild(0).GetChild(childIndex))
            {
                if (i >= scores.Count)
                {
                    setScores(rt, "----", "----");
                }
                else
                {
                    setScores(rt, scores[i]);
                }
                ++i;
            }
        }

        void setScores(Transform rt, ScoreInfo scoreInfo)
        {
            setScores(rt, scoreInfo.name, scoreInfo.score.ToString());
        }
        void setScores(Transform rt, in string playerName, in string score)
        {
            rt.GetChild(0).GetComponent<Text>().text = playerName;
            rt.GetChild(1).GetComponent<Text>().text = score;
        }
    }
}
