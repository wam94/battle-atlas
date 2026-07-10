"""Phase 6 character kit — project-owned garments and equipment (plan §7.1).

All geometry is scripted on top of the MPFB body/rig: garments are
extracted face bands with real tailoring (inflate, skirt flare, collar,
cuffs); equipment is primitive-built and rigid-skinned to bones. Restrained
historically reviewed palettes; Confederate is NOT random earth tones
(gray/butternut families only, documented in the kit note).
"""
import math

import bmesh
import bpy
from mathutils import Matrix, Vector

from soldier_factory import flat_mat

D = math.radians


# --------------------------------------------------------------- palettes
UNION = {
    "id": "union",
    "coat":      (0.058, 0.066, 0.118, 1.0),   # dark navy wool
    "trousers":  (0.265, 0.315, 0.425, 1.0),   # sky-blue kersey
    "hat":       (0.050, 0.058, 0.105, 1.0),   # navy kepi
    "visor":     (0.022, 0.020, 0.020, 1.0),
    "leather":   (0.030, 0.026, 0.024, 1.0),   # blackened leather
    "brogan":    (0.032, 0.028, 0.025, 1.0),   # blackened brogan leather
    "canvas":    (0.055, 0.052, 0.048, 1.0),   # tarred haversack
    "canteen":   (0.180, 0.210, 0.260, 1.0),   # sky-blue wool cover
    "brass":     (0.520, 0.400, 0.130, 1.0),
    "blanket":   (0.320, 0.300, 0.270, 1.0),
    "shirt":     (0.520, 0.480, 0.400, 1.0),
}
CSA = {
    "id": "csa",
    "coat":      (0.400, 0.390, 0.360, 1.0),   # cadet gray wool
    "coat_butternut": (0.430, 0.360, 0.250, 1.0),
    "trousers":  (0.360, 0.330, 0.280, 1.0),   # gray-brown jean cloth
    "hat":       (0.280, 0.240, 0.190, 1.0),   # brown felt slouch
    "visor":     (0.120, 0.100, 0.080, 1.0),
    "leather":   (0.280, 0.170, 0.090, 1.0),   # russet leather
    "brogan":    (0.150, 0.095, 0.050, 1.0),   # dark russet brogan (NOT flesh)
    "canvas":    (0.560, 0.530, 0.460, 1.0),   # plain canvas
    "canteen":   (0.340, 0.270, 0.180, 1.0),   # wood drum canteen
    "brass":     (0.500, 0.390, 0.140, 1.0),
    "blanket":   (0.380, 0.350, 0.300, 1.0),
    "shirt":     (0.500, 0.430, 0.330, 1.0),
}


# ---------------------------------------------------------- band extraction
def extract_band(body, name, predicate):
    """Duplicate the faces whose center satisfies predicate into a new
    object that inherits the body's deform vertex groups."""
    sel = {p.index for p in body.data.polygons if predicate(p.center)}
    bm = bmesh.new()
    bm.from_mesh(body.data)
    bm.faces.ensure_lookup_table()
    doomed = [f for f in bm.faces if f.index not in sel]
    bmesh.ops.delete(bm, geom=doomed, context='FACES')
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    for vg in body.vertex_groups:
        ob.vertex_groups.new(name=vg.name)
    return ob


def inflate(ob, dist):
    """Push vertices outward along their normals (applied, not a modifier,
    so later tailoring operates on final positions)."""
    me = ob.data
    normals = [v.normal.copy() for v in me.vertices]
    for v, n in zip(me.vertices, normals):
        v.co += n * dist


def push_off_body(ob, body, margin):
    """Exact anti-poke pass: push any vertex closer than `margin` to the
    body surface out along the body's surface normal (handles creases the
    normal-clamp in drape cannot see)."""
    me = ob.data
    for v in me.vertices:
        ok, loc, nrm, _ = body.closest_point_on_mesh(v.co, distance=margin * 4)
        if not ok:
            continue
        d = (v.co - loc).dot(nrm)
        if d < margin:
            v.co = loc + nrm * margin


