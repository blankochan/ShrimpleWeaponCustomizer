using Il2CppQuantum_Weapons;
using MemoryPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Il2CppPhoton.Deterministic;
namespace ShrimpleWeaponCustomizer;

public static class FPExtensions
{
    public static FP ToFP(this float flt) => FP.FromFloat_UNSAFE(flt);
}
[MemoryPackable]
public partial struct SerializableVector2
{
    public float x, y;

    public static implicit operator SerializableVector2(FPVector2 vec)
    {
        return new() { x = vec.X.AsFloat, y = vec.Y.AsFloat };
    }

    public static implicit operator FPVector2(SerializableVector2 vec)
    {
        return new(vec.x.ToFP(), vec.y.ToFP());
    }
}

[MemoryPackable]
public partial struct SerializableVector3
{
    public float x, y, z;

    public static implicit operator SerializableVector3(FPVector3 vec)
    {
        return new() { x = vec.X.AsFloat, y = vec.Y.AsFloat, z = vec.Z.AsFloat };
    }

    public static implicit operator FPVector3(SerializableVector3 vec)
    {
        return new(vec.x.ToFP(), vec.y.ToFP(), vec.z.ToFP());
    }
}

[MemoryPackable]
public partial struct SerializableDamageData
{
    public float organicDamage;
    public float machineDamage;
    public float pushImpulse;
    public float stunFac;
    public float unsaddleFac;
    public float screenShakeFac;
    [JsonConverter(typeof(StringEnumConverter))]
    public Il2CppQuantum.DamageType damageType;
    [JsonConverter(typeof(StringEnumConverter))]
    public Il2CppQuantum.DamageDecalID damageDecal;
    public int damageSourceID;

    public static SerializableDamageData FromNative(Il2CppQuantum.DamageData data)
    {
        var s = new SerializableDamageData();
        s.organicDamage = data.organicDamage.AsFloat;
        s.machineDamage = data.machineDamage.AsFloat;
        s.pushImpulse = data.pushImpulse.AsFloat;
        s.stunFac = data.stunFac.AsFloat;
        s.unsaddleFac = data.unsaddleFac.AsFloat;
        s.screenShakeFac = data.screenShakeFac.AsFloat;
        s.damageType = data.damageType;
        s.damageDecal = data.damageDecal;
        s.damageSourceID = data.damageSourceID;
        return s;
    }

    public Il2CppQuantum.DamageData ToNative()
    {
        var s = new Il2CppQuantum.DamageData();
        s.organicDamage = organicDamage.ToFP();
        s.machineDamage = machineDamage.ToFP();
        s.pushImpulse = pushImpulse.ToFP();
        s.stunFac = stunFac.ToFP();
        s.unsaddleFac = unsaddleFac.ToFP();
        s.screenShakeFac = screenShakeFac.ToFP();
        s.damageType = damageType;
        s.damageDecal = damageDecal;
        s.damageSourceID = damageSourceID;
        return s;
    }
}
#pragma warning disable CS8618

[MemoryPackable]
public partial class SerializableWeaponStats
{

    public float shurikenThrowSpeed;
    public float shurikenBend;
    public float brickThrowSpeed_onBike;
    public float brickThrowSpeed_onFoot;
    public float brickGravity;
    public float macheteThrowSpeed;
    public float pushFactor;

    public Dictionary<Il2CppQuantum.EquipmentID, SerializableGenericWeaponStats> stats;


