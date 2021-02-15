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

        private void IncrementDecorationByEquipmentId(uint equipmentId, Dictionary<uint, uint> decorations)
        {
            JewelInfo jewel = MasterData.FindJewelInfoByEquippedItemId(equipmentId);

            if (decorations.TryGetValue(jewel.ItemId, out uint quantity))
                decorations[jewel.ItemId] = quantity + 1;
            else
                decorations.Add(jewel.ItemId, 1);
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

            // Skip until specialized tools
            Skip(0x4AD29);

            for (int i = 0; i < 128; i++) // 128
            {
                Skip(4); // id
                uint isAvailable = reader.ReadUInt32();

                if (isAvailable == 0)
                {
                    Skip(118);
                    continue;
                }

                Skip(4); // Unused

                uint slot1EquipId = reader.ReadUInt32();
                uint slot2EquipId = reader.ReadUInt32();
                uint slot3EquipId = reader.ReadUInt32();

                if (slot1EquipId != uint.MaxValue)
                    IncrementDecorationByEquipmentId(slot1EquipId, decorations);

                if (slot2EquipId != uint.MaxValue)
                    IncrementDecorationByEquipmentId(slot2EquipId, decorations);

                if (slot3EquipId != uint.MaxValue)
                    IncrementDecorationByEquipmentId(slot3EquipId, decorations);

                Skip(102); // Unknown
            }

            // Skip until the end of the struct saveSlot
            Skip(0x539EF);

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