def drape(ob, offset, tangential_iters=2):
    """Cloth-like offset: push along a SMOOTHED normal field (no shrink,
    softens anatomical detail), then tangentially relax with a clamp that
    keeps every vertex at least ~half the offset off the original skin
    (kills poke-through)."""
    me = ob.data
    bm = bmesh.new()
    bm.from_mesh(me)
    bm.verts.ensure_lookup_table()
    orig = [v.co.copy() for v in bm.verts]
    nrm = [v.normal.copy() for v in bm.verts]
    nbrs = [[e.other_vert(v).index for e in v.link_edges] for v in bm.verts]
    sn = [n.copy() for n in nrm]
    for _ in range(3):
        nxt = []
        for i in range(len(sn)):
            acc = sn[i].copy()
            for j in nbrs[i]:
                acc += sn[j]
            nxt.append(acc.normalized() if acc.length > 1e-8 else sn[i])
        sn = nxt
    pos = [orig[i] + sn[i] * offset for i in range(len(orig))]
    for _ in range(tangential_iters):
        nxt = []
        for i in range(len(pos)):
            if nbrs[i]:
                avg = Vector((0.0, 0.0, 0.0))
                for j in nbrs[i]:
                    avg += pos[j]
                avg /= len(nbrs[i])
                pcand = pos[i] * 0.45 + avg * 0.55
            else:
                pcand = pos[i]
            along = (pcand - orig[i]).dot(nrm[i])
            if along < offset * 0.55:
                pcand += nrm[i] * (offset * 0.55 - along)
            nxt.append(pcand)
        pos = nxt
    for i, v in enumerate(bm.verts):
        v.co = pos[i]
    bm.to_mesh(me)
    bm.free()


def smooth_region(ob, pred, radius=0.018, passes=2, weight=1.0):
    """Spatial low-pass over the vertices satisfying pred: each vertex
    moves toward the mean of ALL region vertices within `radius`
    (connectivity-independent, so it erases anatomical features smaller
    than the radius — toes — regardless of mesh density). `weight` may
    be a callable(co)->0..1 to taper toward the region boundary."""
    me = ob.data
    idx = [v.index for v in me.vertices if pred(v.co)]
    if not idx:
        return
    wfun = weight if callable(weight) else (lambda co: weight)
    pos = {i: me.vertices[i].co.copy() for i in idx}
    r2 = radius * radius
    for _ in range(passes):
        items = list(pos.items())
        nxt = {}
        for i, p in items:
            acc = Vector((0.0, 0.0, 0.0))
            wsum = 0.0
            for j, q in items:
                d2 = (p - q).length_squared
                if d2 < r2:
                    w = 1.0 - (d2 / r2) ** 0.5
                    acc += q * w
                    wsum += w
            nxt[i] = p.lerp(acc / wsum, wfun(p)) if wsum > 1e-8 else p
        pos = nxt
    for i in idx:
        me.vertices[i].co = pos[i]


def solidify(ob, thickness):
    mod = ob.modifiers.new("shell", 'SOLIDIFY')
    mod.thickness = thickness
    mod.offset = 1.0


def add_armature(ob, rig):
    mod = ob.modifiers.new("rig", 'ARMATURE')
    mod.object = rig
    ob.parent = rig


def rigid_skin(ob, rig, bone, weights=None):
    """Skin every vertex of ob 100% to one bone (or a dict of bone->weight)."""
    if weights is None:
        weights = {bone: 1.0}
    for b, w in weights.items():
        vg = ob.vertex_groups.get(b) or ob.vertex_groups.new(name=b)
        vg.add(range(len(ob.data.vertices)), w, 'REPLACE')
    add_armature(ob, rig)


def set_mat(ob, mat):
    ob.data.materials.clear()
    ob.data.materials.append(mat)