    public string ToJson(bool minify)
    {
        if (minify)
        {
#pragma warning disable CS8600
            SerializableWeaponStats minifedStats = MemberwiseClone() as SerializableWeaponStats;
            Dictionary<Il2CppQuantum.EquipmentID, SerializableGenericWeaponStats> newStats = new(stats.Count);
            foreach (var statPair in stats)
            {
                if (statPair.Value.OverrideEnabled) newStats[statPair.Key] = statPair.Value;
            }
            return JsonConvert.SerializeObject(minifedStats, Formatting.None);
        }
        else return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    public byte[] ToBinary()
    {
#pragma warning disable CS8600
        SerializableWeaponStats minifedStats = this.MemberwiseClone() as SerializableWeaponStats;
        Dictionary<Il2CppQuantum.EquipmentID, SerializableGenericWeaponStats> newStats = new(stats.Count);
        foreach (var statPair in stats)
        {
            if (statPair.Value.OverrideEnabled) newStats[statPair.Key] = statPair.Value;
        }
        return MemoryPackSerializer.Serialize(minifedStats);
    }

    public static bool TryLoadFromBinary(byte[] data, bool isCompressed, out SerializableWeaponStats stats)
    {
        if (isCompressed)
            data = CompressionUtils.Decompress(data);
        if (MemoryPackSerializer.Deserialize<SerializableWeaponStats>(data) is SerializableWeaponStats weaponStats)
        {
            stats = weaponStats;
            return true;
        }

        stats = null;
        return false;
    }

    public static bool TryLoadFromJson(string json, out SerializableWeaponStats stats)
    {
        if (JsonConvert.DeserializeObject<SerializableWeaponStats>(json) is SerializableWeaponStats weaponStats)
        {
            stats = weaponStats;
            return true;
        }
        stats = null;
        return false;
    }

    public void UpdateNativeStats()
    {
        WeaponStats.shurikenThrowSpeed = shurikenThrowSpeed.ToFP();
        WeaponStats.shurikenBend = shurikenBend.ToFP();
        WeaponStats.brickThrowSpeed_onBike = brickThrowSpeed_onBike.ToFP();
        WeaponStats.brickThrowSpeed_onFoot = brickThrowSpeed_onFoot.ToFP();
        WeaponStats.brickGravity = brickGravity.ToFP();
        WeaponStats.macheteThrowSpeed = macheteThrowSpeed.ToFP();
        WeaponStats.pushFactor = pushFactor.ToFP();
        foreach (var statPair in stats)
        {
            if (statPair.Value.OverrideEnabled)
            {
                var nativeStat = WeaponStats.stats[(int)statPair.Key];
                nativeStat.gunStats = statPair.Value.gunStats.ToNative();
                nativeStat.meleeStats = statPair.Value.meleeStats.ToNative();
                nativeStat.secondaryStats = statPair.Value.secondaryStats.ToNative();
                nativeStat.onlyDamageWeaponStats = statPair.Value.onlyDamageWeaponStats.ToNative();

                WeaponStats.stats[(int)statPair.Key] = nativeStat;
            }
        }
    }

    public static bool TryGetLocalWeaponStats(out SerializableWeaponStats stats)
    {
        if (File.Exists(Core.LocalStatsPath))
        {
            string json = File.ReadAllText(Core.LocalStatsPath);
            if (SerializableWeaponStats.TryLoadFromJson(json, out var weaponStats))
            {
                stats = weaponStats;
                return true;
            }
        }
#pragma warning disable CS8625 
        stats = null;
#pragma warning restore CS8625
        return false;
    }
    public static SerializableWeaponStats GenerateStats()
    {
        SerializableWeaponStats s = new();
        s.shurikenThrowSpeed = WeaponStats.shurikenThrowSpeed.AsFloat;
        s.shurikenBend = WeaponStats.shurikenBend.AsFloat;
        s.brickThrowSpeed_onBike = WeaponStats.brickThrowSpeed_onBike.AsFloat;
        s.brickThrowSpeed_onFoot = WeaponStats.brickThrowSpeed_onFoot.AsFloat;
        s.brickGravity = WeaponStats.brickGravity.AsFloat;
        s.macheteThrowSpeed = WeaponStats.macheteThrowSpeed.AsFloat;
        s.pushFactor = WeaponStats.pushFactor.AsFloat;

        s.stats = new(WeaponStats.stats.Count);

        for (int equipmentIndex = 0; equipmentIndex < WeaponStats.stats.Count; equipmentIndex++)
        {
            Il2CppQuantum.EquipmentID equipmentID = (Il2CppQuantum.EquipmentID)equipmentIndex;
            if (Enum.IsDefined<Il2CppQuantum.EquipmentID>(equipmentID))
                s.stats.Add(equipmentID, SerializableWeaponStats.SerializableGenericWeaponStats.FromNative(WeaponStats.stats[equipmentIndex]));
        }

        return s;
    }

    [MemoryPackable]
    public partial struct SerializableGenericWeaponStats
    {
        [MemoryPackIgnore]
        public bool OverrideEnabled;

        public SerializableWeaponStats.SerializableMeleeWeaponStats meleeStats;
        public SerializableWeaponStats.SerializableGunWeaponStats gunStats;
        public SerializableWeaponStats.SerializableSecondaryWeaponStats secondaryStats;
        public SerializableWeaponStats.SerializableOnlyDamageWeaponStats onlyDamageWeaponStats;

        public static SerializableWeaponStats.SerializableGenericWeaponStats FromNative(WeaponStats.GenericWeaponStats stats)
        {
            var s = new SerializableWeaponStats.SerializableGenericWeaponStats();
            s.meleeStats = SerializableWeaponStats.SerializableMeleeWeaponStats.FromNative(stats.meleeStats);
            s.gunStats = SerializableWeaponStats.SerializableGunWeaponStats.FromNative(stats.gunStats);
            s.secondaryStats = SerializableWeaponStats.SerializableSecondaryWeaponStats.FromNative(stats.secondaryStats);
            s.onlyDamageWeaponStats = SerializableWeaponStats.SerializableOnlyDamageWeaponStats.FromNative(stats.onlyDamageWeaponStats);
            return s;
        }
        public WeaponStats.GenericWeaponStats ToNative()
        {
            var s = new WeaponStats.GenericWeaponStats();

            var melee = meleeStats.ToNative();
            s.meleeStats = melee;
            _ = s.meleeStats.windupTime.AsFloat;

            var gun = gunStats.ToNative();
            s.gunStats = gun;
            _ = s.gunStats.projectileSpeed.AsFloat;

            var secondary = secondaryStats.ToNative();
            s.secondaryStats = secondary;
            _ = s.secondaryStats.explosionRadius.AsFloat;

            var onlyDamage = onlyDamageWeaponStats.ToNative();
            s.onlyDamageWeaponStats = onlyDamage;
            _ = s.onlyDamageWeaponStats.damage.organicDamage.AsFloat;

            return s;
        }
    }

    [MemoryPackable]
    public partial struct SerializableMeleeWeaponStats
    {
        public SerializableDamageData damage;
        public float windupTime;
        public float swingTime;
        public float coolDownTime;
        public float postHitTime;
        public float swingProgPow;
        public float collPointA;
        public float collPointB;
        public float collPointRad;
        public int collPoints;
        public int fakeExtraRange;
        public float dashRange;
        public float forwardDash;
        public float hitRumbleStrength;
        public float hitRumbleTime;
        public float meleeSwingSize;
        public float swingZRotat;
        public float hitShakeScreen;
        public float speedDamagePower;
        public float twoHandedBonus;
        public float baseDamageFactor;
        public float blockSize;
        public float blockTimeWindow;
        public SerializableVector2 verticalSwingFactor;

        public static SerializableWeaponStats.SerializableMeleeWeaponStats FromNative(WeaponStats.MeleeWeaponStats stats)
        {
            var s = new SerializableWeaponStats.SerializableMeleeWeaponStats();
            s.damage = SerializableDamageData.FromNative(stats.damage);
            s.windupTime = stats.windupTime.AsFloat;
            s.swingTime = stats.swingTime.AsFloat;
            s.coolDownTime = stats.coolDownTime.AsFloat;
            s.postHitTime = stats.postHitTime.AsFloat;
            s.swingProgPow = stats.swingProgPow.AsFloat;
            s.collPointA = stats.collPointA.AsFloat;
            s.collPointB = stats.collPointB.AsFloat;
            s.collPointRad = stats.collPointRad.AsFloat;
            s.collPoints = stats.collPoints;
            s.fakeExtraRange = stats.fakeExtraRange;
            s.dashRange = stats.dashRange.AsFloat;
            s.forwardDash = stats.forwardDash.AsFloat;
            s.hitRumbleStrength = stats.hitRumbleStrength.AsFloat;
            s.hitRumbleTime = stats.hitRumbleTime.AsFloat;
            s.meleeSwingSize = stats.meleeSwingSize.AsFloat;
            s.swingZRotat = stats.swingZRotat.AsFloat;
            s.hitShakeScreen = stats.hitShakeScreen.AsFloat;
            s.speedDamagePower = stats.speedDamagePower.AsFloat;
            s.twoHandedBonus = stats.twoHandedBonus.AsFloat;
            s.baseDamageFactor = stats.baseDamageFactor.AsFloat;
            s.blockSize = stats.blockSize.AsFloat;
            s.blockTimeWindow = stats.blockTimeWindow.AsFloat;
            s.verticalSwingFactor = stats.verticalSwingFactor;
            return s;
        }

        public WeaponStats.MeleeWeaponStats ToNative()
        {
            var s = new WeaponStats.MeleeWeaponStats();
            s.damage = damage.ToNative();
            s.windupTime = windupTime.ToFP();
            s.swingTime = swingTime.ToFP();
            s.coolDownTime = coolDownTime.ToFP();
            s.postHitTime = postHitTime.ToFP();
            s.swingProgPow = swingProgPow.ToFP();
            s.collPointA = collPointA.ToFP();
            s.collPointB = collPointB.ToFP();
            s.collPointRad = collPointRad.ToFP();
            s.collPoints = collPoints;
            s.fakeExtraRange = fakeExtraRange;
            s.dashRange = dashRange.ToFP();
            s.forwardDash = forwardDash.ToFP();
            s.hitRumbleStrength = hitRumbleStrength.ToFP();
            s.hitRumbleTime = hitRumbleTime.ToFP();
            s.meleeSwingSize = meleeSwingSize.ToFP();
            s.swingZRotat = swingZRotat.ToFP();
            s.hitShakeScreen = hitShakeScreen.ToFP();
            s.speedDamagePower = speedDamagePower.ToFP();
            s.twoHandedBonus = twoHandedBonus.ToFP();
            s.baseDamageFactor = baseDamageFactor.ToFP();
            s.blockSize = blockSize.ToFP();
            s.blockTimeWindow = blockTimeWindow.ToFP();
            s.verticalSwingFactor = verticalSwingFactor;
            return s;
        }
    }

    [MemoryPackable]
    public partial struct SerializableGunWeaponStats
    {
        public SerializableDamageData damage;
        public bool continuousFire;
        public bool specialProjectile;
        public int magasineSize;
        public int startAmmo;
        public int pickupAmmo;
        [JsonConverter(typeof(StringEnumConverter))]
        public ReloadType reloadType;
        public int reloadTime;
        public int fireDelay;
        public float projectileSpeed;
        public int projectileLifeTime;
        public float randomSpread;
        public SerializableVector2 autoAimRadius_AimedOut;
        public SerializableVector2 autoAimRadius_AimedIn;
        public float onFoot_AutoAimRadiusFactor;
        public float autoAimSpeed;
        public float autoAimRange;
        public SerializableVector2 rumbleStrengthAndTime;
        public SerializableVector2 recoilAngleAdd;

        public static SerializableWeaponStats.SerializableGunWeaponStats FromNative(WeaponStats.GunWeaponStats stats)
        {
            var s = new SerializableWeaponStats.SerializableGunWeaponStats();
            s.damage = SerializableDamageData.FromNative(stats.damage);
            s.continuousFire = stats.continuousFire;
            s.specialProjectile = stats.specialProjectile;
            s.magasineSize = stats.magasineSize;
            s.startAmmo = stats.startAmmo;
            s.pickupAmmo = stats.pickupAmmo;
            s.reloadType = stats.reloadType;
            s.reloadTime = stats.reloadTime;
            s.fireDelay = stats.fireDelay;
            s.projectileSpeed = stats.projectileSpeed.AsFloat;
            s.projectileLifeTime = stats.projectileLifeTime;
            s.randomSpread = stats.randomSpread.AsFloat;
            s.autoAimRadius_AimedOut = stats.autoAimRadius_AimedOut;
            s.autoAimRadius_AimedIn = stats.autoAimRadius_AimedIn;
            s.onFoot_AutoAimRadiusFactor = stats.onFoot_AutoAimRadiusFactor.AsFloat;
            s.autoAimSpeed = stats.autoAimSpeed.AsFloat;
            s.autoAimRange = stats.autoAimRange.AsFloat;
            s.rumbleStrengthAndTime = stats.rumbleStrengthAndTime;
            s.recoilAngleAdd = stats.recoilAngleAdd;
            return s;
        }

        public WeaponStats.GunWeaponStats ToNative()
        {
            var s = new WeaponStats.GunWeaponStats();
            s.damage = damage.ToNative();
            s.continuousFire = continuousFire;
            s.specialProjectile = specialProjectile;
            s.magasineSize = magasineSize;
            s.startAmmo = startAmmo;
            s.pickupAmmo = pickupAmmo;
            s.reloadType = reloadType;
            s.reloadTime = reloadTime;
            s.fireDelay = fireDelay;
            s.projectileSpeed = projectileSpeed.ToFP();
            s.projectileLifeTime = projectileLifeTime;
            s.randomSpread = randomSpread.ToFP();
            s.autoAimRadius_AimedOut = autoAimRadius_AimedOut;
            s.autoAimRadius_AimedIn = autoAimRadius_AimedIn;
            s.onFoot_AutoAimRadiusFactor = onFoot_AutoAimRadiusFactor.ToFP();
            s.autoAimSpeed = autoAimSpeed.ToFP();
            s.autoAimRange = autoAimRange.ToFP();
            s.rumbleStrengthAndTime = rumbleStrengthAndTime;
            s.recoilAngleAdd = recoilAngleAdd;
            return s;
        }
    }
    [MemoryPackable]
    public partial struct SerializableSecondaryWeaponStats
    {
        public SerializableDamageData damage;
        public int startAmount;
        public int onFootStartAmount;
        public float throwAnimationSpeed;
        public int fuseTime;
        public int preFuseTime;
        public int explosionDurationTicks;
        public float explosionRadius;
        public float explosionUpwardsForceFac;

        public static SerializableWeaponStats.SerializableSecondaryWeaponStats FromNative(WeaponStats.SecondaryWeaponStats stats)
        {
            var s = new SerializableWeaponStats.SerializableSecondaryWeaponStats();
            s.damage = SerializableDamageData.FromNative(stats.damage);
            s.startAmount = stats.startAmount;
            s.onFootStartAmount = stats.onFootStartAmount;
            s.throwAnimationSpeed = stats.throwAnimationSpeed.AsFloat;
            s.fuseTime = stats.fuseTime;
            s.preFuseTime = stats.preFuseTime;
            s.explosionDurationTicks = stats.explosionDurationTicks;
            s.explosionRadius = stats.explosionRadius.AsFloat;
            s.explosionUpwardsForceFac = stats.explosionUpwardsForceFac.AsFloat;
            return s;
        }

        public WeaponStats.SecondaryWeaponStats ToNative()
        {
            var s = new WeaponStats.SecondaryWeaponStats();
            s.damage = damage.ToNative();
            s.startAmount = startAmount;
            s.onFootStartAmount = onFootStartAmount;
            s.throwAnimationSpeed = throwAnimationSpeed.ToFP();
            s.fuseTime = fuseTime;
            s.preFuseTime = preFuseTime;
            s.explosionDurationTicks = explosionDurationTicks;
            s.explosionRadius = explosionRadius.ToFP();
            s.explosionUpwardsForceFac = explosionUpwardsForceFac.ToFP();
            return s;
        }
    }

    [MemoryPackable]
    public partial struct SerializableOnlyDamageWeaponStats
    {
        public SerializableDamageData damage;
        public SerializableDamageData damageB;

        public static SerializableWeaponStats.SerializableOnlyDamageWeaponStats FromNative(WeaponStats.OnlyDamageWeaponStats stats)
        {
            var s = new SerializableWeaponStats.SerializableOnlyDamageWeaponStats();
            s.damage = SerializableDamageData.FromNative(stats.damage);
            s.damageB = SerializableDamageData.FromNative(stats.damageB);
            return s;
        }

        public WeaponStats.OnlyDamageWeaponStats ToNative()
        {
            var s = new WeaponStats.OnlyDamageWeaponStats();
            s.damage = damage.ToNative();
            s.damageB = damageB.ToNative();
            return s;
        }
    }
}
