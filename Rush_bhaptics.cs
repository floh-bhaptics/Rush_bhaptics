using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using MelonLoader;
using HarmonyLib;
using MyBhapticsTactsuit;

namespace Rush_bhaptics
{
    public class Rush_bhaptics : MelonMod
    {
        public static TactsuitVR tactsuitVr;

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            tactsuitVr = new TactsuitVR();
            tactsuitVr.PlaybackHaptics("HeartBeat");
        }
        
        [HarmonyPatch(typeof(AircraftControl), "Launch", new Type[] { })]
        public class bhaptics_PlayerLaunch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StartGliding();
            }
        }

        [HarmonyPatch(typeof(AircraftControl), "StopMotion", new Type[] { })]
        public class bhaptics_PlayerStopMotion
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopGliding();
            }
        }

        [HarmonyPatch(typeof(AircraftControl), "Update", new Type[] { })]
        public class bhaptics_PlayerFlyUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(AircraftControl __instance)
            {
                if (__instance.normalizedTotalSpeed == 0.0f) return;
                //tactsuitVr.LOG("Speed: " + __instance.normalizedTotalSpeed.ToString());
                tactsuitVr.updateGlideSpeed(__instance.normalizedTotalSpeed);
            }
        }

        [HarmonyPatch(typeof(AircraftControl), "GoalReached", new Type[] { })]
        public class bhaptics_GoalReached
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.PlaybackHaptics("LevelUp");
            }
        }

        [HarmonyPatch(typeof(AircraftControl), "HitCollectible", new Type[] { typeof(Adventure.AdventureItemType) })]
        public class bhaptics_HitCollectible
        {
            [HarmonyPostfix]
            public static void Postfix(Adventure.AdventureItemType type)
            {
                tactsuitVr.PlaybackHaptics("LevelUp");
            }
        }

        [HarmonyPatch(typeof(AircraftControl), "Crashed", new Type[] { typeof(UnityEngine.Collider) })]
        public class bhaptics_Crashed
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (tactsuitVr.IsPlaying("HitByWall")) return;
                tactsuitVr.StopGliding();
                tactsuitVr.PlaybackHaptics("HitByWall");
            }
        }

        [HarmonyPatch(typeof(AircraftControl), "Splashdown", new Type[] {  })]
        public class bhaptics_Splashdown
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                tactsuitVr.StopGliding();
            }
        }
    }
}