# --------------------------------------------------------------- landmarks
class Landmarks:
    def __init__(self, body, rig):
        self.body = body
        self.rig = rig
        b = rig.data.bones
        # character-right sign along X (MPFB game_engine: right hand is at
        # NEGATIVE x — measured, not assumed; equipment sides depend on it)
        self.rs = 1.0 if b["hand_r"].head_local.x > 0 else -1.0
        self.hip_z = b["pelvis"].head_local.z
        self.waist_z = b["spine_01"].head_local.z + 0.06
        self.chest_z = b["spine_03"].head_local.z
        self.neck_z = b["neck_01"].head_local.z
        self.head_top = max(v.co.z for v in body.data.vertices)
        self.shoulder_z = (b["clavicle_l"].tail_local.z + b["upperarm_l"].head_local.z) / 2
        self.wrist_l = b["hand_l"].head_local.copy()
        self.wrist_r = b["hand_r"].head_local.copy()
        self.ankle_z = b["foot_l"].head_local.z
        self.knee_z = b["calf_l"].head_local.z
        # torso half-width at waist for equipment placement
        band = [v.co for v in body.data.vertices
                if abs(v.co.z - self.waist_z) < 0.03 and abs(v.co.x) < 0.22]
        self.waist_halfwidth = max(abs(c.x) for c in band) if band else 0.16
        self.waist_front = min(c.y for c in band) if band else -0.12
        self.waist_back = max(c.y for c in band) if band else 0.12
        # scalp height at the head column (hats sit on this)
        crown = [v.co.z for v in body.data.vertices
                 if abs(v.co.x) < 0.07 and v.co.z > self.neck_z]
        self.head_top = max(crown) if crown else self.head_top
        # measured skull ellipse at the hat-band line (Gate P8 review fix:
        # hats were sized by CONSTANTS, so wider/deeper heads poked through
        # the dome and the brim read as a detached halo ring at hero
        # distance). zb is the brim plane, just above brow/ears; the
        # ellipse is measured over everything the hat must enclose.
        zb = self.head_top - 0.058
        skull = [v.co for v in body.data.vertices
                 if zb - 0.004 < v.co.z <= self.head_top + 0.001
                 and abs(v.co.x) < 0.12]
        if skull:
            hx = max(abs(c.x) for c in skull)
            ymin = min(c.y for c in skull)
            ymax = max(c.y for c in skull)
        else:  # defensive fallback, never expected on the MPFB body
            hx, ymin, ymax = 0.085, -0.115, 0.095
        self.head_ellipse = (hx, ymin, ymax, zb)

    def surface_front_y(self, x, z, fallback=-0.12):
        """Front (min y) of the body surface near column (x, z)."""
        ys = [v.co.y for v in self.body.data.vertices
              if abs(v.co.x - x) < 0.045 and abs(v.co.z - z) < 0.045]
        return min(ys) if ys else fallback


# ---------------------------------------------------------------- garments
# Garment coverage predicates are module-level so mask_covered_body can
# re-evaluate EXACTLY the regions each garment was extracted from.
def coat_pred(lm, style):
    hem_z = lm.hip_z - 0.16 if style == "sack" else lm.waist_z - 0.01
    collar_z = lm.neck_z + 0.012

    def pred(c):
        ax = abs(c.x)
        if c.z < hem_z:
            return False
        # exclude the neck/head column only; the coat covers the traps
        if c.z >= collar_z and ax < 0.09:
            return False
        if c.z >= collar_z + 0.10:
            return False
        on_torso = ax <= lm.waist_halfwidth + 0.12
        # sleeves down to just below the wrist line
        on_arm = ax > lm.waist_halfwidth - 0.02 and c.z > lm.wrist_l.z - 0.06
        # exclude hands: beyond the wrist along x
        beyond_hand = ax > abs(lm.wrist_l.x) + 0.012
        return (on_torso or on_arm) and not beyond_hand

    return pred


def trousers_pred(lm):
    top_z = lm.waist_z + 0.03
    bot_z = lm.ankle_z + 0.035

    def pred(c):
        return bot_z < c.z < top_z and abs(c.x) < 0.30

    return pred


def brogans_pred(lm):
    def pred(c):
        return c.z < lm.ankle_z + 0.075

    return pred


