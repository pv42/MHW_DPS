using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mhw_dps_wpf{
    public class MonsterDataHelper {
        public enum MonsterSpezies {
            anjanath,
            azure_rathalos,
            barroth,
            bazelgeuse,
            behemoth,
            black_diablos,
            deviljho,
            diablos,
            dodogama,
            greatgirros,
            greatjagras,
            juratodus,
            kirin,
            kuluyaku,
            kulvetaroth,
            kushaladaora,
            legiana,
            lunastra,
            lavasioth,
            pink_rathian,
            nergigante,
            odogaron,
            palumu,
            pukeipukei,
            radobaan,
            rathalos,
            rathian,
            teostra,
            tobikadachi,
            tzitziyaku,
            uragaan,
            vaalhazak,
            xenojiva,
            zorahmagdaros,
            unknown
        };
        public enum CrownThreshholds {
            normal,
            reduced,
            none, // fixed size monsters
            error
        }
        private static readonly Dictionary<string, MonsterSpezies> monstersDict = new Dictionary<string, MonsterSpezies>() {
            {"em001_00",MonsterSpezies.rathian},
            {"em001_01",MonsterSpezies.pink_rathian},
            {"em002_00",MonsterSpezies.rathalos},
            {"em002_01",MonsterSpezies.azure_rathalos},
            {"em007_00",MonsterSpezies.diablos},
            {"em007_01",MonsterSpezies.black_diablos},
            {"em011_00",MonsterSpezies.kirin},
            {"em024_00",MonsterSpezies.kushaladaora},
            {"em026_00",MonsterSpezies.lunastra},
            {"em027_00",MonsterSpezies.teostra},
            {"em036_00",MonsterSpezies.lavasioth},
            {"em043_00",MonsterSpezies.deviljho},
            {"em044_00",MonsterSpezies.barroth},
            {"em045_00",MonsterSpezies.uragaan},
            {"em100_00", MonsterSpezies.anjanath},
            {"em101_00",MonsterSpezies.greatjagras},
            {"em102_00",MonsterSpezies.pukeipukei},
            {"em103_00",MonsterSpezies.nergigante},
            {"em105_00",MonsterSpezies.xenojiva},
            {"em106_00",MonsterSpezies.zorahmagdaros},
            {"em107_00",MonsterSpezies.kuluyaku},
            {"em108_00",MonsterSpezies.juratodus},
            {"em109_00",MonsterSpezies.tobikadachi},
            {"em110_00",MonsterSpezies.palumu},
            {"em111_00",MonsterSpezies.legiana},
            {"em112_00",MonsterSpezies.greatgirros},
            {"em113_00",MonsterSpezies.odogaron},
            {"em114_00",MonsterSpezies.radobaan},
            {"em115_00",MonsterSpezies.vaalhazak},
            {"em116_00",MonsterSpezies.dodogama},
            {"em117_00",MonsterSpezies.kulvetaroth},
            {"em118_00",MonsterSpezies.bazelgeuse},
            {"em120_00",MonsterSpezies.tzitziyaku},
            {"em121_00",MonsterSpezies.behemoth},
        };
        public static MonsterSpezies getSpeziesById(String id) {
            MonsterSpezies spez;
            if (monstersDict.TryGetValue(id, out spez)) {
                return spez;
            } else {
                return MonsterSpezies.unknown;
            }
        }
        public static string getMonsterName(MonsterSpezies spez) {
            switch (spez) {
                case MonsterSpezies.anjanath: return "Anjanath";
                case MonsterSpezies.azure_rathalos: return "Azure Rathalos";
                case MonsterSpezies.barroth: return "Barroth";
                case MonsterSpezies.bazelgeuse: return "Bazelgeuse";
                case MonsterSpezies.behemoth: return "Behemoth";
                case MonsterSpezies.black_diablos: return "Black Diablos";
                case MonsterSpezies.deviljho: return "Deviljho";
                case MonsterSpezies.diablos: return "Diablos";
                case MonsterSpezies.dodogama: return "Dodogama";
                case MonsterSpezies.greatgirros: return "Great Girros";
                case MonsterSpezies.greatjagras: return "Great Jagras";
                case MonsterSpezies.juratodus: return "Jyuratodus";
                case MonsterSpezies.kirin: return "Kirin";
                case MonsterSpezies.kuluyaku: return "Kulu-Ya-Ku";
                case MonsterSpezies.kulvetaroth: return "Kulve Taroth";
                case MonsterSpezies.kushaladaora: return "Kushala Daora";
                case MonsterSpezies.lavasioth: return "Lavasioth";
                case MonsterSpezies.legiana: return "Legiana";
                case MonsterSpezies.lunastra: return "Lunastra";
                case MonsterSpezies.nergigante: return "Nergigante";
                case MonsterSpezies.odogaron: return "Odogaron";
                case MonsterSpezies.palumu: return "Palumu";
                case MonsterSpezies.pink_rathian: return "Pink Rathian";
                case MonsterSpezies.pukeipukei: return "Pukei-Pukei";
                case MonsterSpezies.radobaan: return "Radobaan";
                case MonsterSpezies.rathalos: return "Rathalos";
                case MonsterSpezies.rathian: return "Rathian";
                case MonsterSpezies.teostra: return "Teostra";
                case MonsterSpezies.tobikadachi: return "Tobi-Kadachi";
                case MonsterSpezies.tzitziyaku: return "Tzitzi-Ya-Ku";
                case MonsterSpezies.uragaan: return "Úragaan";
                case MonsterSpezies.vaalhazak: return "Vaal Hazak";
                case MonsterSpezies.xenojiva: return "Xeno'jiva";
                case MonsterSpezies.zorahmagdaros: return "Zorah Magdaros";
                case MonsterSpezies.unknown: return "unknown monster";
                default: return "unknown spezies";
            }
        }
        public static CrownThreshholds getCrownThreshholds(MonsterSpezies spezies) {
            switch (spezies)
            {
                case MonsterSpezies.azure_rathalos:
                case MonsterSpezies.barroth:
                case MonsterSpezies.bazelgeuse:
                case MonsterSpezies.black_diablos:
                case MonsterSpezies.diablos:
                case MonsterSpezies.dodogama:
                case MonsterSpezies.greatgirros:
                case MonsterSpezies.greatjagras:
                case MonsterSpezies.juratodus:
                case MonsterSpezies.kirin:
                case MonsterSpezies.kuluyaku:
                case MonsterSpezies.kushaladaora:
                case MonsterSpezies.lavasioth:
                case MonsterSpezies.legiana:
                case MonsterSpezies.lunastra:
                case MonsterSpezies.nergigante:
                case MonsterSpezies.odogaron:
                case MonsterSpezies.palumu:
                case MonsterSpezies.pink_rathian:
                case MonsterSpezies.rathalos:
                case MonsterSpezies.rathian:
                case MonsterSpezies.teostra:
                case MonsterSpezies.tzitziyaku:
                case MonsterSpezies.vaalhazak: return CrownThreshholds.normal;
                case MonsterSpezies.anjanath:
                case MonsterSpezies.tobikadachi:
                case MonsterSpezies.radobaan:
                case MonsterSpezies.pukeipukei:
                case MonsterSpezies.uragaan:
                case MonsterSpezies.deviljho: return CrownThreshholds.reduced;
                case MonsterSpezies.behemoth:
                case MonsterSpezies.kulvetaroth:
                case MonsterSpezies.xenojiva:
                case MonsterSpezies.zorahmagdaros: return CrownThreshholds.none;
                case MonsterSpezies.unknown:
                default: return CrownThreshholds.error;
            }
        }
        public static SizeCrown getCrown(float size, CrownThreshholds t) {
            switch (t) {
                case CrownThreshholds.none:
                    return SizeCrown.none;
                case CrownThreshholds.reduced:
                    if (size < 0.9) return SizeCrown.small;
                    if (size > 1.2) return SizeCrown.big_gold;
                    if (size > 1.1) return SizeCrown.big_silver;
                    return SizeCrown.none;
                case CrownThreshholds.normal:
                    if (size < 0.9) return SizeCrown.small;
                    if (size > 1.23) return SizeCrown.big_gold;
                    if (size > 1.15) return SizeCrown.big_silver;
                    return SizeCrown.none;
                default:
                    return SizeCrown.none;
            }
        }
        public static SizeCrown getCrown(float size, MonsterSpezies s) {
            return getCrown(size, getCrownThreshholds(s));
        }
    }
}
