using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mhw_dps_wpf {
    public class PlayerList {
        private Player[] array = new Player[4];

        public Player this[int i] {
            get {
                return array[i];
            }
        }

        public PlayerList(MainWindow window) {
            for (int i = 0; i < 4; i++) {
                array[i] = new Player(1, window);
                array[i].name = "";
            }
        }

        public int totalDamage() {
            int sum = 0;
            for(int i = 0; i < 4; i++) {
                sum += array[i].damage;
            }
            return sum;
        }

        public int maxDamage() {
            int max = Int32.MinValue;
            for (int i = 0; i < 4; i++) {
                max = max < array[i].damage ? array[i].damage : max;
            }
            return max;
        }

        public int getPlayerNumber() {
            int num = 0; 
            for(int i = 0; i < 4 ; i++) {
                if (!(array[i].name == "")) num++;
            }
            return num;
        }
    }
}
