using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MHWSaveUtils
{
    public class EquipmentReader : SaveDataReaderBase<EquipmentSaveSlotInfo>
    {
        public EquipmentReader(Stream saveData)
            : base(saveData)
        {
        }

        public override IEnumerable<EquipmentSaveSlotInfo> Read()
        {
            GotoSection3PastSignature();

            Skip(
                4 + // SAVEFILE.data.section.unknown
                8 // sectionSize
            );

            for (int i = 0; i < 3; i++)
            {
                EquipmentSaveSlotInfo saveSlotInfo = ReadSaveSlot(i + 1);
                if (saveSlotInfo != null)
                    yield return saveSlotInfo;
            }
        }

        private EquipmentSaveSlotInfo ReadSaveSlot(int slotNumber)
        {
            Skip(4); // unknown

            SaveSlotInfoBase baseSaveSlotInfo = ReadUntilPlaytimeIncluded(slotNumber);

            // Skip until beginning of struct equipmentSlot
            Skip(
                4 + // unknown
                Constants.HunterAppearanceStructureSize + // H_APPEARANCE
                Constants.PalicoAppearanceStructureSize + // P_APPEARANCE
                Constants.GuildCardStructureSize + // hunterGC
                Constants.GuildCardStructureSize * 100 + // sharedGC
                0x019e36 + // unknown
                Constants.ItemLoadoutsStructureSize + // itemLoadouts
                8 + // unknown
                Constants.ItemPouchStructureSize + // itemPouch
                8 * 200 + // struct items
                8 * 200 + // struct ammo
                8 * 800 + // struct matrials
                8 * 200   // struct decorations
            );

            var equipment = new List<Equipment>();

            // Read 1000 equipment slots
            for (int i = 0; i < 1000; i++)
            {
                var eqp = Equipment.Read(reader);
                if (eqp != null)
                    equipment.Add(eqp);
            }

            // Skip until the end of the struct saveSlot
            Skip(
                0x2449C + // unknown
                0x2a * 250 + // investigations
                0x0FB9 + // unknown
                Constants.EquipLoadoutsStructureSize + // equipLoadout
                0x6521 + // unknown
                Constants.DlcTypeSize * 256 + // DLCClaimed
                0x2A5D // unknown
            );

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            return new EquipmentSaveSlotInfo(equipment.ToArray(), baseSaveSlotInfo);
        }
    }

    public class Equipment
    {
        public uint SortIndex { get; private set; }
        public EquipmentType Type { get; private set; }
        public ArmorPieceType ArmorPieceType { get; private set; }
        public WeaponType WeaponType { get; private set; }
        public uint CharmId { get; private set; }
        public uint KinsectId { get; private set; }
        public uint ClassId { get; private set; }
        public uint UpgradeLevel { get; private set; }
        public uint UpgradePoints { get; private set; }
        public Decoration[] DecorationSlots { get; private set; }
        public BowGunMod[] BowgunMods { get; private set; }
        public KinsectType KinsectType { get; private set; }
        public AugmentationType[] Augmentations { get; private set; }

        private Equipment()
        {
        }

        public static Equipment Read(BinaryReader reader)
        {
            var result = new Equipment
            {
                SortIndex = reader.ReadUInt32(),
                Type = (EquipmentType)reader.ReadUInt32()
            };

            if (result.Type == EquipmentType.None)
            {
                reader.BaseStream.Seek(15 * 4, SeekOrigin.Current);
                return null;
            }

            uint value;

            value = reader.ReadUInt32();

            switch (result.Type)
            {
                case EquipmentType.Armor:
                    result.ArmorPieceType = (ArmorPieceType)value;
                    break;
                case EquipmentType.Weapon:
                    result.WeaponType = (WeaponType)value;
                    break;
                case EquipmentType.Charm:
                    result.CharmId = value;
                    break;
                case EquipmentType.Kinsect:
                    result.KinsectId = value;
                    break;
            }

            result.ClassId = reader.ReadUInt32();
            result.UpgradeLevel = reader.ReadUInt32();
            result.UpgradePoints = reader.ReadUInt32();

            result.DecorationSlots = new[]
            {
                (Decoration)reader.ReadUInt32(),
                (Decoration)reader.ReadUInt32(),
                (Decoration)reader.ReadUInt32()
            };

            if (result.Type == EquipmentType.Kinsect)
            {
                result.KinsectType = (KinsectType)reader.ReadUInt32();
                reader.BaseStream.Seek(2 * 4, SeekOrigin.Current); // skip BowgunMod2 and BowgunMod3
            }
            else if (result.Type == EquipmentType.Weapon && (result.WeaponType == WeaponType.LightBowgun || result.WeaponType == WeaponType.HeavyBowgun))
            {
                result.BowgunMods = new[]
                {
                    (BowGunMod)reader.ReadUInt32(),
                    (BowGunMod)reader.ReadUInt32(),
                    (BowGunMod)reader.ReadUInt32(),
                };
            }
            else
                reader.BaseStream.Seek(3 * 4, SeekOrigin.Current);

            result.Augmentations = new[]
            {
                (AugmentationType)reader.ReadUInt32(),
                (AugmentationType)reader.ReadUInt32(),
                (AugmentationType)reader.ReadUInt32(),
            };

            // skip 2 Unknown
            reader.BaseStream.Seek(2 * 4, SeekOrigin.Current);

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"[{SortIndex}] {Type} {ClassId}");

            if (Type == EquipmentType.Armor)
                sb.Append($" ({ArmorPieceType})");
            else if (Type == EquipmentType.Weapon)
                sb.Append($" ({WeaponType})");
            else if (Type == EquipmentType.Charm)
                sb.Append($" (Id: {CharmId})");
            else if (Type == EquipmentType.Kinsect)
                sb.Append($" (Id: {KinsectId}");

            return sb.ToString();
        }
    }

    public class EquipmentSaveSlotInfo : SaveSlotInfoBase
    {
        public Equipment[] Equipment { get; }

        public EquipmentSaveSlotInfo(Equipment[] equipment, SaveSlotInfoBase baseSaveSlotInfo)
            : base(baseSaveSlotInfo)
        {
            Equipment = equipment;
        }
    }
}
