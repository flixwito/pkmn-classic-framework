﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PkmnFoundations.Support;

namespace PkmnFoundations.Structures
{
    public class Pokemon4 : PokemonBase
    {
        public Pokemon4(Pokedex.Pokedex pokedex) : base(pokedex)
        {

        }

        public Pokemon4(Pokedex.Pokedex pokedex, BinaryReader data) : base(pokedex)
        {
            Load(data);
        }

        public Pokemon4(Pokedex.Pokedex pokedex, byte[] data) : base(pokedex)
        {
            Load(data);
        }

        public Pokemon4(Pokedex.Pokedex pokedex, byte[] data, int offset) : base(pokedex)
        {
            Load(data, offset);
        }

        protected override void Load(BinaryReader reader)
        {
            // header (unencrypted)
            Personality = reader.ReadUInt32();
            ushort zero = reader.ReadUInt16();
            ushort checksum = reader.ReadUInt16();

            // read out the main payload, apply xor decryption
            byte[][] blocks = new byte[4][];
            int rand = (int)checksum;

            for (int x = 0; x < 4; x++)
            {
                byte[] block = blocks[x] = reader.ReadBytes(32);
                // todo: extract to method
                for (int pos = 0; pos < 32; pos += 2)
                {
                    rand = DecryptRNG(rand);
                    block[pos] ^= (byte)(rand >> 16);
                    block[pos + 1] ^= (byte)(rand >> 24);
                }
            }

            // shuffle blocks to their correct order
            List<int> blockSequence = Invert(BlockScramble((Personality & 0x0003e000) >> 0x0d));
            AssertHelper.Equals(blockSequence.Count, 4);
            {
                byte[][] blocks2 = new byte[4][];
                for (int x = 0; x < 4; x++)
                    blocks2[x] = blocks[blockSequence[x]];
                blocks = blocks2;
            }

            int ribbons1, ribbons2, ribbons3;

            {
                byte[] block = blocks[0];

                SpeciesID = BitConverter.ToUInt16(block, 0);
                HeldItemID = BitConverter.ToUInt16(block, 2);
                TrainerID = BitConverter.ToUInt32(block, 4);
                Experience = BitConverter.ToInt32(block, 8);
                Happiness = block[12];
                AbilityID = block[13];
                Markings = (Markings)block[14];
                Language = (Languages)block[15];
                EVs = new ByteStatValues(block[16],
                                         block[17],
                                         block[18],
                                         block[19],
                                         block[20],
                                         block[21]);
                ContestStats = new ConditionValues(block[22],
                                                   block[23], 
                                                   block[24], 
                                                   block[25], 
                                                   block[26], 
                                                   block[27]);

                ribbons1 = BitConverter.ToInt32(block, 28);
            }

            {
                byte[] block = blocks[1];

                Moves[0] = new MoveSlot(m_pokedex, BitConverter.ToUInt16(block, 0), block[8], block[12]);
                Moves[1] = new MoveSlot(m_pokedex, BitConverter.ToUInt16(block, 2), block[9], block[13]);
                Moves[2] = new MoveSlot(m_pokedex, BitConverter.ToUInt16(block, 4), block[10], block[14]);
                Moves[3] = new MoveSlot(m_pokedex, BitConverter.ToUInt16(block, 6), block[11], block[15]);

                int ivs = BitConverter.ToInt32(block, 16);
                IVs = new IvStatValues(ivs & 0x3fffffff);
                IsEgg = (ivs & 0x40000000) != 0;
                HasNickname = (ivs & 0x80000000) != 0;

                ribbons2 = BitConverter.ToInt32(block, 20);

                byte forme = block[24];
                FatefulEncounter = (forme & 0x01) != 0;
                Female = (forme & 0x02) != 0;
                Genderless = (forme & 0x04) != 0;
                FormID = (byte)(forme >> 3);

                // todo: parse this in a meaningful way.
                ShinyLeaves = block[25];
                Unknown1 = BitConverter.ToUInt16(block, 26);

                // todo: doing trainer memos the right way is a pretty large task
                // involving new database work.
                TrainerMemoExtended = BitConverter.ToInt32(block, 28);
            }

            {
                byte[] block = blocks[2];

                NicknameEncoded = new EncodedString4(block, 0, 22);
                Unknown2 = block[22];
                Version = (Versions)block[23];
                ribbons3 = BitConverter.ToInt32(block, 24);
                Unknown3 = BitConverter.ToInt32(block, 28);
            }

            {
                byte[] block = blocks[3];

                TrainerNameEncoded = new EncodedString4(block, 0, 16);

                // todo: parse dates
                EggDate = new byte[3];
                Array.Copy(block, 16, EggDate, 0, 3);
                Date = new byte[3];
                Array.Copy(block, 19, Date, 0, 3);

                TrainerMemo = BitConverter.ToInt32(block, 22);
                PokerusStatus = block[26];
                PokeBallID_DP = block[27];

                byte encounter_level = block[28];
                EncounterLevel = (byte)(encounter_level & 0x7f);
                TrainerFemale = (encounter_level & 0x80) != 0;

                EncounterType = block[29];
                PokeBallID = block[30];
                Unknown4 = block[31];
            }

            // todo: split this class into separate box/party versions.
            {
                rand = (int)Personality;
                byte[] block = reader.ReadBytes(100);
                for (int pos = 0; pos < 100; pos += 2)
                {
                    rand = DecryptRNG(rand);
                    block[pos] ^= (byte)(rand >> 24);
                    block[pos + 1] ^= (byte)(rand >> 16);
                }

                StatusAffliction = block[0];
                Unknown5 = block[1];
                Unknown6 = BitConverter.ToUInt16(block, 2);
                Level = block[4];
                CapsuleIndex = block[5];
                HP = BitConverter.ToUInt16(block, 6);
                Stats = new IntStatValues(BitConverter.ToUInt16(block, 8),
                    BitConverter.ToUInt16(block, 10),
                    BitConverter.ToUInt16(block, 12),
                    BitConverter.ToUInt16(block, 14),
                    BitConverter.ToUInt16(block, 16),
                    BitConverter.ToUInt16(block, 18));

                Unknown7 = new byte[56];
                Array.Copy(block, 20, Unknown7, 0, 56);
                Seals = new byte[24];
                Array.Copy(block, 76, Seals, 0, 24);
            }

            Ribbons1 = ribbons1;
            Ribbons2 = ribbons2;
            Ribbons3 = ribbons3;

            // todo: Ribbons need to be tracked in the database and stored in a meaningful way.
            // pkmncf_pokedex_ribbons
            // id, value3, value4, value5, value6, Name_JA, etc, Description_JA, etc
            // value >> 3 --> array offset.
            // value & 0x07 --> bit position.
            /*
            byte[] ribbons = new byte[12];
            Array.Copy(BitConverter.GetBytes(ribbons1), 0, ribbons, 0, 4);
            Array.Copy(BitConverter.GetBytes(ribbons2), 0, ribbons, 4, 4);
            Array.Copy(BitConverter.GetBytes(ribbons3), 0, ribbons, 8, 4);

            HashSet<Ribbon> Ribbons = new HashSet<Ribbon>();
            foreach (Ribbon r in m_pokedex.Ribbons(Generations.Generation4))
            {
                if (HasRibbon(ribbons, r.Value4))
                    ActualRibbons.Add(r);
            }
            */
        }

