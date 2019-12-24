using RN.UI;
using Unity.Entities;
using UnityEngine.SceneManagement;
using System.Collections;

namespace RN.Network.SpaceWar
{
    public class PausePanel : SubPanel<PausePanel>
    {
        IEnumerator on_setting()
        {
            yield return Out();
            yield return SettingPanel.singleton.In(back: thisPanel);
        }

        void on_quit()
        {
            MessageBox
                .singleton
                .Show((m) => { m.Message("Are you sure to quit game?"); }, onQuit);
        }

        public static IEnumerator onQuit(MessageBox box)
        {
            if (box.yes)
            {
                if (UIManager.singleton.hasCurPanel(LoadingPanel.singleton.channel))
                {
                    yield return UIManager.singleton.getCurPanel(LoadingPanel.singleton.channel).Out();
                }
                yield return LoadingPanel.singleton.In();
                LoadingPanel.singleton.setProgress(SceneManager.LoadSceneAsync(0));

                //
                foreach (var world in World.AllWorlds)
                {
                    foreach (var sys in world.Systems)
                    {
                        sys.Enabled = false;
                    }
                }
            }
        }
    }
}