def mask_covered_body(body, lm, style, inset=0.022):
    """Gate P6 defect-1 fix ('the nude is sticking through the uniform'):
    permanently DELETE the body faces the coat/trousers/brogans cover.

    Garment vertices inherit the deform weights of the body vertices they
    were extracted from, so a garment tracks its source skin in every
    pose; but at weight-blend joints (elbows, knees, shoulders — worst in
    the reload arm raises) NEIGHBORING body vertices deform differently
    and can poke through the offset shell. Faces that can never be seen
    cannot poke. `inset` shrinks every region by sampling the predicate
    at +-inset in x and z, keeping a safety strip of skin under each
    garment opening (collar, cuffs, waist, ankles) so no hole shows."""
    preds = (coat_pred(lm, style), trousers_pred(lm), brogans_pred(lm))
    offs = [Vector((dx, 0.0, dz))
            for dx in (-inset, 0.0, inset) for dz in (-inset, 0.0, inset)]
    collar_z = lm.neck_z + 0.012

    def covered(c):
        # keep a chest/upper-back plug under the coat's neck opening:
        # in deep bends (fence crossing, reload) the camera sees down
        # the collar, and without this the deleted skin reads as a
        # black void inside the coat
        if c.z > collar_z - 0.09 and abs(c.x) < 0.13:
            return False
        return any(all(p(c + o) for o in offs) for p in preds)

    doomed_idx = {p.index for p in body.data.polygons if covered(p.center)}
    bm = bmesh.new()
    bm.from_mesh(body.data)
    bm.faces.ensure_lookup_table()
    doomed = [f for f in bm.faces if f.index in doomed_idx]
    bmesh.ops.delete(bm, geom=doomed, context='FACES')
    # drop the verts orphaned by the face deletion
    lone = [v for v in bm.verts if not v.link_faces]
    if lone:
        bmesh.ops.delete(bm, geom=lone, context='VERTS')
    bm.to_mesh(body.data)
    bm.free()
    print(f"[garments] masked {len(doomed_idx)} covered body faces "
          f"({len(body.data.polygons)} visible remain)")
    return len(doomed_idx)


def collar_shirt(body, lm, pal):
    """Color the kept chest/upper-back plug as shirt cloth: the V under
    the coat's neck opening reads as the period shirt, not bare skin.
    Call AFTER the skin material is assigned (slot 0)."""
    mat = flat_mat(f"{pal['id']}_shirt", pal["shirt"], 0.9)
    body.data.materials.append(mat)
    collar_z = lm.neck_z + 0.012
    n = 0
    for p in body.data.polygons:
        c = p.center
        if collar_z - 0.10 < c.z <= collar_z + 0.005 and abs(c.x) < 0.135:
            p.material_index = 1
            n += 1
    print(f"[garments] collar plug: {n} faces dressed as shirt")


def coat(body, rig, lm, pal, style, arm_limit):
    """Sack coat (Union, skirt to mid-thigh) or shell jacket (CSA, ends at
    the waist). arm_limit: fraction of arm covered (sleeves to wrists)."""
    hem_z = lm.hip_z - 0.16 if style == "sack" else lm.waist_z - 0.01
    ob = extract_band(body, f"{body.name}_coat", coat_pred(lm, style))
    drape(ob, 0.016, tangential_iters=3)
    push_off_body(ob, body, 0.009)

    # skirt flare: verts below the hip flare outward and hang looser
    if style == "sack":
        top = lm.hip_z + 0.05
        for v in ob.data.vertices:
            if v.co.z < top:
                t = min(1.0, (top - v.co.z) / max(0.001, top - hem_z))
                r = Vector((v.co.x, v.co.y - 0.01, 0.0))
                if r.length > 1e-4:
                    v.co += r.normalized() * (0.028 * t)
    solidify(ob, 0.006)
    add_armature(ob, rig)
    set_mat(ob, flat_mat(f"{pal['id']}_{style}_wool", pal["coat"], 0.94))
    return ob


def trousers(body, rig, lm, pal):
    ob = extract_band(body, f"{body.name}_trousers", trousers_pred(lm))
    drape(ob, 0.010)
    solidify(ob, 0.005)
    add_armature(ob, rig)
    set_mat(ob, flat_mat(f"{pal['id']}_trouser_wool", pal["trousers"], 0.94))
    return ob


