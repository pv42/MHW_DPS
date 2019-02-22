using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace mhw_dps_wpf {
    public enum SizeCrown {
        none,
        big_silver,
        big_gold,
        small
    }

    public class HealthInfo {
        public float current;
        public float max;
        public float getPercent() {
            return current / max * 100;
        }
    }
    public class Monster {
        public ulong memory_address;
        public string id;
        public MonsterDataHelper.MonsterSpezies spezies;
        //public int index;
        public HealthInfo health;
        public SizeCrown crown;
        public float size;
        public bool valid;
        public string name {
            get {
                return MonsterDataHelper.getMonsterName(spezies);
            }
        }
        private MainWindow window;
        //public Collection<MonsterPart> parts;
        public Monster(ulong address, string id, float max_hp, float c_hp, float size, MainWindow window) {
            this.memory_address = address;
            this.id = id;
            this.health = new HealthInfo();
            this.health.max = max_hp;
            this.health.current = c_hp;
            this.size = size;
            this.window = window;
            spezies = MonsterDataHelper.getSpeziesById(id);
            crown = MonsterDataHelper.getCrown(size, spezies);
            Console.WriteLine("" + name + " hp:" + c_hp + "/" + max_hp + " size: " + size + " c: " + crown.ToString());
            valid = true;
        }
        public void update(string id, float max_hp, float c_hp, float size) {
            if(c_hp != health.current){
                window.logFile.writeMonsterHP(memory_address, c_hp);
                if(c_hp == 0){
                    window.log("Monster slayed");
                }
            }
            this.health.current = c_hp;
            valid = true;
            if (this.size != size || this.health.max != max_hp || this.id != id){
                this.size = size;
                crown = MonsterDataHelper.getCrown(size, spezies);
                this.health.max = max_hp;
                this.id = id;
                Console.WriteLine("MONSTER info changed for " + id + " mhp: " + max_hp);
                window.logFile.writeMonsterInfo(memory_address, id, max_hp, size);
            }
        }
    }

    public class MonsterPart {
        public Monster owner;
        public bool isRemoveable;
        public HealthInfo health;
    }

    public class MonsterList {
        private MainWindow window;
        private Dictionary<ulong, Monster> _monsters;
        public MonsterList(MainWindow window) {
            this.window = window;
            _monsters = new Dictionary<ulong, Monster>();
        }
        public void updateOrAdd(ulong address, string id, float max_hp, float c_hp, float size) {
            if(_monsters.ContainsKey(address)) {
                _monsters[address].update(id, max_hp, c_hp, size);
            } else {
                _monsters.Add(address, new Monster(address, id, max_hp, c_hp, size, window));
                window.logFile.writeMonsterInfo(address,id,max_hp,size);
                window.logFile.writeMonsterHP(address, c_hp);
            }
            
        }
        public void update() {
            mhw.getMonsterInfo(this);
            List<ulong> toRemove = new List<ulong>();
            foreach (KeyValuePair<ulong,Monster> kvp in _monsters) {
                if (!kvp.Value.valid){
                    Console.WriteLine("Monster " + kvp.Value.id + " removed");
                    toRemove.Add(kvp.Key);
                }
                kvp.Value.valid = false;
            }
            foreach(ulong addr in toRemove){
                _monsters.Remove(addr);
            }
        }
    }
}
