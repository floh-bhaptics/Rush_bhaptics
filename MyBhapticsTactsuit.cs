using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

using System.Resources;
using System.Globalization;
using System.Collections;
using MelonLoader;


namespace MyBhapticsTactsuit
{

    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        public static float glideIntensity = 0.8f;
        public static int glidePause = 300;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent Gliding_mrse = new ManualResetEvent(false);
        // List of flying patterns
        private static List<String> FlyingFront = new List<string> { };
        private static List<String> FlyingBack = new List<string> { };
        // Random numbers for flying
        private static Random flyRandom = new Random();
        private static int patternFront;
        private static int patternBack;
        private static int flyPause;
        private static float flyIntensity;
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, String> FeedbackMap = new Dictionary<String, String>();

        private static bHaptics.RotationOption defaultRotationOption = new bHaptics.RotationOption(0.0f, 0.0f);

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                PlaybackHaptics("HeartBeat");
                Thread.Sleep(1000);
            }
        }

        public void GlidingFunc()
        {
            while (true)
            {
                // Check if reset event is active
                Gliding_mrse.WaitOne();
                patternFront = flyRandom.Next(FlyingFront.Count);
                patternBack = flyRandom.Next(FlyingBack.Count);
                flyPause = flyRandom.Next(glidePause);
                flyIntensity = (float)flyRandom.NextDouble() * glideIntensity;
                PlaybackHaptics(FlyingFront[patternFront], flyIntensity);
                PlaybackHaptics(FlyingBack[patternBack], flyIntensity);
                Thread.Sleep(flyPause);
            }
        }

        public void updateGlideSpeed(float speed)
        {
            glideIntensity = speed * 0.6f;
            glidePause = Math.Max( (int)(500 * (1.0f - speed)), 0) + 30;
        }

        public TactsuitVR()
        {

            LOG("Initializing suit");
            if (!bHaptics.WasError)
            {
                suitDisabled = false;
            }
            RegisterAllTactFiles();
            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread GlidingThread = new Thread(GlidingFunc);
            GlidingThread.Start();
        }

        public void LOG(string logStr)
        {
//#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
//#pragma warning restore CS0618
        }


        void RegisterAllTactFiles()
        {
            if (suitDisabled) { return; }
            /*
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string assemblyFile = Assembly.GetExecutingAssembly().Location;
            string myPath = Path.GetDirectoryName(assemblyFile);
            LOG("Assembly path: " + myPath);
            string configPath = Path.Combine(myPath, "bHaptics");
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    hapticPlayer.RegisterTactFileStr(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
            }
            */
            ResourceSet resourceSet = Rush_bhaptics.Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true);

            foreach (DictionaryEntry dict in resourceSet)
            {
                try
                {
                    bHaptics.RegisterFeedbackFromTactFile(dict.Key.ToString(), dict.Value.ToString());
                    LOG("Pattern registered: " + dict.Key.ToString());
                    FeedbackMap.Add(dict.Key.ToString(), dict.Value.ToString());
                }
                catch (Exception e) { LOG(e.ToString()); continue; }

                if (dict.Key.ToString().StartsWith("FlyingAir_Back"))
                {
                    FlyingBack.Add(dict.Key.ToString());
                }
                if (dict.Key.ToString().StartsWith("FlyingAir_Front"))
                {
                    FlyingFront.Add(dict.Key.ToString());
                }

            }

            systemInitialized = true;
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            if (suitDisabled) { return; }
            if (FeedbackMap.ContainsKey(key))
            {
                bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(intensity, duration);
                bHaptics.SubmitRegistered(key, key, scaleOption, defaultRotationOption);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }


        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StartGliding()
        {
            Gliding_mrse.Set();
        }

        public void StopGliding()
        {
            Gliding_mrse.Reset();
            bHaptics.TurnOff("FlyingMedium");
        }

        public void StopHapticFeedback(String effect)
        {
            bHaptics.TurnOff(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                bHaptics.TurnOff(key);
            }
        }

        public void StopThreads()
        {
            StopHeartBeat();
            StopGliding();
        }

        public bool IsPlaying(String effect)
        {
            return bHaptics.IsPlaying(effect);
        }



    }
}