def brogans(body, rig, lm, pal):
    """Ankle boot with an actual BOOT silhouette (P8 review fix — the old
    skin-tight extraction read as bare feet at hero distance): leather
    bulk over an ankle-height shaft, a flared sole rim, and a welted
    sole slab + heel block per foot, in a leather tone distinct from
    flesh (pal['brogan'])."""
    ob = extract_band(body, f"{body.name}_brogans", brogans_pred(lm))
    drape(ob, 0.011, tangential_iters=4)
    me = ob.data
    floor = min(v.co.z for v in me.vertices)
    sole_top = floor + 0.014

    # melt the toes into a smooth leather toe box (front 55% of each
    # foot, below the ankle line)
    ys = [v.co.y for v in me.vertices if v.co.z < lm.ankle_z]
    toe_y = min(ys) + 0.56 * (max(ys) - min(ys))
    smooth_region(
        ob, lambda c: c.z < lm.ankle_z and c.y < toe_y,
        radius=0.020, passes=2,
        weight=lambda co: min(1.0, (toe_y - co.y) / 0.02))

    # per-foot horizontal centers (feet are the two x-sign clusters)
    def foot_bounds(sign):
        vs = [v.co for v in me.vertices if (v.co.x > 0) == (sign > 0)]
        xs = [c.x for c in vs]
        ys = [c.y for c in vs]
        return (min(xs), max(xs), min(ys), max(ys))

    # flare the lowest band outward into a sole rim
    for v in me.vertices:
        if v.co.z < sole_top:
            x0, x1, y0, y1 = foot_bounds(1.0 if v.co.x > 0 else -1.0)
            center = Vector(((x0 + x1) / 2, (y0 + y1) / 2, 0.0))
            out = Vector((v.co.x, v.co.y, 0.0)) - center
            if out.length > 1e-5:
                k = 1.0 - (v.co.z - floor) / 0.014
                v.co += out.normalized() * (0.007 * k)

    # sole slab + heel block boxes, rigid to each foot bone
    b = lm.rig.data.bones
    boxes = []
    for bone in ("foot_l", "foot_r"):
        sign = 1.0 if b[bone].head_local.x > 0 else -1.0
        x0, x1, y0, y1 = foot_bounds(sign)
        cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
        hx, hy = (x1 - x0) / 2, (y1 - y0) / 2
        sole = _box(f"{body.name}_sole_{bone}",
                    (hx + 0.011, hy + 0.013, 0.008), "brogan", pal)
        place(sole, (cx, cy, floor + 0.006))
        # heel block under the rear of the foot (toes point -y)
        heel = _box(f"{body.name}_heel_{bone}",
                    (hx + 0.009, hy * 0.34, 0.012), "brogan", pal)
        place(heel, (cx, y1 - hy * 0.30, floor + 0.012))
        for bx in (sole, heel):
            vg = bx.vertex_groups.new(name=bone)
            vg.add(range(len(bx.data.vertices)), 1.0, 'REPLACE')
            boxes.append(bx)

    solidify(ob, 0.004)
    add_armature(ob, rig)
    set_mat(ob, flat_mat(f"{pal['id']}_brogan_leather", pal["brogan"], 0.55))
    # merge the sole/heel boxes (join unifies vertex groups by name and
    # keeps the active object's armature modifier)
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    for bx in boxes:
        bx.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.join()
    return ob


# -------------------------------------------------------------- headgear
def _primitive_to_object(bm, name):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    return ob


