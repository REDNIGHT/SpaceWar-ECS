using UnityEngine;

namespace RN.Network.SpaceWar
{
    public class KillInfosPanel : Network.KillInfosPanel
    {
        public KillInfoItem killInfoItemPrefab;
        public float time = 2.5f;
        public override void pushKillInfo(in string targetName, in string[] killerNames)
        {
            var _killerNames = "";
            for (var i = 1; i < killerNames.Length; ++i)
            {
                _killerNames += killerNames[i] = " ";
            }

            var killInfoItem = GameObject.Instantiate(killInfoItemPrefab, transform.GetChild(0));
            killInfoItem.mainKillerName = killerNames[0];
            killInfoItem.killerNames = _killerNames;
            killInfoItem.targetName = targetName;
            killInfoItem.time = time;


            killInfoItem.updateItems();
        }

    }
}
