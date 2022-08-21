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
        public static SubMenu main2;
        public static bool isAllow;
        private static List<CVRPlayerEntity> _players { get; set; }
        public static List<Vector3> TPPost = new List<Vector3>();
        public static List<Quaternion> TPRost = new List<Quaternion>();
        public static MelonPreferences_Category TeleportCategory;
        public static  MelonPreferences_Entry<bool>isAllowAllplayerTP;
        
        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(AccessTools.Constructor(typeof(PlayerDescriptor)), null, new HarmonyMethod(typeof(TeleMain).GetMethod(nameof(OnPlayerJoined), BindingFlags.NonPublic | BindingFlags.Static)));
            TeleportCategory=MelonPreferences.CreateCategory("CVRTeleport");
            isAllowAllplayerTP=TeleportCategory.CreateEntry(
                "isAllowAllplayerTP",
                false,
                "AllowAllplayerTP",
                "Enabling this option will allow Teleporter to anyone, which may be risky！");

        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex != -1)
            {
                return;
            }


            clearMenu();
            TPPost.Clear();
            TPRost.Clear();
            main = ChilloutButtonAPIMain.MainPage.AddSubMenu("Teleporter To Player");
            main2 = ChilloutButtonAPIMain.MainPage.AddSubMenu("Teleporter point");
            var cVRWorld = Resources.FindObjectsOfTypeAll<CVRWorld>();
            if (cVRWorld[0].allowFlying==true&& cVRWorld[0].allowSpawnables==true && Resources.FindObjectsOfTypeAll<CombatSystem>().Length == 0)
            {
                uptele();
                updatepot();
                isAllow = true;
            }
            else
            {
                main.AddLabel("Teleport is not allowed in this world");
                main2.AddLabel("Teleport is not allowed in this world");
                isAllow = false;
            }

        }
        static void OnPlayerJoined(PlayerDescriptor __instance)
        {
            if(isAllow)
            {
                var mc = new TeleMain();
                mc.uptele();
            }
            
        }
        void updatepot()
        {
            main2.AddButton("Add Teleporter point", "", () =>
            {
                TPPost.Add(PlayerSetup.Instance.gameObject.transform.position);
                TPRost.Add(PlayerSetup.Instance.gameObject.transform.rotation);
                var i = TPPost.Count;
                main2.AddButton("Teleporter point " + i.ToString(), "", () =>
                {
                    MovementSystem.Instance.TeleportToPosRot(TPPost[i - 1], TPRost[i - 1]);
                });
            });
        }
        void uptele()
        {
            clearPlayerList();
            main.AddButton("Update Teleport", "", () =>
            {
                uptele();
            });
            _players = CVRPlayerManager.Instance.NetworkPlayers;
            _players.Sort(new playersoft());

            for (int i = 0; i < _players.Count; i++)
            {
                
                if (!isAllowAllplayerTP.Value)
                {
                    if (ABI_RC.Core.Networking.IO.Social.Friends.FriendsWith(_players[i].Uuid))
                        playup(_players[i]);
                }
                else
                {
                    playup(_players[i]);
                }
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
        void clearMenu()
        {
            var teleobj = GameObject.Find("Cohtml/QuickMenu/Universal UI(Clone)/Scroll View/Viewport/Content/");
            foreach (Transform chil in teleobj.transform.GetAllChildren())
            {
                if (chil.gameObject.name == "Button(Clone)" && chil.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text == "Teleporter To Player"|| chil.gameObject.name == "Button(Clone)" && chil.Find("Text (TMP)").GetComponent<TMPro.TextMeshProUGUI>().text == "Teleporter point")
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
                    if (a.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text == "Teleporter To Player"||a.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text == "Teleporter point")
                    {
                        UnityEngine.Object.Destroy(chil.gameObject);
                    }
                }
            }
        }
        void clearPlayerList()
        {
            var teleobj = GameObject.Find("Cohtml/QuickMenu/");
            foreach (Transform chil in teleobj.transform.GetAllChildren())
            {
                if (chil.gameObject.name == "Universal UI(Clone)(Clone)")
                {
                    var a = chil.Find("Scroll View/Viewport/Content/Back Button/Text (TMP) Title/");
                    if (a.gameObject.GetComponent<TMPro.TextMeshProUGUI>().text == "Teleporter To Player")
                    {
                        var b = chil.Find("Scroll View/Viewport/Content/");
                        foreach (Transform dell in b.GetAllChildren())
                        {
                            if (dell.gameObject.name == "Button(Clone)")
                            {
                                UnityEngine.Object.Destroy(dell.gameObject);
                            }
                        }
                    }
                }
            }
        }
       
        class playersoft : IComparer<CVRPlayerEntity>
        {
            public int Compare(CVRPlayerEntity x, CVRPlayerEntity y)
            {
                return x.Username.CompareTo(y.Username);
            }
        }
    }
}