def kepi(body, rig, lm, pal):
    """Forage cap: elliptical band fit to the MEASURED skull (P8 review
    fix — the old constant-radius band was narrower than most heads, so
    the skull swallowed it and the crown read as a floating disk), with a
    forward-tilted crown disk and a leather visor."""
    hx, ymin, ymax, zb = lm.head_ellipse
    yc = (ymin + ymax) / 2
    rx = hx + 0.012
    ry = (ymax - ymin) / 2 + 0.012
    crown_z = lm.head_top + 0.022          # crown clears the scalp
    band_h = crown_z - zb
    bm = bmesh.new()
    # band: elliptical cone, slightly narrower at the crown
    ret = bmesh.ops.create_cone(bm, cap_ends=False, segments=18,
                                radius1=1.0, radius2=0.93, depth=1.0)
    for v in bm.verts:
        t = v.co.z + 0.5                   # 0 bottom .. 1 top
        v.co = Vector((v.co.x * rx, yc + v.co.y * ry - 0.014 * t,
                       zb + t * band_h - 0.008 * t))  # lean/tilt forward
    # crown disk shifted forward (kepi slouch)
    ret2 = bmesh.ops.create_circle(bm, cap_ends=True, segments=18, radius=1.0)
    for v in ret2["verts"]:
        v.co = Vector((v.co.x * rx * 0.93, yc + v.co.y * ry * 0.93 - 0.014,
                       crown_z - 0.008))
    ob = _primitive_to_object(bm, f"{body.name}_kepi")
    set_mat(ob, flat_mat(f"{pal['id']}_kepi_wool", pal["hat"], 0.92))
    _visor(ob, lm, pal)
    rigid_skin(ob, rig, "head")
    return ob


def _visor(hat_ob, lm, pal):
    """Leather visor as an explicit quad-strip arc across the band's
    front (P8 review fix: the old delete-half-the-circle trick destroyed
    the single n-gon face, so kepis shipped with NO visor at all)."""
    hx, ymin, ymax, zb = lm.head_ellipse
    yc = (ymin + ymax) / 2
    rx = hx + 0.012
    ry = (ymax - ymin) / 2 + 0.012
    bm = bmesh.new()
    n = 10
    prev = None
    for i in range(n + 1):
        k = 2.0 * i / n - 1.0              # -1 .. 1 across the arc
        t = math.radians(-90.0 + 72.0 * k) # front = -y
        reach = math.cos(k * math.pi / 2)  # peaks at the front center
        ext = 0.055 * reach
        inner = Vector((rx * 0.99 * math.cos(t),
                        yc + ry * 0.99 * math.sin(t), zb + 0.004))
        outer = Vector(((rx + ext) * math.cos(t),
                        yc + (ry + ext) * math.sin(t),
                        zb + 0.004 - 0.018 * reach))
        v1 = bm.verts.new(inner)
        v2 = bm.verts.new(outer)
        if prev:
            bm.faces.new((prev[0], prev[1], v2, v1))
        prev = (v1, v2)
    me = bpy.data.meshes.new("visor")
    bm.to_mesh(me)
    bm.free()
    vo = bpy.data.objects.new("visor", me)
    bpy.context.scene.collection.objects.link(vo)
    vo.data.materials.append(flat_mat(f"{pal['id']}_visor_leather", pal["visor"], 0.45))
    # merge into the hat object
    bpy.ops.object.select_all(action='DESELECT')
    hat_ob.select_set(True)
    vo.select_set(True)
    bpy.context.view_layer.objects.active = hat_ob
    bpy.ops.object.join()


def slouch_hat(body, rig, lm, pal):
    """Slouch: wide drooping brim + felt dome, both fit to the MEASURED
    skull ellipse (P8 review fix — the old constant-radius sphere was
    smaller than most skulls; the scalp poked through it and the brim
    read as a detached halo ring at hero distance)."""
    hx, ymin, ymax, zb = lm.head_ellipse
    yc = (ymin + ymax) / 2
    rx = hx + 0.013
    ry = (ymax - ymin) / 2 + 0.013
    rmax = max(rx, ry)
    dome_h = (lm.head_top - zb) + 0.026
    bm = bmesh.new()
    # brim: disc drooping outside the skull line
    brim_outer = rmax + 0.048
    brim = bmesh.ops.create_circle(bm, cap_ends=True, segments=20, radius=brim_outer)
    for v in brim["verts"]:
        d = Vector((v.co.x, v.co.y, 0)).length
        if d > rmax + 0.002:
            v.co.z -= 0.034 * ((d - rmax - 0.002) /
                               max(0.001, brim_outer - rmax - 0.002))
    # dome: measured-ellipse felt crown; the squashed lower hemisphere is
    # a short skirt below the brim plane so no scalp strip can show
    dome = bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=9, radius=1.0)
    for v in dome["verts"]:
        v.co.x *= rx
        v.co.y *= ry
        v.co.z = v.co.z * dome_h if v.co.z > 0 else v.co.z * 0.014
    bmesh.ops.translate(bm, verts=dome["verts"], vec=(0, 0, 0.004))
    bm.transform(Matrix.Translation(Vector((0.0, yc, zb))))
    ob = _primitive_to_object(bm, f"{body.name}_slouch")
    set_mat(ob, flat_mat(f"{pal['id']}_slouch_felt", pal["hat"], 0.95))
    solidify(ob, 0.004)
    rigid_skin(ob, rig, "head")
    return ob


