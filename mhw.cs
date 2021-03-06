﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using mhw_dps_wpf;

public static class mhw {

    // [5392952400, 5376615413, 5391770352, 5371118641]
    private static readonly byte?[] pattern_1 = new byte?[]
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
    private static readonly byte?[] pattern_2 = new byte?[]{
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
    private static readonly byte?[] pattern_3 = new byte?[]{
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
    private static readonly byte?[] pattern_player_names = new byte?[]{
        0x48,0x8b,0x0d,null,null,null,null,0x48,0x8d,0x54,0x24,0x38,0xc6,0x44,0x24,0x20,0x00,0x4d,0x8b,0x40,0x08,0xe8,null,null,null,null,0x48,0x8b,0x5c,0x24,0x60,0x48,0x83,0xc4,0x50,0x5f,0xc3
    };
    private static readonly byte?[] pattern_player_damage = new byte?[]{
        0x48,0x8b,0x0d,null,null,null,null,0xe8,null,null,null,null,0x48,0x8b,0xd8,0x48,0x85,0xc0, 0x75, 0x04, 0x33
        //48   8B    0D ??    ??   ??   ??   E8   ??   ??   ??   ??    48   8B  D8   48   85 C0 75 04 33 C9
    };
    private static readonly byte?[] pattern_monster = new byte?[]{
        0x48,0x8b,0x15,null,null,null,null,0x48,0x8b,0x29,0x48,0x63,0x82,null,null,null,null
    };
    private static readonly byte?[] pattern_monster_1 = new byte?[]{
        0x48,0x8b,0x8b,null,null,null,null,0x48,0x8b,0x01,0xff,0x50,null,0x48,0x8b,0x8b,null,null,null,null,0xe8,null,null,null,null,0x48,0x8B,0x8B,null,null,null,null,0xb2,0x01,0xe8,null,null,null,null
    };

    private static ulong player_damages_root_offset;
    private static ulong player_names_root_offset;
    private static ulong monster_base_root_offset;
    private static ulong monster_offset_1;

    private static ulong STEP1_UPR = 0x1400fffffL;
    private static ulong STEP2_LWR = 0x140004000L;
    private static ulong STEP2_UPR = 0x150000000L; 


    public static long damage_base_loc0 = -1L;

    public static long damage_base_loc1 = -1L;

    public static long names_base_address = -1L;


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
            Console.WriteLine("Game base address 0x" + game.MainModule.BaseAddress.ToString("X"));
        } catch (Exception) {
            mhw_dps_wpf.MainWindow.assert(flag: false, "access denied. (Is the game running as admin while the tool isn't? ) WEGAME版必须以管理员身份运行");
        }
    }

