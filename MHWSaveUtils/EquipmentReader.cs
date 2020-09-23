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
                1250 * 8 + // materials
                500 * 8 // decorations
            );

            var equipments = new List<Equipment>();

            // Read 2500 equipment slots
            for (int i = 0; i < 2500; i++)
            {
                var eqp = Equipment.Read(reader);
                if (eqp != null)
                    equipments.Add(eqp);
            }

            // Skip until the end of the struct saveSlot
            Skip(0xa2618);

            if (baseSaveSlotInfo.Playtime == 0)
                return null;

            return new EquipmentSaveSlotInfo(equipments.ToArray(), baseSaveSlotInfo);
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
                reader.BaseStream.Seek(118, SeekOrigin.Current);
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

            reader.BaseStream.Seek(8, SeekOrigin.Current);

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

            // skip many unknowns
            reader.BaseStream.Seek(66, SeekOrigin.Current);

            return result;
        }

        public override string ToString()
        {
            if (Type == EquipmentType.Armor)
                return $"[{ArmorPieceType}] {MasterData.ArmorPieces[(int)ArmorPieceType][ClassId].Name}";

            if (Type == EquipmentType.Weapon)
                return $"[{WeaponType}] {MasterData.Weapons[(int)WeaponType][ClassId].Name}";

            if (Type == EquipmentType.Charm)
                return $"[Charm] {MasterData.Charms[ClassId].Name}";

            if (Type == EquipmentType.Kinsect)
                return ($"[Kinsect] [{SortIndex}] {Type} {ClassId} (Id: {KinsectId})");

            return $"??? (SortIndex: {SortIndex}, Type: {Type}, ArmorPieceType: {ArmorPieceType}, WeaponType: {WeaponType}, CharmId: {CharmId}, KinsectId: {KinsectId}, ClassId: {ClassId}, KinsectType: {KinsectType})";
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
