using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MHWSaveUtils
{
    public class SaveSlotInfoBase
    {
        public SaveDataInfo SaveDataInfo { get; private set; }
        public int SlotNumber { get; }
        public string Name { get; }
        public uint Rank { get; }
        public uint Zeni { get; }
        public uint ResearchPoints { get; }
        public uint ExperiencePoints { get; }
        public uint Playtime { get; }

        public SaveSlotInfoBase(int slotNumber, string name, uint rank, uint zeni, uint researchPoints, uint experiencePoints, uint playtime)
        {
            SlotNumber = slotNumber;
            Name = name;
            Zeni = zeni;
            ResearchPoints = researchPoints;
            ExperiencePoints = experiencePoints;
            Rank = rank;
            Playtime = playtime;
        }

        public SaveSlotInfoBase(SaveSlotInfoBase copy)
        {
            SaveDataInfo = copy.SaveDataInfo;
            SlotNumber = copy.SlotNumber;
            Name = copy.Name;
            Zeni = copy.Zeni;
            ResearchPoints = copy.ResearchPoints;
            ExperiencePoints = copy.ExperiencePoints;
            Rank = copy.Rank;
            Playtime = copy.Playtime;
        }

        public void SetSaveDataInfo(SaveDataInfo saveDataInfo)
        {
            SaveDataInfo = saveDataInfo;
        }
    }

    public abstract class SaveDataReaderBase<T> : IDisposable where T : SaveSlotInfoBase
    {
        protected readonly BinaryReader reader;
        protected readonly long saveDataLength;

        protected SaveDataReaderBase(Stream saveData)
        {
            if (saveData == null)
                throw new ArgumentNullException(nameof(saveData));

            if (saveData.CanRead == false)
                throw new ArgumentException($"Argument '{nameof(saveData)}' must be readable, but is not");
            if (saveData.CanSeek == false)
                throw new ArgumentException($"Argument '{nameof(saveData)}' must be seekable, but is not");

            // This call is very costly, so better cache it
            saveDataLength = saveData.Length;

            reader = new BinaryReader(saveData, Encoding.ASCII, true);
        }

        public abstract IEnumerable<T> Read();

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            if (isDisposing)
            {
                // free native resources here
            }

            // free managed resources here
            reader.Dispose();

            isDisposed = true;
        }

        ~SaveDataReaderBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void GotoSection3PastSignature()
        {
            reader.BaseStream.Position =
                64 + // header
                8 * 3 // 3 first sectionIndex
            ;

            long section3Offset = reader.ReadInt64();

            if (section3Offset < 0 || section3Offset >= saveDataLength)
                throw new FormatException($"Invalid section 3 offset ({section3Offset})");

            reader.BaseStream.Position = section3Offset;

            // Here is the section 3

            uint section3Signature = reader.ReadUInt32();
            if (section3Signature != Constants.Section3Signature)
                throw new FormatException($"Invalid section 3 signature, expected {Constants.Section3Signature:X8} but read {section3Signature:X8}");
        }

        protected SaveSlotInfoBase ReadUntilPlaytimeIncluded(int slotNumber)
        {
            byte[] hunterNameBytes = reader.ReadBytes(64);
            string hunterName = Encoding.UTF8.GetString(hunterNameBytes).TrimEnd('\0');
            uint hunterRank = reader.ReadUInt32();
            uint zeni = reader.ReadUInt32();
            uint researchPoints = reader.ReadUInt32();
            uint hunterXP = reader.ReadUInt32();
            uint playTime = reader.ReadUInt32();

            return new SaveSlotInfoBase(slotNumber, hunterName, hunterRank, zeni, researchPoints, hunterXP, playTime);
        }

        protected void Skip(long count)
        {
            reader.BaseStream.Seek(count, SeekOrigin.Current);
        }
    }
}
