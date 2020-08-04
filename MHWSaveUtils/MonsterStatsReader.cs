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
                8 // sectionSize
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

            Skip(Constants.WeaponUsageStructureSize * 5);

            // Skip the remaining of the GUILDCARD structure
            Skip(
                1 + // poseID
                1 + // expressionID
                1 + // backgroundID
                1 + // stickerID
                256 + // greeting
                256 // title
            );

            Skip(
                5454 // lot of things
            );

            ushort[] captured = ReadMonsters16();
            ushort[] slayed = ReadMonsters16();
            ushort[] largest = ReadMonsters16();
            ushort[] smallest = ReadMonsters16();
            byte[] researchLevel = ReadMonsters8();

            // Skip the remaining of the saveSlot structure
            Skip(
                2127779 + // lot of things
                512 // hash things
            );

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            var monsters = new MonsterStatsInfo[MonsterData.Count];

            int i = 0;
            foreach (MonsterBaseInfo monsterData in MonsterData.Values)
            {
                bool hasMini = HasMini(smallest[i], monsterData);
                bool hasGold = HasGold(largest[i], monsterData);
                bool hasSilver = hasGold || HasSilver(largest[i], monsterData);

                monsters[i] = new MonsterStatsInfo
                {
                    Name = monsterData.Name,
                    Captured = captured[i],
                    Slayed = slayed[i],
                    Largest = largest[i],
                    Smallest = smallest[i],
                    ResearchLevel = researchLevel[i],
                    HasCrowns = monsterData.CrownSize != MonsterSize.None,
                    HasMiniCrown = hasMini,
                    HasSilverCrown = hasSilver,
                    HasGoldCrown = hasGold
                };

                i++;
            }

            return new MonsterStatsSaveSlotInfo(baseSaveSlotInfo, monsters);
        }

        private const int MaxMonsterCount = 96;

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
            if (smallest == 0)
                return false;

            if (monsterBaseInfo.CrownSize == MonsterSize.None)
                return false;

            return smallest <= Mini;
        }

        private bool HasSilver(ushort largest, MonsterBaseInfo monsterBaseInfo)
        {
            if (largest == 0)
                return false;

            if (monsterBaseInfo.CrownSize == MonsterSize.Standard)
                return largest >= SilverStandard;

            if (monsterBaseInfo.CrownSize == MonsterSize.Alternate)
                return largest >= SilverAlternate;

            return false;
        }

        private bool HasGold(ushort largest, MonsterBaseInfo monsterBaseInfo)
        {
            if (largest == 0)
                return false;

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

        private static readonly Dictionary<int, MonsterBaseInfo> MonsterData = new Dictionary<int, MonsterBaseInfo>
        {
            [0] = new MonsterBaseInfo { Name = "Great Jagras", BaseSize = 1109.66f, CrownSize = MonsterSize.Standard },
            [1] = new MonsterBaseInfo { Name = "Kulu-Ya-Ku", BaseSize = 901.24f, CrownSize = MonsterSize.Standard },
            [2] = new MonsterBaseInfo { Name = "Pukei-Pukei", BaseSize = 1102.45f, CrownSize = MonsterSize.Alternate },
            [3] = new MonsterBaseInfo { Name = "Barroth", BaseSize = 1383.07f, CrownSize = MonsterSize.Standard },
            [4] = new MonsterBaseInfo { Name = "Jyuratodus", BaseSize = 1508.71f, CrownSize = MonsterSize.Standard },
            [5] = new MonsterBaseInfo { Name = "Tobi-Kadachi", BaseSize = 1300.52f, CrownSize = MonsterSize.Alternate },
            [6] = new MonsterBaseInfo { Name = "Anjanath", BaseSize = 1646.46f, CrownSize = MonsterSize.Alternate },
            [7] = new MonsterBaseInfo { Name = "Rathian", BaseSize = 1754.37f, CrownSize = MonsterSize.Standard },
            [8] = new MonsterBaseInfo { Name = "Tzitzi-Ya-Ku", BaseSize = 894.04f, CrownSize = MonsterSize.Standard },
            [9] = new MonsterBaseInfo { Name = "Paolumu", BaseSize = 1143.36f, CrownSize = MonsterSize.Standard },
            [10] = new MonsterBaseInfo { Name = "Great Girros", BaseSize = 1053.15f, CrownSize = MonsterSize.Standard },
            [11] = new MonsterBaseInfo { Name = "Radobaan", BaseSize = 1803.47f, CrownSize = MonsterSize.Alternate },
            [12] = new MonsterBaseInfo { Name = "Legiana", BaseSize = 1699.75f, CrownSize = MonsterSize.Standard },
            [13] = new MonsterBaseInfo { Name = "Odogaron", BaseSize = 1388.75f, CrownSize = MonsterSize.Standard },
            [14] = new MonsterBaseInfo { Name = "Rathalos", BaseSize = 1704.22f, CrownSize = MonsterSize.Standard },
            [15] = new MonsterBaseInfo { Name = "Diablos", BaseSize = 2096.25f, CrownSize = MonsterSize.Standard },
            [16] = new MonsterBaseInfo { Name = "Kirin", BaseSize = 536.26f, CrownSize = MonsterSize.Standard },
            [17] = new MonsterBaseInfo { Name = "Zorah Magdaros", BaseSize = 25764.59f, CrownSize = MonsterSize.None },
            [18] = new MonsterBaseInfo { Name = "Dodogama", BaseSize = 1111.11f, CrownSize = MonsterSize.Standard },
            [19] = new MonsterBaseInfo { Name = "Pink Rathian", BaseSize = 1754.37f, CrownSize = MonsterSize.Standard },
            [20] = new MonsterBaseInfo { Name = "Bazelgeuse", BaseSize = 1928.38f, CrownSize = MonsterSize.Standard },
            [21] = new MonsterBaseInfo { Name = "Lavasioth", BaseSize = 1797.24f, CrownSize = MonsterSize.Standard },
            [22] = new MonsterBaseInfo { Name = "Uragaan", BaseSize = 2058.63f, CrownSize = MonsterSize.Alternate },
            [23] = new MonsterBaseInfo { Name = "Azure Rathalos", BaseSize = 1704.22f, CrownSize = MonsterSize.Standard },
            [24] = new MonsterBaseInfo { Name = "Black Diablos", BaseSize = 2096.25f, CrownSize = MonsterSize.Standard },
            [25] = new MonsterBaseInfo { Name = "Nergigante", BaseSize = 1848.12f, CrownSize = MonsterSize.Standard },
            [26] = new MonsterBaseInfo { Name = "Teostra", BaseSize = 1790.15f, CrownSize = MonsterSize.Standard },
            [27] = new MonsterBaseInfo { Name = "Kushala Daora", BaseSize = 1913.13f, CrownSize = MonsterSize.Standard },
            [28] = new MonsterBaseInfo { Name = "Vaal Hazak", BaseSize = 2095.4f, CrownSize = MonsterSize.Standard },
            [29] = new MonsterBaseInfo { Name = "Xeno'jiiva", BaseSize = 4509.1f, CrownSize = MonsterSize.None },
            [30] = new MonsterBaseInfo { Name = "Deviljho", BaseSize = 2063.82f, CrownSize = MonsterSize.Alternate },
            [31] = new MonsterBaseInfo { Name = "Kulve Taroth", BaseSize = 4573.25f, CrownSize = MonsterSize.None },
            [32] = new MonsterBaseInfo { Name = "Lunastra", BaseSize = 1828.69f, CrownSize = MonsterSize.Standard },
            [33] = new MonsterBaseInfo { Name = "Behemoth", BaseSize = 3423.65f, CrownSize = MonsterSize.None },
            [34] = new MonsterBaseInfo { Name = "Leshen", BaseSize = 549.7f, CrownSize = MonsterSize.None },
            [35] = new MonsterBaseInfo { Name = "Ancient Leshen", BaseSize = 549.7f, CrownSize = MonsterSize.None },
            [36] = new MonsterBaseInfo { Name = "Beotodus", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [37] = new MonsterBaseInfo { Name = "Banbaro", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [38] = new MonsterBaseInfo { Name = "Viper Tobi-Kadachi", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [39] = new MonsterBaseInfo { Name = "Nightshade Paolumu", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [40] = new MonsterBaseInfo { Name = "Coral Pukei-Pukei", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [41] = new MonsterBaseInfo { Name = "Barioth", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [42] = new MonsterBaseInfo { Name = "Nargacuga", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [43] = new MonsterBaseInfo { Name = "Glavenus", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [44] = new MonsterBaseInfo { Name = "Tigrex", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [45] = new MonsterBaseInfo { Name = "Brachydios", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [46] = new MonsterBaseInfo { Name = "Acidic Glavenus", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [47] = new MonsterBaseInfo { Name = "Shrieking Legiana", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [48] = new MonsterBaseInfo { Name = "Fulgur Anjanath", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [49] = new MonsterBaseInfo { Name = "Ebony Odogaron", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [50] = new MonsterBaseInfo { Name = "Velkhana", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [51] = new MonsterBaseInfo { Name = "Seething Bazelgeuse", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [52] = new MonsterBaseInfo { Name = "Blackveil Vaal Hazak", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [53] = new MonsterBaseInfo { Name = "Namielle", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [54] = new MonsterBaseInfo { Name = "Savage Deviljho", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [55] = new MonsterBaseInfo { Name = "Ruiner Nergigante", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [56] = new MonsterBaseInfo { Name = "Shara Ishvalda", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [57] = new MonsterBaseInfo { Name = "Zinogre", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [58] = new MonsterBaseInfo { Name = "Yian Garuga", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [60] = new MonsterBaseInfo { Name = "Brute Tigrex", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [61] = new MonsterBaseInfo { Name = "Gold Rathian", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [62] = new MonsterBaseInfo { Name = "Silver Rathalos", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [63] = new MonsterBaseInfo { Name = "Rajang", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [64] = new MonsterBaseInfo { Name = "Stygian Zinogre", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [65] = new MonsterBaseInfo { Name = "Safi'jiiva", BaseSize = 0.0f, CrownSize = MonsterSize.None },
            [69] = new MonsterBaseInfo { Name = "Alatreon", BaseSize = 0.0f, CrownSize = MonsterSize.None },
        };

        private ushort[] ReadMonsters16()
        {
            ushort[] result = new ushort[MaxMonsterCount];

            for (int i = 0; i < MaxMonsterCount; i++)
                result[i] = reader.ReadUInt16();

            return result;
        }

        private byte[] ReadMonsters8()
        {
            byte[] result = new byte[MaxMonsterCount];

            for (int i = 0; i < MaxMonsterCount; i++)
                result[i] = reader.ReadByte();

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
