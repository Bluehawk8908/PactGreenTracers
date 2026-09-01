using System.Collections;
using CustomizableTracers;
using MelonLoader;
using UnityEngine;
using GHPC.Weapons;
using GHPC.Weaponry;
using GHPC.State;

[assembly: MelonInfo(typeof(CustomTracers), "Customizable Tracers", "1.0.0", "Bluehawk")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace CustomizableTracers
{
    public class CustomTracers : MelonMod
    {
        public GameObject gameManager;
        public static MelonPreferences_Entry<float> NATO_red;
        public static MelonPreferences_Entry<float> NATO_green;
        public static MelonPreferences_Entry<float> NATO_blue;

        public static MelonPreferences_Entry<float> PACT_red;
        public static MelonPreferences_Entry<float> PACT_green;
        public static MelonPreferences_Entry<float> PACT_blue;

        public static MelonPreferences_Entry<int> frequency;

        public static Vector4 NATO_colour;
        public static Vector4 PACT_colour;

        public override void OnInitializeMelon()
        {
            MelonPreferences_Category cfg = MelonPreferences.CreateCategory("Customizable Tracers");
            NATO_red = cfg.CreateEntry<float>("NATO Red", 11.34f);
            NATO_red.Comment = "11.34";
            NATO_green = cfg.CreateEntry<float>("NATO Green", 0.55f);
            NATO_green.Comment = "0.55";
            NATO_blue = cfg.CreateEntry<float>("NATO Blue", 0.18f);
            NATO_blue.Comment = "0.18";

            PACT_red = cfg.CreateEntry<float>("Pact Red", 1.57f);
            PACT_red.Comment = "1.57";
            PACT_green = cfg.CreateEntry<float>("Pact Green", 6f);
            PACT_green.Comment = "6";
            PACT_blue = cfg.CreateEntry<float>("Pact Blue", 0.63f);
            PACT_blue.Comment = "0.63";

            frequency = cfg.CreateEntry<int>("Tracer frequency", 5);
            frequency.Comment = "default '5' for 1-in-5; 0 for none";

            NATO_colour = new Vector4(NATO_red.Value, NATO_green.Value, NATO_blue.Value, 1f);
            PACT_colour = new Vector4(PACT_red.Value, PACT_green.Value, PACT_blue.Value, 1f);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "t64_menu" || sceneName == "MainMenu2-1_Scene")
            {
                return;
            }

            gameManager = GameObject.Find("_APP_GHPC_");
            if (gameManager == null) { return; }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Tracers), GameStatePriority.Medium);
        }

        public IEnumerator Tracers(GameState _)
        {
            LiveRoundMarshaller marshaller = gameManager.transform.Find("LiveRoundMarshaller").GetComponent<LiveRoundMarshaller>();
            LiveRoundMarshaller.LiveRoundVisualInfo[] visuals = marshaller.LiveRoundVisuals;

            foreach (var visual in visuals)
            {
                if (visual.Prefab.name == "MG shot Red")
                {
                    GameObject redBullet = visual.Prefab.transform.Find("tracer red small").gameObject;
                    redBullet.transform.Find("Cylinder").GetComponent<MeshRenderer>().material.SetVector("_HDRTint", NATO_colour);
                    redBullet.transform.Find("Point Light").GetComponent<Light>().color = NATO_colour;
                }
                
                if (visual.Prefab.name == "MG shot Green")
                {
                    GameObject greenBullet = visual.Prefab.transform.Find("tracer red small").gameObject;
                    greenBullet.transform.Find("Cylinder").GetComponent<MeshRenderer>().material.SetVector("_HDRTint", PACT_colour);
                    greenBullet.transform.Find("Point Light").GetComponent<Light>().color = PACT_colour;
                }
            }

            marshaller.LiveRoundPools[LiveRoundMarshaller.LiveRoundVisualType.Bullet].Clear();
            marshaller.LiveRoundPools[LiveRoundMarshaller.LiveRoundVisualType.GreenBullet].Clear();
            
            AmmoFeed[] feeds = Object.FindObjectsByType<AmmoFeed>(FindObjectsSortMode.None);            
            foreach (AmmoFeed feed in feeds)
            {                
                //if (feed.LoadedClipType == null) continue;
                
                AmmoCodexScriptable[] codices = feed.LoadedClipType.MinimalPattern;

                if (frequency.Value != 5 && feed.LoadedClipType.MinimalPattern.Length == 5)
                {                    
                    AmmoCodexScriptable ball = feed.LoadedClipType.MinimalPattern[0];
                    AmmoCodexScriptable tracer = feed.LoadedClipType.MinimalPattern[4];                    

                    if (frequency.Value == 0)
                    {
                        AmmoCodexScriptable[] tracerlessBelt = new AmmoCodexScriptable[1];
                        tracerlessBelt[0] = ball;
                        feed.LoadedClipType.MinimalPattern = tracerlessBelt;
                    }
                    else 
                    { 
                        AmmoCodexScriptable[] newBelt = new AmmoCodexScriptable[frequency.Value];
                        for (int i = 0; i < frequency.Value; i++)
                        {
                            if (i == (frequency.Value - 1)) newBelt[i] = tracer; else newBelt[i] = ball;
                        }
                        feed.LoadedClipType.MinimalPattern = newBelt;
                    }
                    
                    feed.LoadedClip.Clear();                    
                    for (int i = 0; i < feed._queuedClipTypeLockedIn.Capacity; i++)
                    {
                        int num = i % feed._queuedClipTypeLockedIn.MinimalPattern.Length;
                        feed.LoadedClip.Enqueue(feed._queuedClipTypeLockedIn.MinimalPattern[num].AmmoType);
                    }                    
                }
                
                foreach (AmmoCodexScriptable codex in codices)
                {                        
                    if (codex.name != "ammo_762x54R_tracer" && codex.name != "ammo_BZT44") continue;
                    codex.AmmoType.VisualType = LiveRoundMarshaller.LiveRoundVisualType.GreenBullet;
                }
                                
            }
            yield break;
        }
    }
}
