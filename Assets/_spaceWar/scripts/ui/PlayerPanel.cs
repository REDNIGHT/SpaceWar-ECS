using RN.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RN.Network.SpaceWar
{
    public class PlayerPanel : SubPanel<PlayerPanel>
    {
        public __ClientWorld clientWorld;

        //
        Text gameReadyTime;
        protected void on_gameReadyTime_updateUI(ElementHandler e)
        {
            gameReadyTime = e.GetComponent<Text>();
            gameReadyTime.text = "";
        }
        public IEnumerator onPanelVisible(bool v)
        {
            while (visible && clientWorld.gameReadyTime > 0f)
            {
                yield return this;

                gameReadyTime.text = clientWorld.gameReadyTime.ToString("f0");
            }

            gameReadyTime.text = "";
        }


        //
        protected void on_actor_updateUI(Button b)
        {
            clientWorld.GetMyActorType();
            b.transform.GetChild(0).gameObject.SetActive(clientWorld.myActorType == b.transform.GetSiblingIndex());
        }
        protected void on_actor(Button b)
        {
            clientWorld.myActorType = (byte)b.transform.GetSiblingIndex();
            clientWorld.ChangeMyActorType();

            updateUI();
        }

        //
        protected void on_name_updateUI(InputField inputField)
        {
            clientWorld.GetMyPlayerName();
            inputField.text = clientWorld.myPlayerName;
        }
        protected void on_nameEnd(InputField inputField)
        {
            if (clientWorld.myPlayerName == inputField.text)
                return;

            if (inputField.text.Length < clientWorld.minPlayerNameCount)
                inputField.text = "P" + Random.Range(1000, 9999);

            clientWorld.myPlayerName = inputField.text;
            clientWorld.ChangeMyPlayerName();
        }

        //
        protected void on_team_updateUI(InputField inputField)
        {
            clientWorld.GetPlayerTeam();
            inputField.text = clientWorld.myPlayerTeam.ToString();
        }
        protected void on_teamEnd(InputField inputField)
        {
            clientWorld.myPlayerTeam = int.Parse(inputField.text);
            clientWorld.ChangeMyPlayerTeam();
        }
    }
}