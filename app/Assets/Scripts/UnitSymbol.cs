using UnityEngine;

namespace BattleAtlas
{
    // The documentary-cartography symbol grammar (atlas-cartography plan,
    // D2): a unit symbol's SHAPE encodes arm of service, its BORDER encodes
    // echelon, its THICKNESS encodes strength — the attested frontage stays
    // the symbol's length, exactly as authored — and its FILL encodes
    // provenance confidence. Pure statics: the taxonomy that must never lie
    // is EditMode-testable without a scene.
    public static class UnitSymbol
    {
        // Shape classes — type reads as SHAPE, not as a shade of one box:
        // infantry the solid bar, cavalry the sheared parallelogram (the
        // map-symbol slash made structural), artillery a row of gun-dots,
        // and ArtilleryPark the honest non-combat symbol for the Artillery
        // Reserve park/trains — hollow outline + inset cross, permanently
        // muted, never readable as a firing line.
        public enum SymbolKind { Infantry, Cavalry, Artillery, ArtilleryPark }

        // Border grammar: brigade = double border (outer heavy + inner
        // pinstripe), regiment = single thin border, battery = the gun-dot
        // grammar itself IS the echelon mark (no bar = no confusion with a
        // regiment), park = the muted outline.
        public enum Echelon { Brigade, Regiment, Battery, Park }

        // Fill provenance. Contested is a RESERVED third slot (cross-hatch)
        // for Phase 11 proper's angle-bundle vocabulary — no macro-format
        // confidence value maps to it here (see StyleOf).
        public enum FillStyle { Solid, Hatched, Contested }

        // Strength -> thickness: area-proportional to effectives wherever
        // the clamp doesn't bite, so a 300-man regiment is visibly a sliver
        // next to a 1,700-man brigade and a brigade THINS as its keyframed
        // strength falls through the charge. ~6 m² of map per effective.
        public const float AreaPerEffectiveM2 = 6f;
        public const float MinDepthM = 8f;   // a sliver still reads at 4 km
        public const float MaxDepthM = 48f;  // a massed column never blots the map
        // thickness quantum: interpolating strength must not dirty the
        // symbol mesh every frame — depth moves in half-meter steps
        public const float DepthQuantumM = 0.5f;

        // gun-dots per battery: ≈ guns from effectives (a Napoleon crew plus
        // drivers runs ~20 men), deterministic from strength alone
        public const float MenPerGunDot = 20f;
        public const int MinGunDots = 2;
        public const int MaxGunDots = 8;

        // Arm of service from the unit id — the LOCKED id-prefix convention
        // (docs/superpowers/plans/2026-07-02-full-cast.md "Id conventions"),
        // PLUS the park split the symbol system needs: batteries `us-btty-*`
        // / `csa-btty-*` and CSA artillery battalions `csa-bn-*` are
        // Artillery (the bn- prefix exists precisely to dodge the
        // csa-garnett INFANTRY brigade), the Artillery Reserve park
        // `us-arty-*` is ArtilleryPark (non-combat trains, not a firing
        // line), cavalry `us-cav-*` — everything else is infantry.
        // Prefixes, never substrings: csa-5al-bn (5th Alabama Battalion) is
        // an infantry battalion and must stay infantry. A format-level
        // `kind` field is a DEFERRED format decision — until it lands, the
        // ids are the contract and this is its ONE decoder
        // (BattleDirector.KindOf delegates here).
        public static SymbolKind KindOf(string unitId)
        {
            if (unitId.StartsWith("us-arty-", System.StringComparison.Ordinal))
                return SymbolKind.ArtilleryPark;
            if (unitId.StartsWith("us-btty-", System.StringComparison.Ordinal)
                || unitId.StartsWith("csa-btty-", System.StringComparison.Ordinal)
                || unitId.StartsWith("csa-bn-", System.StringComparison.Ordinal))
                return SymbolKind.Artillery;
            if (unitId.StartsWith("us-cav-", System.StringComparison.Ordinal))
                return SymbolKind.Cavalry;
            return SymbolKind.Infantry;
        }

        // Echelon from what the data attests: the kinds carry their own
        // echelon grammar (gun-dots ARE the battery mark, the park outline
        // IS the park mark); a decomposed child (has `parent`) is a
        // regiment; a roster, children, or the parentless default at macro
        // grain all read brigade. Adapted from the military I/II/X notch
        // convention to what actually survives 4 km of zoom — border weight
        // and symbol class do, notch ticks don't (plan D2).
        public static Echelon EchelonOf(
            bool hasRoster, bool hasChildren, bool hasParent, SymbolKind kind)
        {
            if (kind == SymbolKind.ArtilleryPark) return Echelon.Park;
            if (kind == SymbolKind.Artillery) return Echelon.Battery;
            if (hasParent) return Echelon.Regiment;
            return Echelon.Brigade;
        }

        // The symbol's thickness in display meters. Frontage is DATA
        // (frontage_m, cited) and never moves; strength becomes depth via
        // the area rule, clamped for legibility and quantized so strength
        // interpolation crosses a rebuild threshold only every half meter.
        // The loader guarantees frontage_m > 0.
        public static float DisplayDepth(float strength, float frontage)
        {
            float raw = strength * AreaPerEffectiveM2 / frontage;
            float clamped = Mathf.Clamp(raw, MinDepthM, MaxDepthM);
            return Mathf.Round(clamped / DepthQuantumM) * DepthQuantumM;
        }

        // How many gun-dots an artillery symbol draws along its frontage.
        public static int GunDotCount(float strength) =>
            Mathf.Clamp(Mathf.RoundToInt(strength / MenPerGunDot),
                MinGunDots, MaxGunDots);

        // USER RULING 2026-07-09 — TWO confidence states render: documented
        // = solid fill, inferred = diagonal-hatched fill, and the format's
        // "unknown" (including the empty default) RENDERS AS inferred —
        // "anything unknown can be considered inferred for this project."
        // The format vocabulary itself is unchanged; this is the ONE place
        // the ruling maps words to styles. FillStyle.Contested stays
        // reserved for Phase 11 proper — nothing here returns it.
        public static FillStyle StyleOf(string confidence) =>
            confidence == "documented" ? FillStyle.Solid : FillStyle.Hatched;
    }
}
