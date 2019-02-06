
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
        public string name;
        public int index;
        public HealthInfo info;
        public SizeCrown crown;
        public float size;
        public Collection<MonsterPart> parts;
    }

    public class MonsterPart {
        public Monster owner;
        public bool isRemoveable;
        public HealthInfo health;
    }
}
