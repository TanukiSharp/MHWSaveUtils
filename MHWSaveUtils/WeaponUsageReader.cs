using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSaveUtils
{
    public class WeaponUsageReader : SaveDataReaderBase<WeaponUsageSaveSlotInfo>
    {
        public WeaponUsageReader(Stream saveData)
            : base(saveData)
        {
        }

        public override IEnumerable<WeaponUsageSaveSlotInfo> Read()
        {
            GotoSection3PastSignature();

            Skip(
                4 + // SAVEFILE.data.section.unknown
                8 // sectionSize
            );

            for (int i = 0; i < 3; i++)
            {
                WeaponUsageSaveSlotInfo saveSlotInfo = ReadSaveSlot(i + 1);
                if (saveSlotInfo != null)
                    yield return saveSlotInfo;
            }
        }

        private WeaponUsageSaveSlotInfo ReadSaveSlot(int slotNumber)
        {
            Skip(4); // unknown

            SaveSlotInfoBase baseSaveSlotInfo = ReadUntilPlaytimeIncluded(slotNumber);

            Skip(
                Constants.HunterAppearanceStructureSize + // H_APPEARANCE
                382 + // unknown
                Constants.PalicoAppearanceStructureSize // P_APPEARANCE
            );

            // Here is struct GUILDCARD

            Skip(
                171 + // begining of GUILDCARD struct
                Constants.HunterAppearanceStructureSize + // hunterAppearance (H_APPEARANCE)
                212 + // unknown
                64 + // Palico name
                4 + // Palico rank
                194 // unknown
            );

            var lowRankWeaponUsage = WeaponUsage.Read(reader);
            var highRankWeaponUsage = WeaponUsage.Read(reader);
            var investigationsWeaponUsage = WeaponUsage.Read(reader);
            var masterRankWeaponUsage = WeaponUsage.Read(reader);
            var guidingLandsWeaponUsage = WeaponUsage.Read(reader);

            Skip(
                4 + // unknown
                2_134_609 + // 2_136_256 (total size of a save slot) - 1647 (bytes read and skipped so far)
                512 // Hash things
            );

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            return new WeaponUsageSaveSlotInfo(
                baseSaveSlotInfo,
                lowRankWeaponUsage,
                highRankWeaponUsage,
                masterRankWeaponUsage,
                investigationsWeaponUsage,
                guidingLandsWeaponUsage
            );
        }
    }

    public class WeaponUsageSaveSlotInfo : SaveSlotInfoBase
    {
        public WeaponUsage LowRank { get; }
        public WeaponUsage HighRank { get; }
        public WeaponUsage MasterRank { get; }
        public WeaponUsage Investigations { get; }
        public WeaponUsage GuidingLands { get; }

        public WeaponUsageSaveSlotInfo(
            SaveSlotInfoBase baseSaveSlotInfo,
            WeaponUsage lowRank, WeaponUsage highRank, WeaponUsage masterRank, WeaponUsage investigations, WeaponUsage guidingLands)
            : base(baseSaveSlotInfo)
        {
            LowRank = lowRank;
            HighRank = highRank;
            MasterRank = masterRank;
            Investigations = investigations;
            GuidingLands = guidingLands;
        }
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct WeaponUsage
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public ushort GreatSword { get; private set; }
        public ushort LongSword { get; private set; }
        public ushort SwordAndShield { get; private set; }
        public ushort DualBlades { get; private set; }
        public ushort Hammer { get; private set; }
        public ushort HuntingHorn { get; private set; }
        public ushort Lance { get; private set; }
        public ushort Gunlance { get; private set; }
        public ushort SwitchAxe { get; private set; }
        public ushort ChargeBlade { get; private set; }
        public ushort InsectGlaive { get; private set; }
        public ushort LightBowgun { get; private set; }
        public ushort HeavyBowgun { get; private set; }
        public ushort Bow { get; private set; }

        public static WeaponUsage Read(BinaryReader reader)
        {
            return new WeaponUsage
            {
                GreatSword = reader.ReadUInt16(),
                LongSword = reader.ReadUInt16(),
                SwordAndShield = reader.ReadUInt16(),
                DualBlades = reader.ReadUInt16(),
                Hammer = reader.ReadUInt16(),
                HuntingHorn = reader.ReadUInt16(),
                Lance = reader.ReadUInt16(),
                Gunlance = reader.ReadUInt16(),
                SwitchAxe = reader.ReadUInt16(),
                ChargeBlade = reader.ReadUInt16(),
                InsectGlaive = reader.ReadUInt16(),
                LightBowgun = reader.ReadUInt16(),
                HeavyBowgun = reader.ReadUInt16(),
                Bow = reader.ReadUInt16()
            };
        }

        public static WeaponUsage operator +(WeaponUsage lhs, WeaponUsage rhs)
        {
            return new WeaponUsage
            {
                GreatSword = (ushort)(lhs.GreatSword + rhs.GreatSword),
                LongSword = (ushort)(lhs.LongSword + rhs.LongSword),
                SwordAndShield = (ushort)(lhs.SwordAndShield + rhs.SwordAndShield),
                DualBlades = (ushort)(lhs.DualBlades + rhs.DualBlades),
                Hammer = (ushort)(lhs.Hammer + rhs.Hammer),
                HuntingHorn = (ushort)(lhs.HuntingHorn + rhs.HuntingHorn),
                Lance = (ushort)(lhs.Lance + rhs.Lance),
                Gunlance = (ushort)(lhs.Gunlance + rhs.Gunlance),
                SwitchAxe = (ushort)(lhs.SwitchAxe + rhs.SwitchAxe),
                ChargeBlade = (ushort)(lhs.ChargeBlade + rhs.ChargeBlade),
                InsectGlaive = (ushort)(lhs.InsectGlaive + rhs.InsectGlaive),
                LightBowgun = (ushort)(lhs.LightBowgun + rhs.LightBowgun),
                HeavyBowgun = (ushort)(lhs.HeavyBowgun + rhs.HeavyBowgun),
                Bow = (ushort)(lhs.Bow + rhs.Bow)
            };
        }
    }
}