# -------------------------------------------------------------- equipment
def _box(name, size, pal_key, pal, bevel=0.0):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co.x *= size[0]
        v.co.y *= size[1]
        v.co.z *= size[2]
    ob = _primitive_to_object(bm, name)
    set_mat(ob, flat_mat(f"{pal['id']}_eq_{pal_key}", pal[pal_key], 0.6))
    return ob


def place(ob, loc, rot_euler_deg=(0, 0, 0)):
    ob.location = loc
    ob.rotation_euler = tuple(D(a) for a in rot_euler_deg)
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)


def cartridge_box(body, rig, lm, pal):
    # worn on the RIGHT hip, rear (sling from the left shoulder)
    ob = _box(f"{body.name}_cartbox", (0.09, 0.035, 0.065), "leather", pal)
    place(ob, (lm.rs * lm.waist_halfwidth * 0.55, lm.waist_back + 0.02,
               lm.waist_z - 0.02), (0, 0, -15))
    rigid_skin(ob, rig, "pelvis")
    return ob


def cap_pouch(body, rig, lm, pal):
    # on the belt, right of the plate
    ob = _box(f"{body.name}_cappouch", (0.028, 0.02, 0.028), "leather", pal)
    place(ob, (lm.rs * lm.waist_halfwidth * 0.55, lm.waist_front - 0.005,
               lm.waist_z))
    rigid_skin(ob, rig, "pelvis")
    return ob


def belt(body, rig, lm, pal):
    """Waist belt band + brass plate. Follows the coat silhouette."""
    bm = bmesh.new()
    ret = bmesh.ops.create_cone(bm, cap_ends=False, segments=18,
                                radius1=1.0, radius2=1.0, depth=0.045)
    ob = _primitive_to_object(bm, f"{body.name}_belt")
    # fit ellipse to waist over coat
    ob.scale = (lm.waist_halfwidth + 0.030, (lm.waist_back - lm.waist_front) / 2 + 0.030, 1.0)
    place(ob, (0, (lm.waist_back + lm.waist_front) / 2, lm.waist_z - 0.005))
    set_mat(ob, flat_mat(f"{pal['id']}_eq_leather", pal["leather"], 0.6))
    plate = _box("beltplate", (0.030, 0.008, 0.022), "brass", pal)
    place(plate, (0, lm.waist_front - 0.028, lm.waist_z - 0.005))
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    plate.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.join()
    rigid_skin(ob, rig, "spine_01")
    return ob


def haversack(body, rig, lm, pal):
    # LEFT hip
    ob = _box(f"{body.name}_haversack", (0.055, 0.11, 0.11), "canvas", pal)
    place(ob, (-lm.rs * (lm.waist_halfwidth + 0.045), 0.02, lm.waist_z - 0.10),
          (0, 8, 0))
    rigid_skin(ob, rig, "pelvis")
    return ob


def canteen(body, rig, lm, pal):
    # LEFT hip, over the haversack
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=14, v_segments=8, radius=0.085)
    for v in bm.verts:
        v.co.x *= 0.35
    ob = _primitive_to_object(bm, f"{body.name}_canteen")
    place(ob, (-lm.rs * (lm.waist_halfwidth + 0.085), 0.055, lm.waist_z - 0.06),
          (0, 6, 0))
    set_mat(ob, flat_mat(f"{pal['id']}_eq_canteen", pal["canteen"], 0.8))
    rigid_skin(ob, rig, "pelvis")
    return ob


