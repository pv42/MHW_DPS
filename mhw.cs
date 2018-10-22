using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public static class mhw
{
	public static bool is_wegame_build = false;

	public static long loc1 = -1L;

	public static long damage_base_loc0 = -1L;

	public static long damage_base_loc1 = -1L;

	public static long names_base_adress = -1L;

	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

	private static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer) {
		int lpNumberOfBytesRead = 0;
		return ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, lpBuffer.Length, ref lpNumberOfBytesRead);
	}

	public static ulong read_ulong(IntPtr hProcess, IntPtr lpBaseAddress) {
		byte[] array = new byte[8];
		ReadProcessMemory(hProcess, lpBaseAddress, array);
		return BitConverter.ToUInt64(array, 0);
	}

	public static uint read_uint(IntPtr hProcess, IntPtr lpBaseAddress) {
		byte[] array = new byte[4];
		ReadProcessMemory(hProcess, lpBaseAddress, array);
		return BitConverter.ToUInt32(array, 0);
	}

	public static int dword_to_int(ref byte[] array) {
		return array[0] + (array[1] << 8) + (array[2] << 16) + (array[3] << 24);
	}

    // reads at loc1, retuns 0x48 + param 2 + 0x58 * read_mem & param3 
	private static ulong asm_func1(Process proc, ulong rcx, uint edx) {
		uint num = read_uint(proc.Handle, (IntPtr)loc1);
		ulong num2 = rcx;
		rcx = (ulong)((long)(num & edx) * 88L);
		return num2 + 72 + rcx;
	}

	public static int[][] get_team_data(Process proc) {
        int[][] data = new int[5][];
        data[(int)data_indices.damages] = new int[4]; // damage
        data[(int)data_indices.slingers] = new int[4]; // slingers
        data[(int)data_indices.located] = new int[4]; // monsters located
        data[(int)data_indices.parts] = new int[4]; // parts
        data[(int)data_indices.tracks] = new int[4]; // tracks
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
                    read_data_set(proc, num3, mem_uint32, data, 0, data_indices.damages, i);
                    read_data_set(proc, num3, mem_uint32, data, 0x28, data_indices.parts, i);
                    read_data_set(proc, num3, mem_uint32, data, 0x48, data_indices.located, i);
                    read_data_set(proc, num3, mem_uint32, data, 0x54, data_indices.tracks, i);
                    read_data_set(proc, num3, mem_uint32, data, 0x58, data_indices.slingers, i);
                }
            }
		}
		return data;
	}

    private static void read_data_set(Process proc, ulong num3, byte[] mem_uint32, int[][] data, ulong address_offset, data_indices data_index, int player_index) {
        bool num4 = ReadProcessMemory(proc.Handle, (IntPtr)(long)(num3 + 72 + address_offset), mem_uint32);
        int player_data = dword_to_int(ref mem_uint32);
        if (num4 && player_data >= 0 && player_data <= 0xFFFFF) {
            data[(int)data_index][player_index] = player_data;
        }
    }

	public static int get_player_seat_id(Process proc) {
		uint num = read_uint(proc.Handle, (IntPtr)names_base_adress);
		uint num2 = read_uint(proc.Handle, (IntPtr)(num + (is_wegame_build ? 1376 : 600)));
		int result = -1;
		if (num2 > 4096)
		{
			uint num3 = read_uint(proc.Handle, (IntPtr)(long)(num2 + 16));
			if (num3 != 0)
			{
				result = (int)read_uint(proc.Handle, (IntPtr)(num3 + (is_wegame_build ? 49396 : 49132)));
			}
		}
		return result;
	}

	public static string[] get_team_player_names(Process proc) {
		string[] names = new string[4];
		byte[] mem_name_string = new byte[40];
		int num = (int)read_uint(proc.Handle, (IntPtr)names_base_adress) + (is_wegame_build ? 349621 : 346693);
		for (int i = 0; i < 4; i++){
			Array.Resize(ref mem_name_string, 40);
			ReadProcessMemory(proc.Handle, (IntPtr)(num + 33 * i), mem_name_string);
			int name_len = Array.FindIndex(mem_name_string, (byte x) => x == 0);
			if (name_len == 0 || name_len == -1) {
				names[i] = "";
			} else {
				Array.Resize(ref mem_name_string, name_len);
				names[i] = Encoding.UTF8.GetString(mem_name_string);
			}
		}
		return names;
	}
}
public enum data_indices {
   damages = 0,
   slingers = 1,
   located = 2,
   parts = 3,
   tracks = 4
}
