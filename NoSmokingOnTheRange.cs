using GHPC.Equipment;
using GHPC.State;
using GHPC.Player;
using MelonLoader;
using No_Smoking_on_the_Range;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(NoSmokeOnTheRange), "No Smoking on the Range", "1.0.1", "Bluehawk")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace No_Smoking_on_the_Range
{
    public class NoSmokeOnTheRange : MelonMod
    {        
        public static GameObject gameManager;
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu2_Scene" || sceneName == "t64_menu" || sceneName == "MainMenu2-1_Scene")
            {                
                return;
            }

            gameManager = GameObject.Find("_APP_GHPC_");
            if (gameManager == null) { return; }

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(Desmoke), GameStatePriority.Lowest);
        }

        public IEnumerator Desmoke(GameState _)
        {            
            if (gameManager.GetComponent<PlayerInput>().MapController.name != "TR01 Topo Map") { yield break; }

            VehicleSmokeManager[] smokes = GameObject.FindObjectsByType<VehicleSmokeManager>(FindObjectsSortMode.None);
            foreach (var vsm in smokes)
            {
                bool skip = false;
                var grenades = vsm._smokeSlots;
                if (grenades.Length == 0) continue;
                foreach (VehicleSmokeManager.SmokeSlot slot in grenades)
                {
                    if (slot.Rounds == 0) skip = true;
                    slot.Rounds = 0;
                }
                if (!skip) MelonLogger.Msg(vsm._unitInfoBroker.Unit.name + " has been extinguished.");
            }

            yield break;
        }
    }
}