    // checks the processes memory structure and loads gets addresses 
    public static void initMemory() {
        find_game_proc();
        bool flag = game.MainWindowTitle.Contains("3142");
        Console.WriteLine("main module base address 0x" + game.MainModule.BaseAddress.ToString("X"));
        List<byte?[]> patterns = new List<byte?[]> {
            pattern_2,
            pattern_player_names,
            pattern_player_damage,
            pattern_monster,
            pattern_monster_1
        };
        ulong[] pattern_results = MemoryHelper.find_patterns(game, (IntPtr)0x140004000L, (IntPtr)0x151000000L, patterns);
        bool step1_flag = true;
        for(int i = 0; i < 4; i++) {
            step1_flag = step1_flag &&(pattern_results[i] > STEP1_UPR);
            Console.WriteLine("FLAG 1 STEP " + i + " VALUE 0x" + pattern_results[i].ToString("X"));
        }
        mhw_dps_wpf.MainWindow.assert(step1_flag, "failed to locate offsets (step 1).");
        //ulong num  = pattern_results[0] +      MemoryHelper.read_uint(game.Handle, (UIntPtr)(long)(pattern_results[0] + 2     )) + 6;
        ulong num2 = pattern_results[0] + 51 + MemoryHelper.read_uint(game.Handle, (UIntPtr)(long)(pattern_results[0] + 54    )) + 7;
        //ulong num3 = pattern_results[2] + 15 + MemoryHelper.read_uint(game.Handle, (UIntPtr)(long)(pattern_results[2] + 15 + 2)) + 6;
        player_names_root_offset = pattern_results[1];
        player_damages_root_offset = pattern_results[2];
        monster_base_root_offset = pattern_results[3];
        monster_offset_1 = pattern_results[4];
        ulong num4 = pattern_results[1] +      MemoryHelper.read_uint(game.Handle, (UIntPtr)(long)(pattern_results[1] + 3     )) + 7;
        //Console.WriteLine("loc1              0x" + num.ToString("X"));
        Console.WriteLine("dmg base address 0 0x" + num2.ToString("X"));
        //Console.WriteLine("dmg base address 1 0x" + num3.ToString("X"));
        Console.WriteLine("names base address 0x" + num4.ToString("X"));
        mhw_dps_wpf.MainWindow.assert(
            //num  > STEP2_LWR && num  < STEP2_UPR &&
            num2 > STEP2_LWR && num2 < STEP2_UPR &&
            //num3 > STEP2_LWR && num3 < STEP2_UPR &&
            num4 > STEP2_LWR && num4 < STEP2_UPR,
            "failed to locate offsets (step 2)."
        );

        //--
        mhw.names_base_address = (long)num4;
    }

    private static readonly ulong PREV_MONSTER_PTR = 0x28;

    public static void getMonsterInfo(MonsterList list) {
        ulong monsterAndBuffRootPtr = loadEffectiveAddressRelative(game, monster_base_root_offset);
        uint monsterAndBuffOffset1 = readStaticOffset(game, monster_offset_1);
        ulong lastMonsterAddress = MemoryHelper.read_pointer_chain(game, monsterAndBuffRootPtr, monsterAndBuffOffset1, 0x8F9BC * 8, 0, 0);

        List<ulong> monsterAddresses = new List<ulong>();

        if (lastMonsterAddress < 0xffffff){
            // no monsters found
            return;
        }

        ulong currentMonsterAddress = lastMonsterAddress;
        //int i = 0;
        while (currentMonsterAddress != 0) {
            monsterAddresses.Insert(0, currentMonsterAddress);
            lastMonsterAddress = currentMonsterAddress;
            currentMonsterAddress = MemoryHelper.read_ulong(game.Handle, (UIntPtr)(currentMonsterAddress + PREV_MONSTER_PTR));
            //i++;
            if(currentMonsterAddress == lastMonsterAddress) break;
        }
        //Console.WriteLine(i + " monster candidates");

        foreach (ulong monsterAddress in monsterAddresses) {
            readMonster(monsterAddress, list);
        }
    }

    public static uint readStaticOffset(Process process, ulong address){
        const uint opcodeLength = 3;
        return MemoryHelper.read_uint(game.Handle, (UIntPtr)(address + opcodeLength));
    }

    // Monster
    //public static readonly ulong NextMonsterPtr = 0x30;
    private static readonly ulong MONSTER_SIZE_SCALE_OFFSET = 0x174;
    private static readonly ulong MONSTER_MODEL_OFFSET = 0x290;
    private static readonly ulong MONSTER_PARTS_OFFSET = 0x129D8;
    //public static readonly ulong RemovablePartCollection = PartCollection + 0x1ED0;
    //public static readonly ulong StatusEffectCollection = 0x19870;
    // MonsterModel
    private static readonly uint MONSTER_MODEL_ID_LENGTH = 64;
    private static readonly ulong MONSTER_MODEL_ID_OFFSET = 0x0C;
    
    // MonsterHealthComponent
    public static readonly ulong MONSTER_MAX_HEALTH_OFFSET = 0x60;
    public static readonly ulong MONSTER_CURRENT_HEALTH_OFFSET = 0x64;

