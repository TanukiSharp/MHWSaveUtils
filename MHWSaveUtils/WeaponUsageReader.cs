using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSaveUtils
{
    public class WeaponUsageReader : SaveDataReaderBase
    {
        public WeaponUsageReader(Stream saveData)
            : base(saveData)
        {
        }

        public IEnumerable<WeaponUsageSaveSlotInfo> Read()
        {
            GotoSection3PastSignature();

            Skip(
                4 + // SAVEFILE.data.section.unknown
                8 + // SAVEFILE.data.section.sectionSize
                4   // SAVEFILE.data.section.sectionData[2].unknown (sectionData_3.unknown)
            );

            for (int i = 0; i < 3; i++)
            {
                WeaponUsageSaveSlotInfo saveSlotInfo = ReadSaveSlot(i + 1);
                if (saveSlotInfo != null)
                    yield return saveSlotInfo;
            }
        }

        // Slot 0 Active @ 0x3F3D64
        // Slot 1 Active @ 0x4E9E74
        // Slot 2 Active @ 0x5DFF84

        private WeaponUsageSaveSlotInfo ReadSaveSlot(int slotNumber)
        {
            BaseSaveSlotInfo baseSaveSlotInfo = ReaderUntilPlaytimeIncluded(slotNumber);

            Skip(
                4 + // unknown
                Constants.HunterAppearanceStructureSize + // H_APPEARANCE
                Constants.PalicoAppearanceStructureSize // P_APPEARANCE
            );

            // Here is struct GUILDCARD

            Skip(
                167 + // begining of GUILDCARD struct
                Constants.HunterAppearanceStructureSize + // hunterAppearance (H_APPEARANCE)
                Constants.PalicoAppearanceStructureSize + // palicoAppearance (P_APPEARANCE)
                Constants.HunterEquipmentStructureSize + // hunterEquipment
                92 + // unknown
                Constants.PalicoStructureSize + // struct palico
                63 // remaining of the struct GUILDCARD until weapon usage
            );

            var lowRankWeaponUsage = WeaponUsage.Read(reader);
            var highRankWeaponUsage = WeaponUsage.Read(reader);
            var investigationsWeaponUsage = WeaponUsage.Read(reader);

            // Skip the remaining of the GUILDCARD structure
            Skip(
                1 + // poseID
                1 + // expressionID
                1 + // backgroundID
                1 + // stickerID
                256 + // greeting
                256 + // title
                2 + // titleFirst
                2 + // titleMiddle
                2 + // titleLast
                4 + // positionX
                4 + // positionY
                4 + // zoom
                10 * Constants.ArenaStatsStructSize + // arenaRecords
                4 * Constants.Creatures16StructSize + // creatureStats
                Constants.Creatures8StructSize // researchLevel
            );

            // Skip the remaining of the saveSlot structure
            Skip(
                Constants.GuildCardStructureSize * 100 + // sharedGC
                0x019e36 + // unknown
                Constants.ItemLoadoutsStructureSize + // itemLoadouts
                8 + //  unknown
                Constants.ItemPouchStructureSize + // itemPouch
                Constants.ItemBoxStructureSize + // itemBox
                0x034E3C + // unknown
                42 * 250 + // investigations
                0x0FB9 + // unknown
                Constants.EquipLoadoutsStructureSize + // equipLoadout
                0x6521 + // unknown
                Constants.DlcTypeSize * 256 + // DLCClaimed
                0x2A5D // unknown
            );

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            return new WeaponUsageSaveSlotInfo(
                baseSaveSlotInfo,
                lowRankWeaponUsage,
                highRankWeaponUsage,
                investigationsWeaponUsage
            );
        }
    }

    public class WeaponUsageSaveSlotInfo : BaseSaveSlotInfo
    {
        public WeaponUsage LowRank { get; }
        public WeaponUsage HighRank { get; }
        public WeaponUsage Investigations { get; }

        public WeaponUsageSaveSlotInfo(
            BaseSaveSlotInfo baseSaveSlotInfo,
            WeaponUsage lowRank, WeaponUsage highRank, WeaponUsage investigations)
            : base(baseSaveSlotInfo)
        {
            LowRank = lowRank;
            HighRank = highRank;
            Investigations = investigations;
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
