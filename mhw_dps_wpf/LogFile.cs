using System;
using System.IO;
using System.Collections.Generic;

namespace mhw_dps_wpf {
    public class LogFile {
        const string PATH_SEPERATOR = "\\";
        public const ushort FILE_VERSION = 7;

        private FileStream fs;
        private BinaryWriter bw;
        private int start_time;
        private Dictionary<string, int> player_indices;
        public LogFile() {
            ensurePathExists();
            string name = getSavePath() + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss") + ".mhlog";
            fs = new FileStream(name, FileMode.Create, FileAccess.Write);
            bw = new BinaryWriter(fs);
            player_indices = new Dictionary<string, int>();
        }

        public void writeHead() {
            bw.Write('M'); // marker for filetype
            bw.Write('H');
            bw.Write('W');
            bw.Write('L');
            bw.Write(FILE_VERSION);
            int headSize = 2 + 2 + 6; 
            bw.Write((uint)headSize); // head size
            writeMarker(Marker.UnixTime);
            start_time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            bw.Write(start_time); //care for the Y2K38 bug
            bw.Flush();
        }

        public void writeHit(String name, int hit) {
            if (!player_indices.ContainsKey(name)) {
                Console.WriteLine("new player found");
                player_indices.Add(name, player_indices.Count);
            }
            writeHit(player_indices[name], hit);
        }

        public void writeHit(int playerIndex, int hit) {
            writeMarker(Marker.PlayerHit);
            bw.Write((UInt16)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - start_time)); // time
            bw.Write((UInt16)playerIndex);
            bw.Write((UInt32)hit); 
        }

        public void writeMonsterHP(ulong unique_id, float hp) {
            writeMarker(Marker.MonsterHP);
            bw.Write((UInt16)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - start_time)); // time
            bw.Write((UInt64)unique_id);
            bw.Write((float)hp);
        }

        public void writeMonsterInfo(ulong unique_id,string name_id, float max_hp, float sizescale) {
            writeMarker(Marker.MonsterInfo);
            bw.Write((UInt64)unique_id);
            bw.Write(name_id);
            bw.Write(max_hp);
            bw.Write(sizescale);
        }

        public void writeBottomAndClose(PlayerList playerList) {
            writeMarker(Marker.LogEnd);
            int len = playerList.getPlayerNumber();
            writeMarker(Marker.PlayerList);
            for (int i = 0; i < len; i++) {
                writeMarker(Marker.PlayerIndex);
                bw.Write((UInt16)i); // player index
                bw.Write(playerList[i].name); // player name including size prefix
            }
            for (int i = 0; i < len; i++) {
                writeMarker(Marker.PlayerDamage);
                bw.Write((UInt16)i); // player index
                bw.Write((UInt32)playerList[i].damage); // player damage 
            }
            writeMarker(Marker.FileEnd);
            bw.Flush();
            fs.Flush();
            fs.Close();
        }

        private void writeMarker(Marker type) {
            switch(type) {
                case Marker.PlayerList:
                    bw.Write('P');
                    bw.Write('L'); // marker for log end
                    break;
                case Marker.PlayerIndex:
                    bw.Write('P');
                    bw.Write('I'); // marker for player index, name pair
                    break;
                case Marker.UnixTime:
                    bw.Write('U');
                    bw.Write('T'); // marker for timestamp
                    break;
                case Marker.PlayerHit:
                    bw.Write('P');
                    bw.Write('H'); // marker for a player hitting a monster
                    break;
                case Marker.LogEnd:
                    bw.Write('L');
                    bw.Write('E'); // marker for log end
                    break;
                case Marker.PlayerDamage:
                    bw.Write('P');
                    bw.Write('D'); // marker for total dmg
                    break;
                case Marker.FileEnd:
                    bw.Write('F');
                    bw.Write('E'); // marker for file end
                    break;
                case Marker.MonsterHP:
                    bw.Write('M');
                    bw.Write('H'); // marker for monster current hp
                    break;
                case Marker.MonsterInfo:
                    bw.Write('M');
                    bw.Write('I'); // marker for monster current hp
                    break;
                default:
                    bw.Write('?');
                    bw.Write('?'); // maker for internal error
                    Console.WriteLine("Tried to write unknown marker " + type);
                    break;
            }
        }
        
        private String getSavePath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + PATH_SEPERATOR + "MHW_Logs" + PATH_SEPERATOR;
        }

        private void ensurePathExists() {
            if (!Directory.Exists(getSavePath())) {
                Directory.CreateDirectory(getSavePath());
                Console.WriteLine("Log path (" + getSavePath() + ")created");
            }
        }

        enum Marker {
            PlayerList,
            PlayerIndex,
            UnixTime,
            PlayerHit,
            LogEnd,
            PlayerDamage,
            FileEnd,
            MonsterHP,
            MonsterInfo
        }
    }
}