        bool HasRibbon(byte[] ribbons, int value)
        {
            if (value >= 96 || value < 0) throw new ArgumentOutOfRangeException();
            int offset = value >> 3;
            byte mask = (byte)(1 << (value & 0x07));
            return (ribbons[offset] & mask) != 0;
        }

        protected override void Save(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override Generations Generation
        {
            get { return Generations.Generation4; }
        }

        // todo: link experience with level
        public int Experience { get; set; }
        public Markings Markings { get; set; }
        public ConditionValues ContestStats { get; set; }
        public bool IsEgg { get; set; }
        public bool HasNickname { get; set; }
        public bool FatefulEncounter { get; set; } // aka. obedience flag
        // todo: validate gender against PV
        public bool Female { get; set; } // only part of gender calculations
        public bool Genderless { get; set; }
        // todo: parse shiny leaves data
        public byte ShinyLeaves { get; set; }
        public ushort Unknown1 { get; set; }
        // todo: parse trainer memos (location, level)
        // this will require some database work.
        public int TrainerMemoExtended { get; set; } // trainer memo for PtHGSS
        public EncodedString4 NicknameEncoded { get; set; }
        public override string Nickname
        {
            get
            {
                return (NicknameEncoded == null) ? null : NicknameEncoded.Text;
            }
            set
            {
                if (Nickname == value) return;
                if (NicknameEncoded == null) NicknameEncoded = new EncodedString4(value, 22);
                else NicknameEncoded.Text = value;
            }
        }
        public byte Unknown2 { get; set; }
        public Versions Version { get; set; }
        public int Unknown3 { get; set; }
        public EncodedString4 TrainerNameEncoded { get; set; }
        // fixme: use DateTimes for these
        public byte[] EggDate { get; set; }
        public byte[] Date { get; set; }
        public int TrainerMemo { get; set; }
        // todo: parse pokerus
        public byte PokerusStatus { get; set; }
        // todo: need list of values and map them onto items
        public byte PokeBallID_DP { get; set; }
        public byte PokeBallID { get; set; }
        public byte EncounterLevel { get; set; }
        public bool TrainerFemale { get; set; }
        // this is the notorious genIV encounter type flag, not used for much besides validation
        public byte EncounterType { get; set; }
        public byte Unknown4 { get; set; }

        public int Ribbons1 { get; set; }
        public int Ribbons2 { get; set; }
        public int Ribbons3 { get; set; }

        public byte StatusAffliction { get; set; }
        public byte Unknown5 { get; set; }
        public ushort Unknown6 { get; set; }
        public byte CapsuleIndex { get; set; }
        public ushort HP { get; set; }
        public IntStatValues Stats { get; set; } // cached stats (only refreshes per level on gen4)
        public byte[] Unknown7 { get; set; }
        public byte[] Seals { get; set; }

        private static int DecryptRNG(int prev)
        {
            return prev * 0x41c64e6d + 0x6073;
        }

        public override int Size
        {
            get { return 236; }
        }
    }
}
