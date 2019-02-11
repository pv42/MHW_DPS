using System;

namespace mhw_dps_wpf {
    public class PlayerList {
        private Player[] players = new Player[4];

        public Player this[int i] {
            get {
                return players[i];
            }
        }

        public PlayerList(MainWindow window) {
            for (int i = 0; i < 4; i++) {
                players[i] = new Player(0, window);
                players[i].name = "";
            }
        }

        public int totalDamage() {
            int sum = 0;
            foreach(Player player in players) {
                if (player.isValid) {
                    sum += player.damage;
                }
            }
            return sum;
        }

        public int maxDamage() {
            int max = Int32.MinValue;
            for (int i = 0; i < 4; i++) {
                max = max < players[i].damage ? players[i].damage : max;
            }
            return max;
        }

        public int getPlayerNumber() {
            int num = 0; 
            for(int i = 0; i < 4 ; i++) {
                if (players[i].isValid) num++;
            }
            return num;
        }

        public void update() {
            for (int i = 0; i < 4; i++) {
                this[i].update(i);
            }
        }
    }
}
