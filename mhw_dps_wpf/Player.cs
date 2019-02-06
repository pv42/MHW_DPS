using System;
using System.Collections.Generic;

namespace mhw_dps_wpf {
    public class Player {
        bool _valid;
        string _name;
        public bool isValid {
            get {
                return _valid;
            }
        }
        public string name {
            get {
                return _name;
            }
            set {
                _name = value;
                _valid = (_name.Length > 0); 
            }
        }
        int _damage;
        int _slingers;
        int _parts_broken;
        int _located;
        int _tracks;
        MainWindow _window;
        bool initialized = false;
        LinkedList<Hit> hitlist = new LinkedList<Hit>();

        public int damage {
            get {
                return _damage;
            }
            set {
                if (_damage > value || !initialized) { 
                    init();
                }
                if (_damage != value && initialized && !name.Equals("")) {
                    Hit hit = new Hit(value - _damage, time());
                    _window.log(name + " hit for " + hit.damage);
                    _window.logFile.writeHit(name, hit.damage);
                    hitlist.AddFirst(hit);
                }
                _damage = value;
            }
        }

        public int slingers {
            get {
                return _slingers;
            }
            set {
                if (_slingers > value || !initialized) {
                    init();  
                } 
                if (_slingers != value && initialized && !name.Equals("")) {
                    if (value - _slingers == 1) {
                        _window.log(name + " hit a slinger shot");
                    } else { 
                        _window.log(name + " hit a slinger shot (" + (value - _slingers) + " hits)");
                    }
                }
                _slingers = value;
            }
        }

        public int parts_broken {
            get {
                return _parts_broken;
            }
            set {
                if (_parts_broken > value || !initialized) {
                    init();
                }
                if (_parts_broken != value && initialized && !name.Equals("")) {
                    if (value - _parts_broken == 1) {
                        _window.log(name + " broke a part");
                    } else {
                        _window.log(name + " broke multiple parts!");
                    }
                }
                _parts_broken = value;
            }
        }

        public int tracks_collected {
            get {
                return _tracks;
            }
            set {
                if (_tracks > value || !initialized) {
                    init();
                }
                if (_tracks != value && initialized && !name.Equals("")) {
                    if (value - _tracks == 1) {
                        _window.log(name + " collected a track");
                    } else {
                        _window.log(name + " collected multiple tracks!");
                    }
                }
                _tracks = value;
            }
        }

        public int monsters_located {
            get {
                return _located;
            }
            set {
                if (_located > value || !initialized) {
                    init();
                }
                if (_located != value && initialized && !name.Equals("")) {
                    if (value - _located == 1) {
                        _window.log(name + " located a monster");
                    } else {
                        _window.log(name + " located multiple monsters!");
                    }
                }
                _located = value;
            }
        }


        public Player(int damage, MainWindow window) {
            _window = window;
            _damage = damage;
        }

        void init() {
            initialized = true;
            hitlist.Clear();
            _damage = 0;
            _slingers = 0;
            _located = 0;
            _parts_broken = 0;
            _tracks = 0;
            Console.WriteLine("Reseted player " + name);
            _window.log("reseted player " + name);
            if(_window.logFile == null) {
                _window.logFile = new LogFile();
                _window.logFile.writeHead(_window.players);
            }

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
        
        public void update(int global_index) {
            name = mhw.get_team_player_name(global_index);
            if (!isValid) return;
            int [] data = mhw.get_player_data(global_index);
            damage = data[(int)player_data_indices.damages];
            slingers = data[(int)player_data_indices.slingers];
            monsters_located = data[(int)player_data_indices.located];
            parts_broken = data[(int)player_data_indices.parts];
            tracks_collected = data[(int)player_data_indices.tracks];
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
