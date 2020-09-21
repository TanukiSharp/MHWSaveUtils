using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MHWCrypto;

namespace MHWSaveUtils.Tester
{
    class Program
    {
        static void Main()
        {
            new Program().Run().Wait();
        }

        private async Task Run()
        {
            await MasterData.Load();

            foreach (SaveDataInfo saveDataInfo in FileSystemUtils.EnumerateSaveDataInfo())
            {
                Console.WriteLine($"UserId: {saveDataInfo.UserId}");
                Console.WriteLine($"SaveDataFullFilename: {saveDataInfo.SaveDataFullFilename}");
                Console.WriteLine();

                while (true)
                {
                    await ReadAccount(saveDataInfo);
                    Console.WriteLine();
                    Console.WriteLine("Press any key to reload...");

                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        break;

                    Console.Clear();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
        }

        private async Task ReadAccount(SaveDataInfo saveDataInfo)
        {
            MemoryStream ms;

            var stopwatch = Stopwatch.StartNew();
            var crypto = new Crypto();

            using (Stream inputStream = File.OpenRead(saveDataInfo.SaveDataFullFilename))
            {
                byte[] buffer = new byte[inputStream.Length];
                await inputStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
                await crypto.DecryptAsync(buffer);
                ms = new MemoryStream(buffer);
            }

            stopwatch.Stop();

            Console.WriteLine($"Load and decrypt took {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();

            //string targetFilename = $"{saveDataFullFilename}.decrypted.bin";
            //File.WriteAllBytes(targetFilename, ms.ToArray());

            PrintSeparator('=');
            ReadMonsterStats(ms);
            //PrintSeparator('=');
            //ReadEquipment(ms);
            PrintSeparator('=');
            ReadDecorations(ms);
            PrintSeparator('=');
            ReadWeaponUsage(ms);
        }

        private void PrintSeparator(char sep)
        {
            Console.WriteLine(new string(sep, Console.WindowWidth - 1));
        }

        private void PrintBaseSaveData(SaveSlotInfoBase baseSaveSlotInfo)
        {
            Console.WriteLine($"Hunter name: {baseSaveSlotInfo.Name}");
            Console.WriteLine($"HR: {baseSaveSlotInfo.HR}");
            Console.WriteLine($"MR: {baseSaveSlotInfo.MR}");
            Console.WriteLine($"Playtime: {MiscUtils.PlaytimeToGameString(baseSaveSlotInfo.Playtime)}");
        }

        private void ReadMonsterStats(Stream saveData)
        {
            var equipmentReader = new MonsterStatsReader(saveData);

            foreach (MonsterStatsSaveSlotInfo monsterStatsInfo in equipmentReader.Read())
            {
                PrintBaseSaveData(monsterStatsInfo);
                Console.WriteLine();
                int index = 0;
                foreach (MonsterStatsInfo monsterStats in monsterStatsInfo.MonsterStats)
                {
                    Console.Write($"{index:d2} - {monsterStats.Name,-22}{monsterStats.Slayed + monsterStats.Captured,-5}{monsterStats.Captured,-10}");

                    if (monsterStats.HasCrowns)
                    {
                        if (monsterStats.HasMiniCrown)
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                        else
                            Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.Write($"{MiniCrown(monsterStats),-3}");

                    if (monsterStats.HasCrowns)
                    {
                        if (monsterStats.HasGoldCrown)
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                        else if (monsterStats.HasSilverCrown)
                            Console.ForegroundColor = ConsoleColor.White;
                        else
                            Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.Write($"{LargeCrown(monsterStats),-10}");

                    Console.ResetColor();

                    Console.WriteLine($"{monsterStats.ResearchLevel}");

                    index++;
                }

                PrintSeparator('-');
            }

            string MiniCrown(MonsterStatsInfo monsterStats)
            {
                if (monsterStats.HasCrowns == false)
                    return " ";

                if (monsterStats.HasMiniCrown)
                    return "G";

                return "X";
            }

            string LargeCrown(MonsterStatsInfo monsterStats)
            {
                if (monsterStats.HasCrowns == false)
                    return " ";

                if (monsterStats.HasGoldCrown)
                    return "G";
                else if (monsterStats.HasSilverCrown)
                    return "S";

                return "X";
            }
        }

        private void ReadEquipment(Stream saveData)
        {
            var equipmentReader = new EquipmentReader(saveData);

            foreach (EquipmentSaveSlotInfo equipmentInfo in equipmentReader.Read())
            {
                PrintBaseSaveData(equipmentInfo);
                Console.WriteLine();
                Console.WriteLine($"{equipmentInfo.Equipment.Length} equipment");
                foreach (Equipment equipment in equipmentInfo.Equipment.OrderBy(x => x.SortIndex))
                    Console.WriteLine(equipment);
                PrintSeparator('-');
            }
        }

        private void ReadDecorations(Stream saveData)
        {
            var decorationsReader = new DecorationsReader(saveData);

            foreach (DecorationsSaveSlotInfo decorationInfo in decorationsReader.Read())
            {
                PrintBaseSaveData(decorationInfo);
                Console.WriteLine();
                Console.WriteLine($"{decorationInfo.Decorations.Count} decorations");
                foreach (KeyValuePair<uint, uint> decoKeyValue in decorationInfo.Decorations.OrderBy(kv => MasterData.FindJewelInfoByItemId(kv.Key).Name))
                    Console.WriteLine($"   {MasterData.FindJewelInfoByItemId(decoKeyValue.Key).Name}: x{decoKeyValue.Value}");
                PrintSeparator('-');
            }
        }

        private void ReadWeaponUsage(Stream saveData)
        {
            using (var weaponUsageReader = new WeaponUsageReader(saveData))
            {
                foreach (WeaponUsageSaveSlotInfo weaponUsageInfo in weaponUsageReader.Read())
                {
                    PrintBaseSaveData(weaponUsageInfo);
                    Console.WriteLine();
                    Console.WriteLine("Low rank:");
                    PrintWeaponUsage(weaponUsageInfo.LowRank);
                    Console.WriteLine("High rank:");
                    PrintWeaponUsage(weaponUsageInfo.HighRank);
                    Console.WriteLine("Master rank:");
                    PrintWeaponUsage(weaponUsageInfo.MasterRank);
                    Console.WriteLine("Investigations:");
                    PrintWeaponUsage(weaponUsageInfo.Investigations);
                    Console.WriteLine("Guiding Lands:");
                    PrintWeaponUsage(weaponUsageInfo.GuidingLands);
                    Console.WriteLine("Total:");
                    PrintWeaponUsage(weaponUsageInfo.LowRank + weaponUsageInfo.HighRank + weaponUsageInfo.MasterRank + weaponUsageInfo.Investigations + weaponUsageInfo.GuidingLands);
                    PrintSeparator('-');
                }
            }
        }

        private void PrintWeaponUsage(WeaponUsage weaponUsage)
        {
            var sb = new StringBuilder("    ");

            sb.Append($"GS: {weaponUsage.GreatSword}, ");
            sb.Append($"LS: {weaponUsage.LongSword}, ");
            sb.Append($"SnS: {weaponUsage.SwordAndShield}, ");
            sb.Append($"DB: {weaponUsage.DualBlades}, ");
            sb.Append($"Hammer: {weaponUsage.Hammer}, ");
            sb.Append($"HH: {weaponUsage.HuntingHorn}, ");
            sb.Append($"Lance: {weaponUsage.Lance}, ");
            sb.Append($"GL: {weaponUsage.Gunlance}, ");
            sb.Append($"SA: {weaponUsage.SwitchAxe}, ");
            sb.Append($"CB: {weaponUsage.ChargeBlade}, ");
            sb.Append($"IG: {weaponUsage.InsectGlaive}, ");
            sb.Append($"LBG: {weaponUsage.LightBowgun}, ");
            sb.Append($"HBG: {weaponUsage.HeavyBowgun}, ");
            sb.Append($"Bow: {weaponUsage.Bow}");

            Console.WriteLine(sb.ToString());
        }
    }
}
