using System;
using System.Collections.ObjectModel;

namespace mhw_dps_wpf {
    public enum SizeCrown {
        none,
        big_silver,
        big_gold,
        small
    }

    public class HealthInfo {
        public float health;
        public float max_heath;
        public float getPercent() {
            return health / max_heath;
        }
    }
    public class Monster {
        public string id;
        //public int index;
        public HealthInfo health;
        //public SizeCrown crown;
        public float size;
        //public Collection<MonsterPart> parts;
        public Monster(string id, float max_hp, float c_hp, float size){
            Console.WriteLine("id:" + id + " hp:" + c_hp + "/" + max_hp);
            this.id = id;
            this.health = new HealthInfo();
            this.health.max_heath = max_hp;
            this.health.health = c_hp;
            this.size = size;
        }
    }

    public class MonsterPart {
        public Monster owner;
        public bool isRemoveable;
        public HealthInfo health;
    }
}
