﻿using System;
using System.IO;
using System.Collections.Generic;

namespace mhw_dps_wpf {
    public class LogFile {
        const String PATH_SEPERATOR = "\\";
        public const UInt16 FILE_VERSION = 5;

        private FileStream fs;
        private BinaryWriter bw;
        private Int32 start_time;
        private Dictionary<string, int> player_indices;
        public LogFile() {
            ensurePathExists();
            String name = getSavePath() + DateTime.Now.ToString("dd.MM.yyyy HH.mm.ss") + ".mhlog";
            fs = new FileStream(name, FileMode.Create, FileAccess.Write);
            bw = new BinaryWriter(fs);
            player_indices = new Dictionary<string, int>();
        }

        public void writeHead(PlayerList playerList) {
            bw.Write('M'); // marker for filetype
            bw.Write('H');
            bw.Write('W');
            bw.Write('L');
            bw.Write(FILE_VERSION);
            int len = playerList.getPlayerNumber();
            int headSize = 2 + 2 + 6; // len + UT marker + timestamp
            /*for (int i = 0; i < len; i++) {
                headSize += 4; // marker + index
                headSize += playerList[i].name.Length + 1; // player name including size prefix
                if (player_indices.ContainsKey(playerList[i].name)) {
                    Console.WriteLine("PlayerDuplicate name=" + playerList[i].name);
                } else {
                    player_indices.Add(playerList[i].name, i);
                }
            }*/
            bw.Write((UInt32)headSize); // head size
            writeMarker(Marker.UnixTime);
            start_time = (Int32)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
                    bw.Write('T');
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
                    bw.Write('P'); // marker for total dmg
                    bw.Write('D');
                    break;
                case Marker.FileEnd:
                    bw.Write('F');
                    bw.Write('E'); // marker for file end
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
                System.IO.Directory.CreateDirectory(getSavePath());
                Console.WriteLine("Log path (" + getSavePath() + ")created");
            }
        }

        public enum Marker {
               PlayerList,
               PlayerIndex,
               UnixTime,
               PlayerHit,
               LogEnd,
               PlayerDamage,
               FileEnd
        }
    }
}
