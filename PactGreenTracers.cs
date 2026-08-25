using System.Collections;
using PactGreenTracers;
using MelonLoader;
using UnityEngine;
using GHPC.Weapons;
using GHPC.Weaponry;
using GHPC.State;

[assembly: MelonInfo(typeof(PactGreenTracersClass), "Pact Green Tracers", "1.0.0", "Bluehawk")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace PactGreenTracers
{
    public class PactGreenTracersClass : MelonMod
    {
        public GameObject gameManager;
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
            AmmoFeed[] feeds = Object.FindObjectsByType<AmmoFeed>(FindObjectsSortMode.None);            
            foreach (AmmoFeed feed in feeds)
            {                
                if (feed.LoadedClipType != null)
                {
                    AmmoCodexScriptable[] codices = feed.LoadedClipType.MinimalPattern;
                    foreach (AmmoCodexScriptable codex in codices)
                    {                        
                        if (codex.name != "ammo_762x54R_tracer" && codex.name != "ammo_BZT44") continue;
                        AmmoType sovTracer = codex.AmmoType;
                        if (sovTracer.VisualType == LiveRoundMarshaller.LiveRoundVisualType.GreenBullet) continue;
                        else
                        {
                            sovTracer.VisualType = LiveRoundMarshaller.LiveRoundVisualType.GreenBullet;
                            Light light = sovTracer.ShotVisual.GetComponent<Light>();
                            light.color = new Color(0f, 0.23f, 1f, 1f);
                        }
                        //tracer prefab material stores colour in vector "HDRTint"
                    }
                }                
            }
            yield break;
        }
    }
}
