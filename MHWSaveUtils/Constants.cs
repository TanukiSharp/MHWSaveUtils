using System;
using System.Collections.Generic;
using System.Text;

namespace MHWSaveUtils
{
    public static class Constants
    {
        public const uint Section3Signature = 0xAD35B985;

        public const long HunterAppearanceStructureSize = 120; // H_APPEARANCE
        public const long PalicoAppearanceStructureSize = 44; // P_APPEARANCE
        public const long WeaponUsageStructureSize = 14 * 2; // WEAPONUSAGE
        public const long HunterEquipmentStructureSize = 18 * 4; // struct hunterEquipment
        public const long PalicoEquipmentStructureSize = 8 * 4; // struct  palicoEquipment
        public const long PalicoStructureSize = 119 + PalicoEquipmentStructureSize; // struct  palico
        public const long ArenaRecordStructureSize = 60; // struct arenaRecord
        public const long ArenaStatsStructSize = 2 + ArenaRecordStructureSize * 5; // ARENASTATS
        public const long Creatures8StructSize = 64; // CREATURES8
        public const long Creatures16StructSize = 64 * 2; // CREATURES16
        public const long GuildCardStructureSize = // GUILDCARD
            1020 +
            HunterEquipmentStructureSize +
            PalicoStructureSize +
            3 * WeaponUsageStructureSize +
            10 * ArenaStatsStructSize +
            4 * Creatures16StructSize +
            Creatures8StructSize;

        public const long ItemLoadoutStructureSize = 1128; // struct loadout
        public const long ItemLoadoutsStructureSize = ItemLoadoutStructureSize * 56 + 56; // struct itemLoadouts
        public const long ItemPouchStructureSize = 24 * 8 + 16 * 8 + 256 + 4 * 8; // struct itemPouch
        public const long ItemBoxStructureSize = 8 * 200 + 8 * 200 + 8 * 800 + 8 * 200; // struct itemBox
        public const long EquipLoadoutStructureSize = 544; // struct equipLoadout
        public const long EquipLoadoutsStructureSize = EquipLoadoutStructureSize * 112;
        public const long DlcTypeSize = 2; // DLCTYPE
    }
}
