using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHWSaveUtils.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().Wait();
        }

        private async Task Run()
        {
            foreach (SaveDataInfo saveDataInfo in FileSystemUtils.EnumerateSaveDataInfo())
            {
                Console.WriteLine($"UserId: {saveDataInfo.UserId}");
                Console.WriteLine($"SaveDataFullFilename: {saveDataInfo.SaveDataFullFilename}");
                Console.WriteLine();

                await ReadAccount(saveDataInfo);
            }
        }

        private async Task ReadAccount(SaveDataInfo saveDataInfo)
        {
            var ms = new MemoryStream();

            var stopwatch = Stopwatch.StartNew();

            using (Stream inputStream = File.OpenRead(saveDataInfo.SaveDataFullFilename))
            {
                await Crypto.DecryptAsync(inputStream, ms, CancellationToken.None);
            }

            stopwatch.Stop();

            Console.WriteLine($"Load and decrypt took {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine();

            //string targetFilename = $"{saveDataFullFilename}.decrypted.bin";
            //File.WriteAllBytes(targetFilename, ms.ToArray());

            PrintSeparator('=');

            ReadDecorations(ms);

            PrintSeparator('=');

            ReadWeaponUsage(ms);
        }

        private void PrintSeparator(char sep)
        {
            Console.WriteLine(new string(sep, Console.WindowWidth - 1));
        }

        private void PrintBaseSaveData(BaseSaveSlotInfo baseSaveSlotInfo)
        {
            Console.WriteLine($"Hunter name: {baseSaveSlotInfo.Name}");
            Console.WriteLine($"Rank: {baseSaveSlotInfo.Rank}");
            Console.WriteLine($"Playtime: {MiscUtils.PlaytimeToGameString(baseSaveSlotInfo.Playtime)}");
        }

        private void ReadDecorations(Stream saveData)
        {
            var decorationsReader = new DecorationsReader(saveData);
            foreach (DecorationsSaveSlotInfo decorationInfo in decorationsReader.Read())
            {
                PrintBaseSaveData(decorationInfo);
                Console.WriteLine();
                Console.WriteLine($"{decorationInfo.Decorations.Count} decorations");
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
                    Console.WriteLine("Investigations:");
                    PrintWeaponUsage(weaponUsageInfo.Investigations);
                    Console.WriteLine("Total:");
                    PrintWeaponUsage(weaponUsageInfo.LowRank + weaponUsageInfo.HighRank + weaponUsageInfo.Investigations);
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
