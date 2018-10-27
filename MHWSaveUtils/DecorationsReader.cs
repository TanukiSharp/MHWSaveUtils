using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace MHWSaveUtils
{
    public class DecorationsReader : SaveDataReaderBase
    {
        public DecorationsReader(Stream saveData)
            : base(saveData)
        {
        }

        public IEnumerable<DecorationsSaveSlotInfo> Read()
        {
            GotoSection3PastSignature();

            Skip(
                4 + // SAVEFILE.data.section.unknown
                8 + // SAVEFILE.data.section.sectionSize
                4   // SAVEFILE.data.section.sectionData[2].unknown (sectionData_3.unknown)
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
            BaseSaveSlotInfo baseSaveSlotInfo = ReaderUntilPlaytimeIncluded(slotNumber);

            // Skip until beginning of struct itemBox
            Skip(
                4 + // unknown
                Constants.HunterAppearanceStructureSize + // H_APPEARANCE
                Constants.PalicoAppearanceStructureSize + // P_APPEARANCE
                Constants.GuildCardStructureSize + // hunterGC
                Constants.GuildCardStructureSize * 100 + // sharedGC
                0x019e36 + // unknown
                Constants.ItemLoadoutsStructureSize + // itemLoadouts
                8 + // unknown
                Constants.ItemPouchStructureSize // itemPouch
            );

            Skip(
                8 * 200 + // struct items
                8 * 200 + // struct ammo
                8 * 800   // struct matrials
            );

            var decorations = new Dictionary<uint, uint>();
            for (int i = 0; i < 200; i++)
            {
                uint itemId = reader.ReadUInt32();
                uint itemQuantity = reader.ReadUInt32();

                if (itemId > 0)
                    decorations.Add(itemId, itemQuantity);
            }

            // Skip until the end of the struct saveSlot
            Skip(
                0x034E3C + // unknown
                0x2a * 250 + // investigations
                0x0FB9 + // unknown
                Constants.EquipLoadoutsStructureSize + // equipLoadout
                0x6521 + // unknown
                Constants.DlcTypeSize * 256 + // DLCClaimed
                0x2A5D // unknown
            );

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            return new DecorationsSaveSlotInfo(baseSaveSlotInfo, decorations);
        }
    }

    public class DecorationsSaveSlotInfo : BaseSaveSlotInfo
    {
        public IReadOnlyDictionary<uint, uint> Decorations { get; }

        public DecorationsSaveSlotInfo(
            BaseSaveSlotInfo baseSaveSlotInfo,
            IDictionary<uint, uint> decorations
        )
            : base(baseSaveSlotInfo)
        {
            Decorations = new ReadOnlyDictionary<uint, uint>(decorations);
        }
    }
}