    //MonsterPartCollection
    //public static readonly int MaxItemCount = 16;
    public static readonly ulong MONSTER_PART_HEALTH_OFFSET = 0x48;
    //public static readonly ulong FirstPart = 0x50;

    private static void readMonster(ulong monsterAddress, MonsterList list){
        ulong modelPtr = MemoryHelper.read_ulong(game.Handle, (UIntPtr)(monsterAddress + MONSTER_MODEL_OFFSET));
        string id = MemoryHelper.read_string(game.Handle, (UIntPtr)(modelPtr + MONSTER_MODEL_ID_OFFSET), MONSTER_MODEL_ID_LENGTH); //TODO null byte
        if (id == "") {
            return;
        }
        

        id = id.Split('\\').Last();
        if (!isMonsterIDValid(id)) {
            return;
        }

        ulong healthComponentAddress = MemoryHelper.read_ulong(game.Handle, (UIntPtr)(monsterAddress + MONSTER_PARTS_OFFSET + MONSTER_PART_HEALTH_OFFSET));
        float maxHealth = MemoryHelper.read_float(game.Handle, (UIntPtr)(healthComponentAddress + MONSTER_MAX_HEALTH_OFFSET));
        if (maxHealth <= 0){
            return;
        }

        float currentHealth = MemoryHelper.read_float(game.Handle, (UIntPtr)(healthComponentAddress + MONSTER_CURRENT_HEALTH_OFFSET));
        float sizeScale = MemoryHelper.read_float(game.Handle, (UIntPtr)(monsterAddress + MONSTER_SIZE_SCALE_OFFSET));


        list.updateOrAdd(monsterAddress, id, maxHealth, currentHealth, sizeScale);
        
        //monster = OverlayViewModel.Instance.MonsterWidget.Context.UpdateAndGetMonster(monsterAddress, id, maxHealth, currentHealth, sizeScale);

        //UpdateMonsterParts(process, monster);
        //UpdateMonsterRemovableParts(process, monster);
        //UpdateMonsterStatusEffects(process, monster);

    } // todo overflow

    private static bool isMonsterIDValid(String id){
        string IncludeMonsterIdRegex = "em[0-9]";
        return new Regex(IncludeMonsterIdRegex).IsMatch(id);
}


    // converts a 4 byte array to an int with big endian byte order
    public static int dword_to_int(ref byte[] array) {
        return array[0] + (array[1] << 0x8) + (array[2] << 0x10) + (array[3] << 0x18);
    }

    public static int get_player_seat_id() {
        uint num = MemoryHelper.read_uint(game.Handle, (UIntPtr)names_base_address);
        uint num2 = MemoryHelper.read_uint(game.Handle, (UIntPtr)(num + 0x258));
        int result = -1;
        if (num2 > 0x1000) {
            uint num3 = MemoryHelper.read_uint(game.Handle, (UIntPtr)(long)(num2 + 0x10));
            if (num3 != 0) {
                result = MemoryHelper.read_int(game.Handle, (UIntPtr)(num3 + 0xBFEC));
            }
        }
        return result;
    }

    private static ulong getULongFromPointerChain(ulong[] chain) {
        byte[] mem_uint64 = new byte[8];
        ulong address;
        for (int i = 0; i < chain.Length; i++) {
            if (i == 0) {
                address = (ulong)game.MainModule.BaseAddress.ToInt64();
            } else {
                address = BitConverter.ToUInt64(mem_uint64, 0);
            }
            MemoryHelper.read_bytes(game.Handle, (UIntPtr)(address + chain[i]), mem_uint64);
        }
        return BitConverter.ToUInt64(mem_uint64, 0);
    }

    public struct MonsterInfo {

        public float hp;
        public float maxhp;
    }

