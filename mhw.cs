﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public static class mhw {
    private static byte?[] pattern_1 = new byte?[26]
        {
            (byte)139,
            (byte)13,
            null,
            null,
            null,
            null,
            (byte)35,
            (byte)202,
            (byte)129,
            (byte)249,
            0,
            (byte)1,
            0,
            0,
            (byte)115,
            (byte)47,
            (byte)15,
            (byte)183,
            null,
            null,
            null,
            null,
            null,
            (byte)193,
            (byte)234,
            (byte)16
        };
    private static byte?[] pattern_2 = new byte?[58]
    {
            (byte)72,
            (byte)137,
            (byte)116,
            (byte)36,
            (byte)56,
            (byte)139,
            (byte)112,
            (byte)24,
            (byte)72,
            (byte)139,
            null,
            null,
            null,
            null,
            null,
            (byte)137,
            (byte)136,
            (byte)12,
            (byte)5,
            0,
            0,
            (byte)72,
            (byte)139,
            null,
            null,
            null,
            null,
            null,
            (byte)137,
            (byte)144,
            (byte)16,
            (byte)5,
            0,
            0,
            (byte)72,
            (byte)139,
            null,
            null,
            null,
            null,
            null,
            (byte)137,
            (byte)152,
            (byte)20,
            (byte)5,
            0,
            0,
            (byte)133,
            (byte)219,
            (byte)126,
            null,
            (byte)72,
            (byte)139,
            null,
            null,
            null,
            null,
            null
    };
    private static byte?[] pattern_3 = new byte?[21]
    {
            (byte)178,
            (byte)172,
            (byte)11,
            0,
            0,
            (byte)73,
            (byte)139,
            (byte)217,
            (byte)139,
            (byte)81,
            (byte)84,
            (byte)73,
            (byte)139,
            (byte)248,
            (byte)72,
            (byte)139,
            (byte)13,
            null,
            null,
            null,
            null
    };
    private static byte?[] pattern_4 = new byte?[37]
    {
            (byte)72,
            (byte)139,
            (byte)13,
            null,
            null,
            null,
            null,
            (byte)72,
            (byte)141,
            (byte)84,
            (byte)36,
            (byte)56,
            (byte)198,
            (byte)68,
            (byte)36,
            (byte)32,
            0,
            (byte)77,
            (byte)139,
            (byte)64,
            (byte)8,
            (byte)232,
            null,
            null,
            null,
            null,
            (byte)72,
            (byte)139,
            (byte)92,
            (byte)36,
            (byte)96,
            (byte)72,
            (byte)131,
            (byte)196,
            (byte)80,
            (byte)95,
            (byte)195
    };

    private static ulong STEP1_UPR = 0x1400FFFFFL;
    private static ulong STEP2_LWR = 0x140004000L;
    private static ulong STEP2_UPR = 0x150000000L; 

    public static bool is_wegame_build = false;

    public static long loc1 = -1L;

    public static long damage_base_loc0 = -1L;

    public static long damage_base_loc1 = -1L;

    public static long names_base_adress = -1L;

    public static long monster_health_base_adress = -1L;

    private static Process game;

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

    public static void find_game_proc() {
        IEnumerable<Process> source = from x in Process.GetProcesses()
                                      where x.ProcessName == "MonsterHunterWorld"
                                      select x;
        mhw_dps_wpf.MainWindow.assert(source.Count() == 1, "frm_main_load: #proc not 1. (Is the game running?)");
        game = source.FirstOrDefault();
        try {
            Console.WriteLine("Game base adress 0x" + game.MainModule.BaseAddress.ToString("X"));
        } catch (Exception) {
            mhw_dps_wpf.MainWindow.assert(flag: false, "access denied. (Is the game running as admin while the tool isn't? ) WEGAME版必须以管理员身份运行");
        }
    }

    // checks the processes memory structure and loads gets adresses 
    public static void initMemory() {
        find_game_proc();
        mhw.is_wegame_build = game.MainWindowTitle.Contains("怪物猎人 世界"); // Monster hunter world
        bool flag = game.MainWindowTitle.Contains("3142");
        if (!mhw.is_wegame_build) {
            Console.WriteLine("main module base adress 0x" + game.MainModule.BaseAddress.ToString("X"));
            List<byte?[]> patterns = new List<byte?[]> {
                pattern_1,
                pattern_2,
                pattern_3,
                pattern_4
            };
            ulong[] array = memory.find_patterns(game, (IntPtr)0x140004000L, (IntPtr)0x145000000L, patterns);
            bool step1_flag = true;
            for(int i = 1; i < 4; i++) {
                step1_flag = step1_flag &&(array[i] > STEP1_UPR);
            }
            mhw_dps_wpf.MainWindow.assert(step1_flag, "failed to locate offsets (step 1).");
            ulong num  = array[0] +      mhw.read_uint(game.Handle, (IntPtr)(long)(array[0] + 2     )) + 6;
            ulong num2 = array[1] + 51 + mhw.read_uint(game.Handle, (IntPtr)(long)(array[1] + 54    )) + 7;
            ulong num3 = array[2] + 15 + mhw.read_uint(game.Handle, (IntPtr)(long)(array[2] + 15 + 2)) + 6;
            ulong num4 = array[3] +      mhw.read_uint(game.Handle, (IntPtr)(long)(array[3] + 3     )) + 7;
            Console.WriteLine("loc1              0x" + num.ToString("X"));
            Console.WriteLine("dmg base adress 0 0x" + num2.ToString("X"));
            Console.WriteLine("dmg base adress 1 0x" + num3.ToString("X"));
            Console.WriteLine("names base adress 0x" + num4.ToString("X"));
            mhw_dps_wpf.MainWindow.assert(
                num  > STEP2_LWR && num  < STEP2_UPR &&
                num2 > STEP2_LWR && num2 < STEP2_UPR &&
                num3 > STEP2_LWR && num3 < STEP2_UPR &&
                num4 > STEP2_LWR && num4 < STEP2_UPR,
                "failed to locate offsets (step 2).");
            mhw.loc1 = (long)num;
            mhw.damage_base_loc0 = (long)num2;
            mhw.damage_base_loc1 = (long)num3;
            mhw.names_base_adress = (long)num4;
            //--
            mhw.monster_health_base_adress = game.MainModule.BaseAddress.ToInt64() + 0x3b79ec0;
        } else {
            mhw_dps_wpf.MainWindow.assert(flag, "版本错误，必须为3142才能使用"); //The version is wrong and must be 3142 to use
            mhw.loc1 =              0x14397C872L;
            mhw.damage_base_loc0 =  0x143B2E4D8L;
            mhw.damage_base_loc1 =  0x143B30EA8L;
            mhw.names_base_adress = 0x1448248B8L;
        }
    }

    // reads a bytearray from a process at a given address, the lenght is the lenght of the buffer given to write to
    private static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer) {
        int lpNumberOfBytesRead = 0;
        return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesRead);
    }

    // reads an uint64 from a process at a given address
    public static ulong read_ulong(IntPtr hProcess, IntPtr lpBaseAddress) {
        byte[] array = new byte[8];
        ReadProcessMemory(hProcess, lpBaseAddress, array);
        return BitConverter.ToUInt64(array, 0);
    }

    // reads an uint32 from a process at a given address
    public static uint read_uint(IntPtr hProcess, IntPtr lpBaseAddress) {
        byte[] data = new byte[4];
        ReadProcessMemory(hProcess, lpBaseAddress, data);
        return BitConverter.ToUInt32(data, 0);
    }

    // converts a 4 byte array to an int with big endian byte order
    public static int dword_to_int(ref byte[] array) {
        return array[0] + (array[1] << 0x8) + (array[2] << 0x10) + (array[3] << 0x18);
    }

    // reads at loc1, retuns 0x48 + param 2 + 0x58 * read_mem & param3 
    private static ulong asm_func1(Process proc, ulong rcx, uint edx) {
        uint num = read_uint(proc.Handle, (IntPtr)loc1);
        ulong num2 = rcx;
        rcx = (ulong)((long)(num & edx) * 0x58L);
        return num2 + 0x48 + rcx;
    }

    public static int[][] get_team_data() {
        Process proc = game;
        int[][] data = new int[5][];
        //TODO fix this
        for(int i = 0; i < 4; i++) {
            data[i] = new int[5];
        }
        return data;

        data[(int)player_data_indices.damages] = new int[4];  // damage
        data[(int)player_data_indices.slingers] = new int[4]; // slingers
        data[(int)player_data_indices.located] = new int[4];  // monsters located
        data[(int)player_data_indices.parts] = new int[4];    // parts
        data[(int)player_data_indices.tracks] = new int[4];   // tracks
        byte[] mem_uint32 = new byte[4];
        byte[] mem_uint64 = new byte[8];
        ReadProcessMemory(proc.Handle, (IntPtr)damage_base_loc0, mem_uint64);
        ulong num = BitConverter.ToUInt64(mem_uint64, 0) + 0x66B0;
        ReadProcessMemory(proc.Handle, (IntPtr)damage_base_loc1, mem_uint64);
        ulong rcx = BitConverter.ToUInt64(mem_uint64, 0);
        for (int i = 0; i < 4; i++) {
            ReadProcessMemory(proc.Handle, (IntPtr)((long)num + 4L * (long)i), mem_uint32);
            uint edx = read_uint(proc.Handle, (IntPtr)((long)num + 4L * (long)i));
            ulong num2 = asm_func1(proc, rcx, edx);
            if (num2 != 0) {
                ReadProcessMemory(proc.Handle, (IntPtr)(long)(num2 + 0x48), mem_uint64);
                ulong num3 = BitConverter.ToUInt64(mem_uint64, 0);
                if (num3 != 0) {
                    read_data_set(num3, mem_uint32, data[i], 0, player_data_indices.damages);
                    read_data_set(num3, mem_uint32, data[i], 0x28, player_data_indices.parts);
                    read_data_set(num3, mem_uint32, data[i], 0x48, player_data_indices.located);
                    read_data_set(num3, mem_uint32, data[i], 0x54, player_data_indices.tracks);
                    read_data_set(num3, mem_uint32, data[i], 0x58, player_data_indices.slingers);
                }
            }
        }
        return data;
    }

    public static int[] get_player_data(int index) {
        Process proc = game;
        int[] data = new int[5];
        // damage
        // slingers
        // monsters located
        // parts
        // tracks
        byte[] mem_uint32 = new byte[4];
        byte[] mem_uint64 = new byte[8];
        ReadProcessMemory(proc.Handle, (IntPtr)damage_base_loc0, mem_uint64);
        ulong num = BitConverter.ToUInt64(mem_uint64, 0) + 0x66B0;
        ReadProcessMemory(proc.Handle, (IntPtr)damage_base_loc1, mem_uint64);
        ulong rcx = BitConverter.ToUInt64(mem_uint64, 0);
        ReadProcessMemory(proc.Handle, (IntPtr)((long)num + 4L * (long)index), mem_uint32);
        uint edx = read_uint(proc.Handle, (IntPtr)((long)num + 4L * (long)index));
        ulong num2 = asm_func1(proc, rcx, edx);
        if (num2 != 0) {
            ReadProcessMemory(proc.Handle, (IntPtr)(long)(num2 + 0x48), mem_uint64);
            ulong num3 = BitConverter.ToUInt64(mem_uint64, 0);
            if (num3 != 0) {
                read_data_set(num3, mem_uint32, data, 0, player_data_indices.damages);
                read_data_set(num3, mem_uint32, data, 0x28, player_data_indices.parts);
                read_data_set(num3, mem_uint32, data, 0x48, player_data_indices.located);
                read_data_set(num3, mem_uint32, data, 0x54, player_data_indices.tracks);
                read_data_set(num3, mem_uint32, data, 0x58, player_data_indices.slingers);
            }
        }
        return data;
    }

    private static void read_data_set(ulong num3, byte[] mem_uint32, int[] data, ulong address_offset, player_data_indices data_index) {
        Process proc = game;
        bool num4 = ReadProcessMemory(proc.Handle, (IntPtr)(long)(num3 + 0x48 + address_offset), mem_uint32);
        int player_data = dword_to_int(ref mem_uint32);
        if (num4 && player_data >= 0 && player_data <= 0xFFFFF) {
            data[(int)data_index] = player_data;
        }
    }

    public static int get_player_seat_id() {
        uint num = read_uint(game.Handle, (IntPtr)names_base_adress);
        uint num2 = read_uint(game.Handle, (IntPtr)(num + (is_wegame_build ? 0x560 : 0x258)));
        int result = -1;
        if (num2 > 0x1000) {
            uint num3 = read_uint(game.Handle, (IntPtr)(long)(num2 + 0x10));
            if (num3 != 0) {
                result = (int)read_uint(game.Handle, (IntPtr)(num3 + (is_wegame_build ? 0xC0F4 : 0xBFEC)));
            }
        }
        return result;
    }

    public static MonsterInfo updateHealthInfo(int slot) {
        Process proc = game;
        long monster_adress = monster_health_base_adress + (long)(slot * 0x60);
        byte[] mem_uint64 = new byte[8];
        byte[] mem_float = new byte[4];
        MonsterInfo info = new MonsterInfo();
        ReadProcessMemory(proc.Handle, (IntPtr)monster_adress, mem_uint64);
        ulong of0 = BitConverter.ToUInt64(mem_uint64, 0);
        ReadProcessMemory(proc.Handle, (IntPtr)of0 + 0xc58, mem_uint64);
        ulong of1 = BitConverter.ToUInt64(mem_uint64, 0);
        if (of1 > 0x0) {
            ReadProcessMemory(proc.Handle, (IntPtr)of1 + 0x8a8, mem_uint64);
            ulong of2 = BitConverter.ToUInt64(mem_uint64, 0);
            ReadProcessMemory(proc.Handle, (IntPtr)of2 + 0x3b0, mem_uint64);
            ulong of3 = BitConverter.ToUInt64(mem_uint64, 0);
            ReadProcessMemory(proc.Handle, (IntPtr)of3 + 0x18, mem_uint64);
            ulong of4 = BitConverter.ToUInt64(mem_uint64, 0);
            ReadProcessMemory(proc.Handle, (IntPtr)of4 + 0x58, mem_uint64);
            ulong of5 = BitConverter.ToUInt64(mem_uint64, 0);
            ReadProcessMemory(proc.Handle, (IntPtr)of5 + 0x60, mem_float);
            info.maxhp = BitConverter.ToSingle(mem_float, 0);
            ReadProcessMemory(proc.Handle, (IntPtr)of5 + 0x64, mem_float);
            info.hp = BitConverter.ToSingle(mem_float, 0);
        }
        return info;
    }

    public struct MonsterInfo {
        
        public float hp;
        public float maxhp;
    }

    //legacy
    public static string[] get_team_player_names() {
        string[] names = new string[4];
        byte[] mem_name_string = new byte[0x28];
        int num = (int)read_uint(game.Handle, (IntPtr)names_base_adress) + (is_wegame_build ? 0x555B5 : 0x54A45);
        for (int i = 0; i < 4; i++) {
            Array.Resize(ref mem_name_string, 0x28);
            ReadProcessMemory(game.Handle, (IntPtr)(num + 0x21 * i), mem_name_string);
            int name_len = Array.FindIndex(mem_name_string, (byte x) => x == 0);
            if (name_len <= 0) {
                names[i] = "";
            } else {
                Array.Resize(ref mem_name_string, name_len);
                names[i] = Encoding.UTF8.GetString(mem_name_string);
            }
        }
        return names;
    }

    public static string get_team_player_name(int index) {
        string name;
        byte[] mem_name_string = new byte[0x28];
        int num = (int)read_uint(game.Handle, (IntPtr)names_base_adress) + (is_wegame_build ? 0x555B5 : 0x54A45);
        Array.Resize(ref mem_name_string, 0x28);
        ReadProcessMemory(game.Handle, (IntPtr)(num + 0x21 * index), mem_name_string);
        int name_len = Array.FindIndex(mem_name_string, (byte x) => x == 0);
        if (name_len <= 0) {
            name = "";
        } else {
            Array.Resize(ref mem_name_string, name_len);
            name = Encoding.UTF8.GetString(mem_name_string);
        }
        return name;
    }

    public static bool hasGameExited() {
        return game.HasExited;
    }
}

public enum player_data_indices {
   damages = 0,
   slingers = 1,
   located = 2,
   parts = 3,
   tracks = 4
}
