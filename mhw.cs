using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public static class mhw {

    // [5392952400, 5376615413, 5391770352, 5371118641]
    private static byte?[] pattern_1 = new byte?[26]
        {
            0x8b,
            0x0d,
            null,
            null,
            null,
            null,
            0x23,
            0xca,
            0x81,
            0xf9,
            0x00,
            0x01,
            0x00,
            0x00,
            0x73,
            0x2f,
            0x0f,
            0xb7,
            null,
            null,
            null,
            null,
            null,
            0xc1,
            0xea,
            0x10,
        };
    private static byte?[] pattern_2 = new byte?[6]{
        (byte)72,
        (byte)137,
        (byte)116,
        (byte)36,
        (byte)56,
        (byte)139,// 115 44 76 139 144 128 131 0 0 131
    };
    /*new byte?[58]
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
    };*/
    private static byte?[] pattern_3 = new byte?[21]{
            0xb2,
            0xac,
            0x0b,
            0x00,
            0x00,
            0x49,
            0x8b,
            0xd9,
            0x8b,
            0x51,
            0x54,
            0x49,
            0x8b,
            0xf8,
            0x48,
            0x8b,
            0x0d,
            null,
            null,
            null,
            null,
    };
    private static byte?[] pattern_player_names = new byte?[37]{
            0x48,
            0x8b,
            0x0d,
            null,
            null,
            null,
            null,
            0x48,
            0x8d,
            0x54,
            0x24,
            0x38,
            0xc6,
            0x44,
            0x24,
            0x20,
            0x00,
            0x4d,
            0x8b,
            0x40,
            0x08,
            0xe8,
            null,
            null,
            null,
            null,
            0x48,
            0x8b,
            0x5c,
            0x24,
            0x60,
            0x48,
            0x83,
            0xc4,
            0x50,
            0x5f,
            0xc3,
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
                pattern_player_names
            };
            ulong[] pattern_results = memory.find_patterns(game, (IntPtr)0x140004000L, (IntPtr)0x145000000L, patterns);
            bool step1_flag = true;
            for(int i = 0; i < 4; i++) {
                step1_flag = step1_flag &&(pattern_results[i] > STEP1_UPR);
                Console.WriteLine("FLAG 1 STEP " + i + " VALUE " + pattern_results[i]);
            }
            mhw_dps_wpf.MainWindow.assert(step1_flag, "failed to locate offsets (step 1).");
            ulong num  = pattern_results[0] +      memory.read_uint(game.Handle, (IntPtr)(long)(pattern_results[0] + 2     )) + 6;
            ulong num2 = pattern_results[1] + 51 + memory.read_uint(game.Handle, (IntPtr)(long)(pattern_results[1] + 54    )) + 7;
            ulong num3 = pattern_results[2] + 15 + memory.read_uint(game.Handle, (IntPtr)(long)(pattern_results[2] + 15 + 2)) + 6;
            ulong num4 = pattern_results[3] +      memory.read_uint(game.Handle, (IntPtr)(long)(pattern_results[3] + 3     )) + 7;
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


    // converts a 4 byte array to an int with big endian byte order
    public static int dword_to_int(ref byte[] array) {
        return array[0] + (array[1] << 0x8) + (array[2] << 0x10) + (array[3] << 0x18);
    }

    // reads at loc1, retuns 0x48 + position + 0x58 * (pattern at loc 1 & offset) 
    private static ulong calculateOffset(Process proc, ulong position, uint offset) {
        uint offset_patter = memory.read_uint(proc.Handle, (IntPtr)loc1); // usually 0xffff
        ulong offset_result = (ulong)((offset_patter & offset) * 0x58L);
        return offset_result + 0x48 + position;
    }

    public static int[] get_player_data(int index) {
        int[] data = new int[5];
        // damage
        // slingers
        // monsters located
        // parts
        // tracks
        byte[] mem_uint32 = new byte[4];
        byte[] mem_uint64 = new byte[8];
        long num = memory.read_long(game.Handle, (IntPtr)damage_base_loc0) + 0x66B0;
        uint offset = memory.read_uint(game.Handle, (IntPtr)(num + 4L * index));
        ulong position = memory.read_ulong(game.Handle, (IntPtr)damage_base_loc1);
        ulong final_offset = calculateOffset(game, position, offset);
        if (final_offset != 0) {
            ulong adress = memory.read_ulong(game.Handle, (IntPtr)(final_offset + 0x48));
            if (adress != 0) {
                read_data_set(adress, mem_uint32, data, 0x00, player_data_indices.damages);
                read_data_set(adress, mem_uint32, data, 0x28, player_data_indices.parts);
                read_data_set(adress, mem_uint32, data, 0x48, player_data_indices.located);
                read_data_set(adress, mem_uint32, data, 0x54, player_data_indices.tracks);
                read_data_set(adress, mem_uint32, data, 0x58, player_data_indices.slingers);
            }
        }
        return data;
    }

    private static void read_data_set(ulong num3, byte[] mem_uint32, int[] data, ulong address_offset, player_data_indices data_index) {
        Process proc = game;
        bool num4 = memory.read_bytes(proc.Handle, (IntPtr)(long)(num3 + 0x48 + address_offset), mem_uint32);
        int player_data = dword_to_int(ref mem_uint32);
        if (num4 && player_data >= 0 && player_data <= 0xFFFFF) {
            data[(int)data_index] = player_data;
        }
    }

    public static int get_player_seat_id() {
        uint num = memory.read_uint(game.Handle, (IntPtr)names_base_adress);
        uint num2 = memory.read_uint(game.Handle, (IntPtr)(num + (is_wegame_build ? 0x560 : 0x258)));
        int result = -1;
        if (num2 > 0x1000) {
            uint num3 = memory.read_uint(game.Handle, (IntPtr)(long)(num2 + 0x10));
            if (num3 != 0) {
                result = (int)memory.read_uint(game.Handle, (IntPtr)(num3 + (is_wegame_build ? 0xC0F4 : 0xBFEC)));
            }
        }
        return result;
    }

    public static MonsterInfo getMonsterInfo(int slot) {
        //long monster_adress = monster_health_base_adress + (long)(slot * 0x60);
        MonsterInfo info = new MonsterInfo();
        //if (slot != 0) return info; // todo 
        //byte[] mem_uint64 = new byte[8];
        byte[] mem_float = new byte[4];
        /*ReadProcessMemory(proc.Handle, (IntPtr)monster_adress, mem_uint64);
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
        }*/ // 8a8 3b0 18 58 64

        ulong monster_info_adr = getULongFromPointerChain(new ulong[] { 0x03b318e0,0xc58, 0x8a8, 0x3b0, 0x18, 0x58});
        memory.read_bytes(game.Handle, (IntPtr)monster_info_adr + 0x60, mem_float);
        info.maxhp = BitConverter.ToSingle(mem_float, 0);
        memory.read_bytes(game.Handle, (IntPtr)monster_info_adr + 0x64, mem_float);
        info.hp = BitConverter.ToSingle(mem_float, 0);
        return info;
    }

    private static ulong getULongFromPointerChain(ulong[] chain) {
        byte[] mem_uint64 = new byte[8];
        ulong adress;
        for (int i = 0; i < chain.Length; i++) {
            if (i == 0) {
                adress = (ulong)game.MainModule.BaseAddress.ToInt64();
            } else {
                adress = BitConverter.ToUInt64(mem_uint64, 0);
            }
            memory.read_bytes(game.Handle, (IntPtr)(adress + chain[i]), mem_uint64);
        }
        return BitConverter.ToUInt64(mem_uint64, 0);
    }

    public struct MonsterInfo {
        
        public float hp;
        public float maxhp;
    }

    //legacy
    public static string[] get_team_player_names() {
        string[] names = new string[4];
        byte[] mem_name_string = new byte[0x28];
        int num = (int)memory.read_uint(game.Handle, (IntPtr)names_base_adress) + (is_wegame_build ? 0x555B5 : 0x54A45);
        for (int i = 0; i < 4; i++) {
            Array.Resize(ref mem_name_string, 0x28);
            memory.read_bytes(game.Handle, (IntPtr)(num + 0x21 * i), mem_name_string);
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
        uint names_adress = memory.read_uint(game.Handle, (IntPtr)names_base_adress) + (uint)(is_wegame_build ? 0x555B5 : 0x54A45);
        return memory.read_string(game.Handle, (IntPtr)(names_adress + 0x21 * index), 0x28);
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
