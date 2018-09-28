using System;
using System.Collections;
using System.Collections.Generic;

namespace mhw_dps_wpf {
    class Player {
        public string name { get; set; }
        int _damage;
        MainWindow _window;
        bool initialized = false;
        LinkedList<Hit> hitlist = new LinkedList<Hit>();

        public int damage {
            get {
                return _damage;
            }
            set {
                if (_damage > value) { // i am almost sure it should be the other way around but whatever
                    hitlist.Clear();  // reset
                    Console.WriteLine("Reseted hit history");
                    _window.log("reseted player " + name);

                } else {
                    if (_damage != value && initialized && !name.Equals("")) {
                        _window.log(name + " hit for " + (value - _damage));
                        hitlist.AddFirst(new Hit(value - _damage, time()));
                    }
                }
                
                initialized = true;
                _damage = value;
            }
        }

        public Player(int damage, MainWindow window) {
            _window = window;
            _damage = damage;
        }

        private static double time() => (DateTime.UtcNow - DateTime.MinValue).TotalSeconds;

        public double getLastDPS(double seconds) {
            LinkedListNode<Hit> hit = hitlist.First;
            if(hit == null) return 0;
            int dmg = 0;
            int i = 0;
            Hit last = hit.Value;
            while (hit != null && (time() - hit.Value.time < seconds)) {
                dmg += hit.Value.damage;
                last = hit.Value;
                hit = hit.Next;
                i++;
            }
            //Console.WriteLine("dmg " + dmg + " i" + i + " dt" + (time() - last.time));
            return ((double)dmg) / (time() - last.time + 1);
        }

    }
    class Hit {
        public int damage;
        public double time;
        public Hit(int damage, double time) {
            this.damage = damage;
            this.time = time;
        }
    }
}
