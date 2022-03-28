using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                tactsuitVr.LOG("Speed: " + __instance.normalizedTotalSpeed.ToString());
                tactsuitVr.updateGlideSpeed(__instance.normalizedTotalSpeed);
            }
        }

    }
}
