using HarmonyLib;
using MelonLoader;
using Il2CppPhoton.Realtime;
namespace ShrimpleWeaponCustomizer.Patches;

static class Patches
{

    static System.Collections.IEnumerator updateStatsCoroutine(Il2CppPhoton.Realtime.RealtimeClient client)
    {
        int guard = 100 * 60;
        while (true) // shitty way of waiting for the network to connect
        {
            yield return null;
            guard--;
            if (guard < 0)
            {
                Melon<Core>.Logger.Warning("Room connection timeout, custom weapon properties were not set/sent");
                yield break;
            }
            if (client.IsConnectedAndReady && client.InRoom)
            {
                Core.UpdateWeaponStats();
                yield break;
            }
        }
    }
    [HarmonyPatch(typeof(RealtimeClient), nameof(RealtimeClient.OpCreateRoom))]
    static class RealtimeClient_OpCreateRoom_Patch
    {
        public static void Postfix(Il2CppPhoton.Realtime.RealtimeClient __instance, bool __result, Il2CppPhoton.Realtime.EnterRoomArgs __0)
        {
            MelonCoroutines.Start(updateStatsCoroutine(__instance));
        }
    }

    [HarmonyPatch(typeof(RealtimeClient), nameof(RealtimeClient.OpJoinRoom))]
    static class RealtimeClient_OpJoinRoom_Patch
    {
        public static void Postfix(Il2CppPhoton.Realtime.RealtimeClient __instance, bool __result, Il2CppPhoton.Realtime.EnterRoomArgs __0)
        {
            MelonCoroutines.Start(updateStatsCoroutine(__instance));
        }
    }
}