    public static bool hasGameExited() {
        return game.HasExited;
    }
    //----------------------------------------------------------------------------
    public static readonly uint PLAYER_NAME_LENGHT = 0x21; 
    public static readonly ulong FIRST_PLAYER_NAME_OFFSET = 0x54A45;
    public static string get_team_player_name(int index) {
        ulong playerNamesPtr = loadEffectiveAddressRelative(game, player_names_root_offset);
        uint playerNamesAddress = MemoryHelper.read_uint(game.Handle, (UIntPtr)playerNamesPtr);
        ulong playerNameOffset = PLAYER_NAME_LENGHT * (ulong)index;
        string name = MemoryHelper.read_string(game.Handle, (UIntPtr)(playerNamesAddress + FIRST_PLAYER_NAME_OFFSET + playerNameOffset), PLAYER_NAME_LENGHT);
        return name;
    }
    public static ulong loadEffectiveAddressRelative(Process process, ulong address){
        const uint opcodeLength = 3;
        const uint paramLength = 4;
        const uint instructionLength = opcodeLength + paramLength;
        uint operand = MemoryHelper.read_uint(process.Handle, (UIntPtr)(address + opcodeLength));
        ulong operand64 = operand;
        // 64 bit relative addressing 
        if (operand64 > Int32.MaxValue) {
            operand64 = 0xffffffff00000000 | operand64;
        }
        return address + operand64 + instructionLength;
    }


    public static readonly ulong FIRST_STATS_PLAYER_OFFSET = 0x48;
    public static readonly ulong NEXT_STATS_PLAYER_OFFSET = 0x58;

    public static readonly ulong PLAYER_STATS_DAMAGE_OFFSET =   0x48;
    public static readonly ulong PLAYER_STATS_PARTS_OFFSET =    0x70;
    public static readonly ulong PLAYER_STATS_LOCATED_OFFSET =  0x90;
    public static readonly ulong PLAYER_STATS_TRACKS_OFFSET =   0x9c;
    public static readonly ulong PLAYER_STATS_SLINGERS_OFFSET = 0xa0;

    public static int[] get_player_data(int index) {
        int[] stats = new int[5];
        ulong playerDamageRootPtr = loadEffectiveAddressRelative(game, player_damages_root_offset);
        ulong playerDamageCollectionAddress = MemoryHelper.read_pointer_chain(game, playerDamageRootPtr, 0x48 + 0x20 * 0x58);
        ulong firstPlayerPtr = playerDamageCollectionAddress + FIRST_STATS_PLAYER_OFFSET;
        ulong currentPlayerPtr = firstPlayerPtr + ((ulong)index * NEXT_STATS_PLAYER_OFFSET);
        ulong currentPlayerAddress = MemoryHelper.read_ulong(game.Handle, (UIntPtr)currentPlayerPtr);
        int damage = MemoryHelper.read_int(game.Handle, (UIntPtr)(currentPlayerAddress + PLAYER_STATS_DAMAGE_OFFSET));
        int parts = MemoryHelper.read_int(game.Handle, (UIntPtr)(currentPlayerAddress + PLAYER_STATS_PARTS_OFFSET));
        int located = MemoryHelper.read_int(game.Handle, (UIntPtr)(currentPlayerAddress + PLAYER_STATS_LOCATED_OFFSET));
        int tracks = MemoryHelper.read_int(game.Handle, (UIntPtr)(currentPlayerAddress + PLAYER_STATS_TRACKS_OFFSET));
        int slingers = MemoryHelper.read_int(game.Handle, (UIntPtr)(currentPlayerAddress + PLAYER_STATS_SLINGERS_OFFSET));
        stats[(int)player_data_indices.damages] = damage;
        stats[(int)player_data_indices.parts] = parts;
        stats[(int)player_data_indices.located] = located;
        stats[(int)player_data_indices.tracks] = tracks;
        stats[(int)player_data_indices.slingers] = slingers;
        return stats;
    }
}

public enum player_data_indices {
   damages = 0,
   slingers = 1,
   located = 2,
   parts = 3,
   tracks = 4
}
