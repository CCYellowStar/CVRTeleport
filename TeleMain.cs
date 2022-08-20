using System;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using ChilloutButtonAPI;
using ChilloutButtonAPI.UI;
using ABI_RC.Systems.MovementSystem;
using CVRPlayerEntity = ABI_RC.Core.Player.CVRPlayerEntity;
using ABI_RC.Core.Player;
using ABI.CCK.Components;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;

namespace CVRTeleport
{
    public class TeleMain:MelonMod
    {
        public static SubMenu main;
        public static bool isAllow;
        private static List<CVRPlayerEntity> _players { get; set; }
        private static List<CVRPlayerEntity> players = new List<CVRPlayerEntity>();
        private static List<string> _playersUname = new List<string>();
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(AccessTools.Constructor(typeof(PlayerDescriptor)), null, new HarmonyMethod(typeof(TeleMain).GetMethod(nameof(OnPlayerJoined), BindingFlags.NonPublic | BindingFlags.Static)));
            


            }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != -1)
            {
                return;
            }


            var teleobj = GameObject.Find("Cohtml/QuickMenu/Universal UI(Clone)/Scroll View/Viewport/Content/");
            foreach (Transform chil in teleobj.transform.GetAllChildren())
            {
                if(chil.gameObject.name== "Button(Clone)"&&chil.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text== "Teleport")
                {
                    UnityEngine.Object.Destroy(chil.gameObject);
                }
            }
            var teleobj2 = GameObject.Find("Cohtml/QuickMenu/");
            foreach (Transform chil in teleobj2.transform.GetAllChildren())
            {
                if (chil.gameObject.name == "Universal UI(Clone)(Clone)")
                {
                    var a = chil.Find("Scroll View/Viewport/Content/Back Button/Text (TMP) Title/");
                    if (a.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text == "Teleport")
                    {
                        UnityEngine.Object.Destroy(chil.gameObject);
                    }
                }
            }
            main = ChilloutButtonAPIMain.MainPage.AddSubMenu("Teleport");
            var cVRWorld = Resources.FindObjectsOfTypeAll<CVRWorld>();
            if (cVRWorld[0].allowFlying==true&& cVRWorld[0].allowSpawnables==true && Resources.FindObjectsOfTypeAll<CombatSystem>().Length == 0)
            {
                uptele();
                isAllow = true;
            }
            else
            {
                main.AddLabel("Teleport is not allowed in this world");
                isAllow = false;
            }

           


        }
        static  void OnPlayerJoined(PlayerDescriptor __instance)
        {
            if(isAllow)
            {
                var mc = new TeleMain();
                mc.uptele();
            }
            
        }
        void uptele()
        {
            var teleobj = GameObject.Find("Cohtml/QuickMenu/");
            foreach(Transform chil in teleobj.transform.GetAllChildren())
            {
                if(chil.gameObject.name== "Universal UI(Clone)(Clone)")
                {
                    var a = chil.Find("Scroll View/Viewport/Content/Back Button/Text (TMP) Title/");
                    if (a.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text== "Teleport")
                    {
                        var b = chil.Find("Scroll View/Viewport/Content/");
                        foreach(Transform dell in b.GetAllChildren())
                        {
                            if (dell.gameObject.name== "Button(Clone)")
                            {
                                UnityEngine.Object.Destroy(dell.gameObject);
                            }
                        }
                    }
                }
            }
            main.AddButton("Update Teleport", "", () =>
            {
                uptele();
            });
            _playersUname.Clear();
            players.Clear();
            _players = CVRPlayerManager.Instance.NetworkPlayers;
            for (int i = 0; i < _players.Count; i++)
            {
                _playersUname.Add(_players[i].Username);
            }
            _playersUname.Sort();
            for (int i = 0; i < _playersUname.Count; i++)
            {
                foreach(var ply in _players)
                {
                    if(ply.Username== _playersUname[i])
                    {
                        players.Add(ply);
                    }
                }
            }
            for (int i = 0; i < players.Count; i++)
            {
                if(ABI_RC.Core.Networking.IO.Social.Friends.FriendsWith(_players[i].Uuid))
                playup(players[i]);
            }
                
        }
        void playup(CVRPlayerEntity __instance)
        {
            var name = __instance.Username;
            main.AddButton(name, name, () =>
            {

                MovementSystem.Instance.TeleportTo(__instance.PlayerDescriptor.gameObject.transform.position);
            });
        }

        

    }
}
