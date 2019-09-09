using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSaveUtils
{
    public class MonsterStatsReader : SaveDataReaderBase<MonsterStatsSaveSlotInfo>
    {
        public MonsterStatsReader(Stream saveData)
            : base(saveData)
        {
        }

        public override IEnumerable<MonsterStatsSaveSlotInfo> Read()
        {
            GotoSection3PastSignature();

            Skip(
                4 + // SAVEFILE.data.section.unknown
                8 + // SAVEFILE.data.section.sectionSize
                4   // SAVEFILE.data.section.sectionData[2].unknown (sectionData_3.unknown)
            );

            for (int i = 0; i < 3; i++)
            {
                MonsterStatsSaveSlotInfo saveSlotInfo = ReadSaveSlot(i + 1);
                if (saveSlotInfo != null)
                    yield return saveSlotInfo;
            }
        }

        // Slot 0 Active @ 0x3F3D64
        // Slot 1 Active @ 0x4E9E74
        // Slot 2 Active @ 0x5DFF84

        private MonsterStatsSaveSlotInfo ReadSaveSlot(int slotNumber)
        {
            SaveSlotInfoBase baseSaveSlotInfo = ReadUntilPlaytimeIncluded(slotNumber);

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

            Skip(Constants.WeaponUsageStructureSize * 3);

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
                10 * Constants.ArenaStatsStructSize // arenaRecords
            );

            ushort[] captured = ReadMonsters16();
            ushort[] slayed = ReadMonsters16();
            ushort[] largest = ReadMonsters16();
            ushort[] smallest = ReadMonsters16();
            byte[] researchLevel = ReadMonsters8();

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

            var monsters = new MonsterStatsInfo[captured.Length];
            for (int i = 0; i < captured.Length; i++)
            {
                bool hasMini = HasMini(smallest[i], MonsterData[i]);
                bool hasGold = HasGold(largest[i], MonsterData[i]);
                bool hasSilver = hasGold ? true : HasSilver(largest[i], MonsterData[i]);

                monsters[i] = new MonsterStatsInfo
                {
                    Name = MonsterData[i].Name,
                    Captured = captured[i],
                    Slayed = slayed[i],
                    Largest = largest[i],
                    Smallest = smallest[i],
                    ResearchLevel = researchLevel[i],
                    HasCrowns = MonsterData[i].CrownSize != MonsterSize.None,
                    HasMiniCrown = hasMini,
                    HasSilverCrown = hasSilver,
                    HasGoldCrown = hasGold
                };
            }

            return new MonsterStatsSaveSlotInfo(baseSaveSlotInfo, monsters);
        }

        private const int AvailableMonsterCount = 36;
        private const int MaxMonsterCount = 64;

        private enum MonsterSize
        {
            None,
            Standard,
            Alternate
        }

        private const ushort Mini = 90;
        private const ushort SilverStandard = 115;
        private const ushort GoldStandard = 123;
        private const ushort SilverAlternate = 110;
        private const ushort GoldAlternate = 120;

        private bool HasMini(ushort smallest, MonsterBaseInfo monsterBaseInfo)
        {
            if (monsterBaseInfo.CrownSize == MonsterSize.None)
                return false;

            return smallest <= Mini;
        }

        private bool HasSilver(ushort largest, MonsterBaseInfo monsterBaseInfo)
        {
            if (monsterBaseInfo.CrownSize == MonsterSize.Standard)
                return largest >= SilverStandard;

            if (monsterBaseInfo.CrownSize == MonsterSize.Alternate)
                return largest >= SilverAlternate;

            return false;
        }

        private bool HasGold(ushort largest, MonsterBaseInfo monsterBaseInfo)
        {
            if (monsterBaseInfo.CrownSize == MonsterSize.Standard)
                return largest >= GoldStandard;

            if (monsterBaseInfo.CrownSize == MonsterSize.Alternate)
                return largest >= GoldAlternate;

            return false;
        }

        private struct MonsterBaseInfo
        {
            public string Name;
            public float BaseSize;
            public MonsterSize CrownSize;
        }

        private static readonly MonsterBaseInfo[] MonsterData =
        {
            new MonsterBaseInfo { Name = "Great Jagras", BaseSize = 1109.66f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Kulu-Ya-Ku", BaseSize = 901.24f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Pukei-Pukei", BaseSize = 1102.45f, CrownSize = MonsterSize.Alternate },
            new MonsterBaseInfo { Name = "Barroth", BaseSize = 1383.07f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Jyuratodus", BaseSize = 1508.71f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Tobi-Kadachi", BaseSize = 1300.52f, CrownSize = MonsterSize.Alternate },
            new MonsterBaseInfo { Name = "Anjanath", BaseSize = 1646.46f, CrownSize = MonsterSize.Alternate },
            new MonsterBaseInfo { Name = "Rathian", BaseSize = 1754.37f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Tzitzi-Ya-Ku", BaseSize = 894.04f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Paolumu", BaseSize = 1143.36f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Great Girros", BaseSize = 1053.15f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Radobaan", BaseSize = 1803.47f, CrownSize = MonsterSize.Alternate },
            new MonsterBaseInfo { Name = "Legiana", BaseSize = 1699.75f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Odogaron", BaseSize = 1388.75f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Rathalos", BaseSize = 1704.22f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Diablos", BaseSize = 2096.25f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Kirin", BaseSize = 536.26f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Zorah Magdaros", BaseSize = 25764.59f, CrownSize = MonsterSize.None },
            new MonsterBaseInfo { Name = "Dodogama", BaseSize = 1111.11f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Pink Rathian", BaseSize = 1754.37f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Bazelgeuse", BaseSize = 1928.38f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Lavasioth", BaseSize = 1797.24f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Uragaan", BaseSize = 2058.63f, CrownSize = MonsterSize.Alternate },
            new MonsterBaseInfo { Name = "Azure Rathalos", BaseSize = 1704.22f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Black Diablos", BaseSize = 2096.25f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Nergigante", BaseSize = 1848.12f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Teostra", BaseSize = 1790.15f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Kushala Daora", BaseSize = 1913.13f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Vaal Hazak", BaseSize = 2095.4f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Xeno'jiiva", BaseSize = 4509.1f, CrownSize = MonsterSize.None },
            new MonsterBaseInfo { Name = "Deviljho", BaseSize = 2063.82f, CrownSize = MonsterSize.Alternate },
            new MonsterBaseInfo { Name = "Kulve Taroth", BaseSize = 4573.25f, CrownSize = MonsterSize.None },
            new MonsterBaseInfo { Name = "Lunastra", BaseSize = 1828.69f, CrownSize = MonsterSize.Standard },
            new MonsterBaseInfo { Name = "Behemoth", BaseSize = 3423.65f, CrownSize = MonsterSize.None },
            new MonsterBaseInfo { Name = "Leshen", BaseSize = 549.7f, CrownSize = MonsterSize.None },
            new MonsterBaseInfo { Name = "Ancient Leshen", BaseSize = 549.7f, CrownSize = MonsterSize.None },
        };

        private ushort[] ReadMonsters16()
        {
            ushort[] result = new ushort[AvailableMonsterCount];

            for (int i = 0; i < AvailableMonsterCount; i++)
                result[i] = reader.ReadUInt16();

            Skip((MaxMonsterCount - AvailableMonsterCount) * 2);

            return result;
        }

        private byte[] ReadMonsters8()
        {
            byte[] result = new byte[AvailableMonsterCount];

            for (int i = 0; i < AvailableMonsterCount; i++)
                result[i] = reader.ReadByte();

            Skip((MaxMonsterCount - AvailableMonsterCount) * 2);

            return result;
        }
    }

    public struct MonsterStatsInfo
    {
        public string Name;
        public ushort Captured;
        public ushort Slayed;
        public ushort Largest;
        public ushort Smallest;
        public byte ResearchLevel;
        public bool HasCrowns;
        public bool HasMiniCrown;
        public bool HasSilverCrown;
        public bool HasGoldCrown;
    }

    public class MonsterStatsSaveSlotInfo : SaveSlotInfoBase
    {
        public MonsterStatsInfo[] MonsterStats { get; }

        public MonsterStatsSaveSlotInfo(
            SaveSlotInfoBase baseSaveSlotInfo,
            MonsterStatsInfo[] monsterStats
        )
            : base(baseSaveSlotInfo)
        {
            MonsterStats = monsterStats;
        }
    }
}
