using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace MHWSaveUtils
{
    public class DecorationsReader : SaveDataReaderBase<DecorationsSaveSlotInfo>
    {
        public DecorationsReader(Stream saveData)
            : base(saveData)
        {
        }

        public override IEnumerable<DecorationsSaveSlotInfo> Read()
        {
            GotoSection3PastSignature();

            Skip(
                4 + // SAVEFILE.data.section.unknown
                8 // sectionSize
            );

            for (int i = 0; i < 3; i++)
            {
                DecorationsSaveSlotInfo saveSlotInfo = ReadSaveSlot(i + 1);
                if (saveSlotInfo != null)
                    yield return saveSlotInfo;
            }
        }

        private DecorationsSaveSlotInfo ReadSaveSlot(int slotNumber)
        {
            Skip(4); // unknown

            SaveSlotInfoBase baseSaveSlotInfo = ReadUntilPlaytimeIncluded(slotNumber);

            Skip(
                Constants.HunterAppearanceStructureSize + // H_APPEARANCE
                382 + // unknown
                Constants.PalicoAppearanceStructureSize // P_APPEARANCE
            );

            Skip(Constants.GuildCardStructureSize * 101); // hunter's one + 100 shared

            Skip(209447); // unkn1

            Skip(142200); // item loadouts

            // Skip item pouch
            Skip(
                24 * 8 + // items
                16 * 8 + // ammo
                256 + // unknown
                7 * 8 // special
            );

            Skip(
                200 * 8 + // items
                200 * 8 + // ammo
                1250 * 8 // materials
            );

            var decorations = new Dictionary<uint, uint>();

            for (int i = 0; i < 500; i++)
            {
                uint itemId = reader.ReadUInt32();
                uint itemQuantity = reader.ReadUInt32();

                if (itemId > 0)
                    decorations.Add(itemId, itemQuantity);
            }

            // Read 1000 equipment slots
            for (int i = 0; i < 2500; i++)
                ReadEquipmentSlot(decorations);

            // Skip until the end of the struct saveSlot
            Skip(0xa2618);

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            return new DecorationsSaveSlotInfo(baseSaveSlotInfo, decorations);
        }

        public void ReadEquipmentSlot(Dictionary<uint, uint> equippedJewels)
        {
            Skip(4); // SortIndex

            var equipmentType = (EquipmentType)reader.ReadUInt32();

            Skip(
                4 + // EquipmentType argument 1 (ArmourSlot, WeaponType, CharmPresence, KinsectPresence, None)
                4 + // IdInClass
                4 + // UpgradeLevel
                4   // UpgradePoints
            );

            if (equipmentType == EquipmentType.Armor || equipmentType == EquipmentType.Weapon)
            {
                for (int i = 0; i < 3; i++)
                {
                    uint deco = reader.ReadUInt32();

                    if (deco == uint.MaxValue)
                        continue;

                    uint id = MasterData.FindJewelInfoByEquippedItemId(deco).ItemId;

                    if (equippedJewels.TryGetValue(id, out uint quantity))
                        equippedJewels[id] = quantity + 1;
                    else
                        equippedJewels.Add(id, 1);
                }
            }
            else
            {
                Skip(
                    4 + // DecoSlot1
                    4 + // DecoSlot2
                    4   // DecoSlot3
                );
            }

            // Skip the remaining of the whole structure
            Skip(90);
        }
    }

    public class DecorationsSaveSlotInfo : SaveSlotInfoBase
    {
        public IReadOnlyDictionary<uint, uint> Decorations { get; }

        public DecorationsSaveSlotInfo(
            SaveSlotInfoBase baseSaveSlotInfo,
            IDictionary<uint, uint> decorations
        )
            : base(baseSaveSlotInfo)
        {
            Decorations = new ReadOnlyDictionary<uint, uint>(decorations);
        }
    }
}
