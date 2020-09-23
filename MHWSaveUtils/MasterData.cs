using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MHWSaveUtils
{
    public struct EquipmentInfo
    {
        public string Name { get; }
        public uint Id { get; }

        public EquipmentInfo(string name, uint id)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Argument '{nameof(name)}' must not be empty.");
            if (id == uint.MaxValue)
                throw new ArgumentException($"Argument '{nameof(id)}' cannot be 0x{uint.MaxValue:X}");

            Name = name;
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj is EquipmentInfo equipmentInfo)
                return equipmentInfo.Name == Name && equipmentInfo.Id == Id;
            return false;
        }

        public override int GetHashCode()
        {
            return $"{Name}|{Id}".GetHashCode();
        }

        public static bool operator ==(EquipmentInfo left, EquipmentInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EquipmentInfo left, EquipmentInfo right)
        {
            return !(left == right);
        }
    }

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
        private static async Task LoadJewels(HttpClient httpClient)
        {
            string jewelsJsonString = await httpClient.GetStringAsync("https://raw.githubusercontent.com/TanukiSharp/MHWMasterDataUtils/master/MHWMasterDataUtils.Exporter/data/jewels.json");

            var jewelsArray = (JArray)JsonConvert.DeserializeObject(jewelsJsonString);

            var jewels = new List<JewelInfo>();

            foreach (JObject jewelObject in jewelsArray)
            {
                string name = jewelObject["name"]["eng"].Value<string>();
                uint id = jewelObject["id"].Value<uint>();
                uint equipmentId = jewelObject["equipmentId"].Value<uint>();

                jewels.Add(new JewelInfo(name, id, equipmentId));
            }

            Jewels = new ReadOnlyCollection<JewelInfo>(jewels);
        }

        private static async Task<ReadOnlyDictionary<uint, EquipmentInfo>> LoadEquipmentsFile(HttpClient httpClient, string type, string idPropertyName)
        {
            string url = $"https://raw.githubusercontent.com/TanukiSharp/MHWMasterDataUtils/master/MHWMasterDataUtils.Exporter/data/{type}.json";

            string equipmentsJsonString = await httpClient.GetStringAsync(url);

            var equipmentsArray = (JArray)JsonConvert.DeserializeObject(equipmentsJsonString);

            var equipments = new Dictionary<uint, EquipmentInfo>();

            foreach (JObject equipmentObject in equipmentsArray)
            {
                string name = equipmentObject["name"]["eng"].Value<string>();
                uint id = equipmentObject[idPropertyName].Value<uint>();

                equipments.Add(id, new EquipmentInfo(name, id));
            }

            return new ReadOnlyDictionary<uint, EquipmentInfo>(equipments);
        }

        private static async Task LoadArmorPieces(HttpClient httpClient)
        {
            const string isPropertyName = "setGroup";

            ReadOnlyDictionary<uint, EquipmentInfo>[] result = await Task.WhenAll(
                LoadEquipmentsFile(httpClient, "heads", isPropertyName),
                LoadEquipmentsFile(httpClient, "chests", isPropertyName),
                LoadEquipmentsFile(httpClient, "arms", isPropertyName),
                LoadEquipmentsFile(httpClient, "waists", isPropertyName),
                LoadEquipmentsFile(httpClient, "legs", isPropertyName)
            );

            ArmorPieces = new ReadOnlyCollection<ReadOnlyDictionary<uint, EquipmentInfo>>(result);
        }

        private static async Task LoadWeapons(HttpClient httpClient)
        {
            const string idPropertyName = "sortOrder";

            ReadOnlyDictionary<uint, EquipmentInfo>[] result = await Task.WhenAll(
                LoadEquipmentsFile(httpClient, "great-swords", idPropertyName),
                LoadEquipmentsFile(httpClient, "sword-and-shields", idPropertyName),
                LoadEquipmentsFile(httpClient, "dual-blades", idPropertyName),
                LoadEquipmentsFile(httpClient, "long-swords", idPropertyName),
                LoadEquipmentsFile(httpClient, "hammers", idPropertyName),
                LoadEquipmentsFile(httpClient, "hunting-horns", idPropertyName),
                LoadEquipmentsFile(httpClient, "lances", idPropertyName),
                LoadEquipmentsFile(httpClient, "gunlances", idPropertyName),
                LoadEquipmentsFile(httpClient, "switch-axes", idPropertyName),
                LoadEquipmentsFile(httpClient, "charge-blades", idPropertyName),
                LoadEquipmentsFile(httpClient, "insect-glaives", idPropertyName),
                LoadEquipmentsFile(httpClient, "bows", idPropertyName),
                LoadEquipmentsFile(httpClient, "heavy-bowguns", idPropertyName),
                LoadEquipmentsFile(httpClient, "light-bowguns", idPropertyName)
            );

            Weapons = new ReadOnlyCollection<ReadOnlyDictionary<uint, EquipmentInfo>>(result);
        }

        private static async Task LoadCharms(HttpClient httpClient)
        {
            Charms = await LoadEquipmentsFile(httpClient, "charms", "setGroup");
        }

        public static async Task Load()
        {
            using (var httpClient = new HttpClient())
            {
                await Task.WhenAll(
                    LoadJewels(httpClient),
                    LoadArmorPieces(httpClient),
                    LoadCharms(httpClient),
                    LoadWeapons(httpClient)
                );
            }
        }

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

        public static ReadOnlyCollection<ReadOnlyDictionary<uint, EquipmentInfo>> ArmorPieces { get; private set; }
        public static ReadOnlyCollection<ReadOnlyDictionary<uint, EquipmentInfo>> Weapons { get; private set; }
        public static ReadOnlyDictionary<uint, EquipmentInfo> Charms { get; private set; }
        public static ReadOnlyCollection<JewelInfo> Jewels { get; private set; }
    }
}
