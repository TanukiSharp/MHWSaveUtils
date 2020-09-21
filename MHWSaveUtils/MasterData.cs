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

        public static async Task Load()
        {
            using (var httpClient = new HttpClient())
            {
                await LoadJewels(httpClient);
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

        public static IReadOnlyCollection<JewelInfo> Jewels { get; private set; }
    }
}
