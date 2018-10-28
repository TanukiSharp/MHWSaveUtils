using System;
using System.Collections.Generic;
using System.Text;

namespace MHWSaveUtils
{
    public struct JewelInfo
    {
        public string Name { get; }
        public uint ItemId { get; }
        public uint EquippedItemId { get; }

        public JewelInfo(string name, uint itemId, uint equippedItemId)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Argument '{nameof(name)}' must not be empty.");
            if (itemId == uint.MaxValue)
                throw new ArgumentException($"Argument '{nameof(itemId)}' cannot be 0x{uint.MaxValue:X}");

            Name = name;
            ItemId = itemId;
            EquippedItemId = equippedItemId;
        }

        public override bool Equals(object obj)
        {
            if (obj is JewelInfo jewelInfo)
                return jewelInfo.Name == Name && jewelInfo.ItemId == ItemId && jewelInfo.EquippedItemId == EquippedItemId;
            return false;
        }

        public override int GetHashCode()
        {
            return $"{Name}|{ItemId}|{EquippedItemId}".GetHashCode();
        }

        public static bool operator ==(JewelInfo left, JewelInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JewelInfo left, JewelInfo right)
        {
            return !(left == right);
        }
    }

    public static class MasterData
    {
        public static JewelInfo FindJewelInfoByName(string name)
        {
            foreach (JewelInfo info in Jewels)
            {
                if (info.Name == name)
                    return info;
            }

            throw new ArgumentException($"Could not find {nameof(name)} {name} in {nameof(Jewels)} collection.");
        }

        public static JewelInfo FindJewelInfoByEquippedItemId(uint equippedItemId)
        {
            foreach (JewelInfo info in Jewels)
            {
                if (info.EquippedItemId == equippedItemId)
                    return info;
            }

            throw new ArgumentException($"Could not find {nameof(equippedItemId)} {equippedItemId} in {nameof(Jewels)} collection.");
        }

        public static JewelInfo FindJewelInfoByItemId(uint itemId)
        {
            foreach (JewelInfo info in Jewels)
            {
                if (info.ItemId == itemId)
                    return info;
            }

            throw new ArgumentException($"Could not find {nameof(itemId)} {itemId} in {nameof(Jewels)} collection.");
        }

        private static JewelInfo[] jewels;
        public static JewelInfo[] Jewels
        {
            get
            {
                if (jewels == null)
                {
                    jewels = new[]
                    {
                        new JewelInfo("Antidote Jewel 1", 727, 0),
                        new JewelInfo("Antipara Jewel 1", 728, 1),
                        new JewelInfo("Pep Jewel 1", 729, 2),
                        new JewelInfo("Steadfast Jewel 1", 730, 3),
                        new JewelInfo("Antiblast Jewel 1", 731, 4),
                        new JewelInfo("Suture Jewel 1", 732, 5),
                        new JewelInfo("Def Lock Jewel 1", 733, 6),
                        new JewelInfo("Earplug Jewel 3", 734, 7),
                        new JewelInfo("Wind Resist Jewel 2", 735, 8),
                        new JewelInfo("Footing Jewel 2", 736, 9),
                        new JewelInfo("Fertilizer Jewel 1", 737, 0xFFFFFFFF),
                        new JewelInfo("Heat Resist Jewel 2", 738, 0xFFFFFFFF),
                        new JewelInfo("Attack Jewel 1", 739, 10),
                        new JewelInfo("Defense Jewel 1", 740, 11),
                        new JewelInfo("Vitality Jewel 1", 741, 12),
                        new JewelInfo("Recovery Jewel 1", 742, 13),
                        new JewelInfo("Fire Res Jewel 1", 743, 14),
                        new JewelInfo("Water Res Jewel 1", 744, 15),
                        new JewelInfo("Ice Res Jewel 1", 745, 16),
                        new JewelInfo("Thunder Res Jewel 1", 746, 17),
                        new JewelInfo("Dragon Res Jewel 1", 747, 18),
                        new JewelInfo("Resistor Jewel 1", 748, 19),
                        new JewelInfo("Blaze Jewel 1", 749, 20),
                        new JewelInfo("Stream Jewel 1", 750, 21),
                        new JewelInfo("Frost Jewel 1", 751, 22),
                        new JewelInfo("Bolt Jewel 1", 752, 23),
                        new JewelInfo("Dragon Jewel 1", 753, 24),
                        new JewelInfo("Venom Jewel 1", 754, 25),
                        new JewelInfo("Paralyzer Jewel 1", 755, 26),
                        new JewelInfo("Sleep Jewel 1", 756, 27),
                        new JewelInfo("Blast Jewel 1", 757, 28),
                        new JewelInfo("Poisoncoat Jewel 3", 758, 29),
                        new JewelInfo("Paracoat Jewel 3", 759, 30),
                        new JewelInfo("Sleepcoat Jewel 3", 760, 31),
                        new JewelInfo("Blastcoat Jewel 3", 761, 32),
                        new JewelInfo("Powercoat Jewel 3", 762, 0xFFFFFFFF),
                        new JewelInfo("Release Jewel 3", 763, 33),
                        new JewelInfo("Expert Jewel 1", 764, 34),
                        new JewelInfo("Critical Jewel 2", 765, 35),
                        new JewelInfo("Tenderizer Jewel 2", 766, 36),
                        new JewelInfo("Charger Jewel 2", 767, 37),
                        new JewelInfo("Handicraft Jewel 3", 768, 38),
                        new JewelInfo("Draw Jewel 2", 769, 39),
                        new JewelInfo("Destroyer Jewel 2", 770, 40),
                        new JewelInfo("KO Jewel 2", 771, 41),
                        new JewelInfo("Drain Jewel 1", 772, 42),
                        new JewelInfo("Rodeo Jewel 2", 773, 0xFFFFFFFF),
                        new JewelInfo("Flight Jewel 2", 774, 43),
                        new JewelInfo("Throttle Jewel 2", 775, 44),
                        new JewelInfo("Challenger Jewel 2", 776, 45),
                        new JewelInfo("Flawless Jewel 2", 777, 46),
                        new JewelInfo("Potential Jewel 2", 778, 47),
                        new JewelInfo("Fortitude Jewel 1", 779, 48),
                        new JewelInfo("Furor Jewel 2", 780, 49),
                        new JewelInfo("Sonorous Jewel 1", 781, 50),
                        new JewelInfo("Magazine Jewel 2", 782, 51),
                        new JewelInfo("Trueshot Jewel 1", 783, 52),
                        new JewelInfo("Artillery Jewel 1", 784, 53),
                        new JewelInfo("Heavy Artillery Jewel 1", 785, 54),
                        new JewelInfo("Sprinter Jewel 2", 786, 55),
                        new JewelInfo("Physique Jewel 2", 787, 56),
                        new JewelInfo("Flying Leap Jewel 1", 788, 0xFFFFFFFF),
                        new JewelInfo("Refresh Jewel 2", 789, 57),
                        new JewelInfo("Hungerless Jewel 1", 790, 58),
                        new JewelInfo("Evasion Jewel 2", 791, 59),
                        new JewelInfo("Jumping Jewel 2", 792, 60),
                        new JewelInfo("Ironwall Jewel 1", 793, 61),
                        new JewelInfo("Sheath Jewel 1", 794, 62),
                        new JewelInfo("Friendship Jewel 1", 795, 63),
                        new JewelInfo("Enduring Jewel 1", 796, 64),
                        new JewelInfo("Satiated Jewel 1", 797, 65),
                        new JewelInfo("Gobbler Jewel 1", 798, 66),
                        new JewelInfo("Grinder Jewel 1", 799, 67),
                        new JewelInfo("Bomber Jewel 1", 800, 68),
                        new JewelInfo("Fungiform Jewel 1", 801, 69),
                        new JewelInfo("Angler Jewel 1", 802, 0xFFFFFFFF),
                        new JewelInfo("Chef Jewel 1", 803, 0xFFFFFFFF),
                        new JewelInfo("Transporter Jewel 1", 804, 0xFFFFFFFF),
                        new JewelInfo("Gathering Jewel 1", 805, 0xFFFFFFFF),
                        new JewelInfo("Honeybee Jewel 1", 806, 0xFFFFFFFF),
                        new JewelInfo("Carver Jewel 1", 807, 0xFFFFFFFF),
                        new JewelInfo("Protection Jewel 1", 808, 70),
                        new JewelInfo("Meowster Jewel 1", 809, 71),
                        new JewelInfo("Botany Jewel 1", 810, 72),
                        new JewelInfo("Geology Jewel 1", 811, 73),
                        new JewelInfo("Mighty Jewel 2", 812, 74),
                        new JewelInfo("Stonethrower Jewel 1", 813, 75),
                        new JewelInfo("Tip Toe Jewel 1", 814, 76),
                        new JewelInfo("Brace Jewel 3", 815, 77),
                        new JewelInfo("Scoutfly Jewel 1", 816, 0xFFFFFFFF),
                        new JewelInfo("Crouching Jewel 1", 817, 0xFFFFFFFF),
                        new JewelInfo("Longjump Jewel 1", 818, 0xFFFFFFFF),
                        new JewelInfo("Smoke Jewel 1", 819, 78),
                        new JewelInfo("Mirewalker Jewel 1", 820, 79),
                        new JewelInfo("Climber Jewel 1", 821, 0xFFFFFFFF),
                        new JewelInfo("Radiosity Jewel 1", 822, 0xFFFFFFFF),
                        new JewelInfo("Research Jewel 1", 823, 0xFFFFFFFF),
                        new JewelInfo("Specimen Jewel 1", 824, 80),
                        new JewelInfo("Miasma Jewel 1", 825, 97),
                        new JewelInfo("Scent Jewel 1", 826, 81),
                        new JewelInfo("Slider Jewel 1", 827, 0xFFFFFFFF),
                        new JewelInfo("Intimidator Jewel 1", 828, 82),
                        new JewelInfo("Hazmat Jewel 1", 829, 0xFFFFFFFF),
                        new JewelInfo("Mudshield Jewel 1", 830, 0xFFFFFFFF),
                        new JewelInfo("Element Resist Jewel 1", 831, 0xFFFFFFFF),
                        new JewelInfo("Slider Jewel 2", 832, 83),
                        new JewelInfo("Medicine Jewel 1", 833, 84),
                        new JewelInfo("Forceshot Jewel 3", 834, 85),
                        new JewelInfo("Pierce Jewel 3", 835, 86),
                        new JewelInfo("Spread Jewel 3", 836, 87),
                        new JewelInfo("Enhancer Jewel 2", 837, 88),
                        new JewelInfo("Crisis Jewel 1", 838, 89),
                        new JewelInfo("Dragonseal Jewel 3", 839, 90),
                        new JewelInfo("Discovery Jewel 2", 840, 0xFFFFFFFF),
                        new JewelInfo("Detector Jewel 1", 841, 0xFFFFFFFF),
                        new JewelInfo("Maintenance Jewel 1", 842, 91),
                        new JewelInfo("Mighty Bow Jewel 2", 874, 92),
                        new JewelInfo("Mind's Eye Jewel 2", 875, 93),
                        new JewelInfo("Shield Jewel 2", 876, 94),
                        new JewelInfo("Sharp Jewel 2", 877, 95),
                        new JewelInfo("Elementless Jewel 2", 878, 96),
                    };
                }

                return jewels;
            }
        }
    }
}
