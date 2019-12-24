using RN.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace RN.Network.SpaceWar
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    public class PlayerInput2UISystem : ComponentSystem
    {
        Rewired.Player input;
        __ClientWorld clientWorld;
        PlayerInputClientSystem playerInputClientSystem;
        CameraControllerSystem cameraControllerSystem;
        protected void OnInit(Transform root)
        {
            input = Rewired.ReInput.players.GetPlayer(InputPlayer.System);
            //input.isPlaying = true;

            clientWorld = root.GetComponent<__ClientWorld>();
            Debug.Assert(clientWorld != null, $"clientWorld != null  root={root}", root);

            //
            World.GetExistingSystem<NetworkStreamStateSystem>().connectHandle += connect;
            World.GetExistingSystem<NetworkStreamStateSystem>().connectedHandle += connected;
            World.GetExistingSystem<NetworkStreamStateSystem>().disconnectedHandle += disconnected;

            playerInputClientSystem = World.GetExistingSystem<PlayerInputClientSystem>();
            cameraControllerSystem = World.GetExistingSystem<CameraControllerSystem>();
        }

        protected override void OnUpdate()
        {
            OnExit();
            OnScores();
        }


        //
        void connect()
        {
            if (World == ClientBootstrap.worlds[0])
                Message.singleton.show("Connecting...", -1f);
        }
        void connected()
        {
            if (World == ClientBootstrap.worlds[0] && Message.singleton.visible)
                Message.singleton.startOut();
        }
        void disconnected(string error)
        {
            Enabled = false;

            if (World == ClientBootstrap.worlds[0] && Message.singleton.visible)
                Message.singleton.startOut();

            if (ScoresPanel.singleton.visible)
                ScoresPanel.singleton.startOut();

            if (PausePanel.singleton.visible)
                PausePanel.singleton.startOut();

            MessageBox
                .singleton
                .Show((m) =>
                {
                    m
                    .No(null)
                    .autoBack(false)
                    .Message($"Disconnected!\n<size=16>e:{error}</size>")
                    ;
                },
                PausePanel.onQuit);
        }


        //
        protected void OnExit()
        {
            if (input.GetButtonUp(PlayerInputActions.Exit)
            && UIManager.singleton.hasCurPanel(0) == false)
            {
                PlayerPanel.singleton.clientWorld = clientWorld;
                PausePanel.singleton.startIn();

                PausePanel.singleton.StartCoroutine(playerInputClientSystemEnabled());
            }
        }

        IEnumerator playerInputClientSystemEnabled()
        {
            playerInputClientSystem.Enabled = false;
            cameraControllerSystem.Enabled = false;

            while (UIManager.singleton.hasCurPanel(0))
                yield return this;

            playerInputClientSystem.Enabled = true;
            cameraControllerSystem.Enabled = true;
        }


        //
        List<ScoreInfo> playerScores = new List<ScoreInfo>();
        List<ScoreInfo> myTeamScores = new List<ScoreInfo>();
        protected void OnScores()
        {
            if (input.GetButtonDown(PlayerInputActions.Scores))
            {
                if (UIManager.singleton.hasCurPanel(0))
                    return;

                var myPlayerTeamId = 0;

                var myPlayerSingleton = GetSingleton<MyPlayerSingleton>();
                if (myPlayerSingleton.playerEntity != default)
                {
                    myPlayerTeamId = EntityManager.GetComponentData<PlayerTeam>(myPlayerSingleton.playerEntity).value;
                }


                //
                Entities
                    .WithAllReadOnly<Player, PlayerName, PlayerScore, PlayerTeam>()
                    .WithNone<PlayerDestroyNetMessages>()
                    .ForEach((ref PlayerName playerName, ref PlayerScore playerScore, ref PlayerTeam playerTeam) =>
                    {
                        var scoreInfo = new ScoreInfo { name = playerName.value.ToString(), score = playerScore.value };
                        playerScores.Add(scoreInfo);

                        var playerTeamId = playerTeam.value;
                        if (playerTeamId != 0 && playerTeamId == myPlayerTeamId)//my team
                        {
                            myTeamScores.Add(scoreInfo);
                        }
                    });

                ScoresPanel.singleton.setScores(playerScores, myTeamScores);
                ScoresPanel.singleton.startIn();

                playerScores.Clear();
                myTeamScores.Clear();
            }

            if (input.GetButtonUp(PlayerInputActions.Scores))
            {
                if (ScoresPanel.singleton.visible)
                    ScoresPanel.singleton.startOut();
            }
        }
    }
}