def bayonet_scabbard(body, rig, lm, pal):
    # LEFT hip, rear
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=8,
                          radius1=0.013, radius2=0.006, depth=0.50)
    ob = _primitive_to_object(bm, f"{body.name}_scabbard")
    place(ob, (-lm.rs * (lm.waist_halfwidth * 0.7), lm.waist_back - 0.01,
               lm.waist_z - 0.28), (12, 0, 8))
    set_mat(ob, flat_mat(f"{pal['id']}_eq_leather", pal["leather"], 0.55))
    rigid_skin(ob, rig, "pelvis")
    return ob


def strap(body, rig, lm, pal, name, from_shoulder, pal_key="leather", width=0.055):
    """Cross-body sling ribbon from one shoulder to the opposite hip.
    from_shoulder: 'l' or 'r'."""
    s = -lm.rs if from_shoulder == 'l' else lm.rs
    sh = Vector((s * 0.062, -0.01, lm.shoulder_z + 0.045))
    hip = Vector((-s * (lm.waist_halfwidth * 0.8), 0.0, lm.waist_z - 0.05))
    # waypoints hug the torso: front panel down across the chest
    front = lm.waist_front
    pts = []
    n = 9
    for i in range(n + 1):
        t = i / n
        p = sh.lerp(hip, t)
        # follow the actual torso front surface, offset for coat + air
        p.y = lm.surface_front_y(p.x, p.z, fallback=front) - 0.030
        pts.append(p)
    bm = bmesh.new()
    prev = None
    half = width / 2
    for p in pts:
        v1 = bm.verts.new(p + Vector((-half * 0.7, 0, half * 0.4)))
        v2 = bm.verts.new(p + Vector((half * 0.7, 0, -half * 0.4)))
        if prev:
            bm.faces.new((prev[0], prev[1], v2, v1))
        prev = (v1, v2)
    ob = _primitive_to_object(bm, f"{body.name}_{name}")
    set_mat(ob, flat_mat(f"{pal['id']}_eq_{pal_key}", pal[pal_key], 0.6))
    rigid_skin(ob, rig, None, weights={"spine_03": 0.5, "spine_02": 0.5})
    solidify(ob, 0.004)
    return ob


def blanket_roll(body, rig, lm, pal):
    """CSA bedroll: torus arc over the left shoulder to the right hip."""
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=10,
                          radius1=0.052, radius2=0.052, depth=1.0)
    # length subdivisions so the bezier bend below has verts to bend
    long_edges = [e for e in bm.edges
                  if abs(e.verts[0].co.z - e.verts[1].co.z) > 0.1]
    bmesh.ops.subdivide_edges(bm, edges=long_edges, cuts=8)
    ob = _primitive_to_object(bm, f"{body.name}_bedroll")
    me = ob.data
    # left shoulder -> right hip
    p0 = Vector((-lm.rs * 0.085, -0.005, lm.shoulder_z + 0.055))
    p2 = Vector((lm.rs * (lm.waist_halfwidth + 0.02), 0.01, lm.waist_z - 0.06))
    mid_z = (p0.z + p2.z) / 2
    # the bezier midpoint gets only half of p1's pull: solve p1.y so the
    # curve midpoint sits a roll-radius clear of the chest surface
    mid_target = lm.surface_front_y(-0.02, mid_z) - 0.052 - 0.022
    p1 = Vector((-0.02, (mid_target - 0.25 * (p0.y + p2.y)) / 0.5, mid_z))
    for v in me.vertices:
        t = v.co.z + 0.5          # 0..1 along the roll
        q0 = p0.lerp(p1, t)
        q1 = p1.lerp(p2, t)
        p = q0.lerp(q1, t)        # quadratic bezier
        tangent = (q1 - q0).normalized()
        side = tangent.cross(Vector((0, -1, 0))).normalized()
        outn = side.cross(tangent).normalized()
        me.vertices[v.index].co = p + side * v.co.x + outn * v.co.y
    set_mat(ob, flat_mat(f"{pal['id']}_bedroll_wool", pal["blanket"], 0.95))
    rigid_skin(ob, rig, None, weights={"spine_03": 0.6, "spine_02": 0.4})
    return ob
