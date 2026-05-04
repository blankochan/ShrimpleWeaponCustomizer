using MelonLoader;
using Il2CppQuantum.Core;
using Il2CppQuantum;
using Il2CppPhoton.Deterministic;
using Il2Cpp;
using HarmonyLib;

[assembly: MelonInfo(typeof(ShrimpleWeaponCustomizer.Core), "ShrimpleWeaponCustomizer", "0.0.3", "blankochan")]
[assembly: MelonGame("Videocult", "Airframe")]
[assembly: MelonAdditionalDependencies("ShrimpleNetworkingAPI")]

namespace ShrimpleWeaponCustomizer;

public class Core : MelonMod
{
    public static ShrimpleNetworkingAPI.Registration.NetworkingMetadata NetworkingMetadata;
    public const string WeaponPropertyKey = "ShrimpleWeaponCustomizer_CustomWeaponStats";
    public static string LocalStatsPath = $"{MelonLoader.Utils.MelonEnvironment.UserDataDirectory}/CustomizableWeapons.json";
    public override void OnLateInitializeMelon()
    {
        if (!MelonMod.RegisteredMelons.Any(melon => melon.Info.Name == "ShrimpleNetworkingAPI" && melon.Info.Version == "0.1.0"))
        {
            this.Unregister("ShrimpleNetworkingAPI Is missing or has a mismatched version.");
            return;
        }
        NetworkingMetadata = new()
        {
            Identifer = $"{BuildInfo.Author}.{BuildInfo.Name}",
            Version = BuildInfo.Version,
            RequiredForJoin = true,
            UseStrictVersioning = false,
        };
        ShrimpleNetworkingAPI.Registration.TryRegister(NetworkingMetadata);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName is "Splashes")
            if (!File.Exists(LocalStatsPath))
            {
                LoggerInstance.Msg("Generating Default Weapon Stats");
                File.WriteAllText(LocalStatsPath, SerializableWeaponStats.GenerateStats().ToJson(minify: false));
            }

        if (sceneName is not "Bootstrap" or "Splashes")
        {
            if (PhotonController.instance.client.InRoom)
                UpdateWeaponStats();
        }
    }

    public static void UpdateWeaponStats()
    {
        if (PhotonController.instance.client.CurrentRoom is Il2CppPhoton.Realtime.Room currentRoom)
        {
            var client = PhotonController.instance.client;
            if (client.LocalPlayer.IsMasterClient || currentRoom.IsOffline) // host or offline
            {
                if (SerializableWeaponStats.TryGetLocalWeaponStats(out var stats))
                {
                    Il2CppPhoton.Client.PhotonHashtable customProp = new(1);
                    customProp[Core.WeaponPropertyKey] = CompressionUtils.Compress(stats.ToBinary()).ToIl2CppStructArray<byte>().Cast<Il2CppSystem.Object>();
                    currentRoom.SetCustomProperties(customProp);
                    stats.UpdateNativeStats();
                    return;
                }
            }
            else // client
            {
                if (currentRoom.CustomProperties.TryGetValue(Core.WeaponPropertyKey, out var bin))
                    if (SerializableWeaponStats.TryLoadFromBinary(bin.Cast<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>(), isCompressed: true, out var stats))
                    {
                        stats.UpdateNativeStats();
                        return;
                    }

            }
            return;
        }
        return;

    }

#if DEBUG
    //[HarmonyPatch(typeof(Il2CppQuantum.Core.FrameContext), nameof(Il2CppQuantum.Core.FrameContext.OnFrameSimulationBegin))]
    static class FrameContext_OnFrameSimulationBegin__patch
    {
        static readonly FP fixedBoostingAmount = FP.FromFloat_UNSAFE(0.04361f);
        static void Postfix(FrameBase f)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name is not "Warehouse") return;
            Il2CppSystem.Collections.Generic.List<EntityRef> entityRefs = new();
            f.GetAllEntityRefs(entityRefs);
            Frame frame = f.Cast<Frame>();
            EquipmentConfig equipmentConfig = f._context.ResourceManager.GetAsset(FrameRuntimeConfigExtensions.GameConfig(frame).equipmentConfig.Id).Cast<EquipmentConfig>();

            {
                unsafe
                {

                    foreach (EntityRef entityRef in entityRefs)
                    {
                        if (f.Has<Player>(entityRef))
                        {
                            if (f.Has<Health>(entityRef))
                            {
                                Health* playerHealth = f.GetPointer<Health>(entityRef);
                                playerHealth->health = FP._1;
                                playerHealth->healthCapacity = 100;

                            }
                            Player* playerStruct = f.GetPointer<Player>(entityRef);
                            var cameraState = playerStruct->cameraState;
                            cameraState.firstPerson.Value = 1;
                            cameraState.firstPerson = true;
                            playerStruct->cameraState = cameraState;
                            playerStruct->cameraGoalState.type = CameraGoalStateType.Spectator;
                        }
                        if (f.Has<HoverBike>(entityRef))
                        {
                            HoverBike* bike = f.GetPointer<HoverBike>(entityRef);
                            bike->boosts = 102;
                        }
                    }
                }
            }
        }
    }
#endif 
}
