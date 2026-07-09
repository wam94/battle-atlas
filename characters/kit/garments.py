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

    def surface_front_y(self, x, z, fallback=-0.12):
        """Front (min y) of the body surface near column (x, z)."""
        ys = [v.co.y for v in self.body.data.vertices
              if abs(v.co.x - x) < 0.045 and abs(v.co.z - z) < 0.045]
        return min(ys) if ys else fallback


# ---------------------------------------------------------------- garments
def coat(body, rig, lm, pal, style, arm_limit):
    """Sack coat (Union, skirt to mid-thigh) or shell jacket (CSA, ends at
    the waist). arm_limit: fraction of arm covered (sleeves to wrists)."""
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

    ob = extract_band(body, f"{body.name}_coat", pred)
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
    top_z = lm.waist_z + 0.03
    bot_z = lm.ankle_z + 0.035

    def pred(c):
        return bot_z < c.z < top_z and abs(c.x) < 0.30

    ob = extract_band(body, f"{body.name}_trousers", pred)
    drape(ob, 0.010)
    solidify(ob, 0.005)
    add_armature(ob, rig)
    set_mat(ob, flat_mat(f"{pal['id']}_trouser_wool", pal["trousers"], 0.94))
    return ob


def brogans(body, rig, lm, pal):
    def pred(c):
        return c.z < lm.ankle_z + 0.055

    ob = extract_band(body, f"{body.name}_brogans", pred)
    drape(ob, 0.007, tangential_iters=1)
    solidify(ob, 0.004)
    add_armature(ob, rig)
    set_mat(ob, flat_mat(f"{pal['id']}_brogan_leather", pal["leather"], 0.55))
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
    """Forage cap: short tilted cylinder + shifted crown disk + visor."""
    top = Vector((0.0, -0.032, lm.head_top))
    r = 0.101
    bm = bmesh.new()
    # band (slightly conical, leaning forward)
    ret = bmesh.ops.create_cone(bm, cap_ends=False, segments=16,
                                radius1=r, radius2=r * 0.92, depth=0.075)
    bmesh.ops.translate(bm, verts=ret["verts"], vec=(0, 0, 0.0375))
    # crown disk shifted forward (kepi slouch)
    ret2 = bmesh.ops.create_circle(bm, cap_ends=True, segments=16, radius=r * 0.95)
    bmesh.ops.translate(bm, verts=ret2["verts"], vec=(0, -0.018, 0.075))
    bm.transform(Matrix.Translation(top - Vector((0, 0, 0.072))))
    ob = _primitive_to_object(bm, f"{body.name}_kepi")
    set_mat(ob, flat_mat(f"{pal['id']}_kepi_wool", pal["hat"], 0.92))
    _visor(ob, top, r, pal)
    rigid_skin(ob, rig, "head")
    return ob


def _visor(hat_ob, top, r, pal):
    bm = bmesh.new()
    ret = bmesh.ops.create_circle(bm, cap_ends=True, segments=12, radius=r * 0.98)
    # keep the front half, squash into a visor arc
    doomed = [v for v in bm.verts if v.co.y > 0.01]
    bmesh.ops.delete(bm, geom=doomed, context='VERTS')
    for v in bm.verts:
        v.co.y *= 1.9             # extend forward
        v.co.z = -0.014 * (abs(v.co.y) / (r * 1.9))  # slight droop
    bmesh.ops.translate(bm, verts=list(bm.verts),
                        vec=top + Vector((0, -0.018, -0.062)))
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
    """CSA slouch: wide brim + low dome."""
    top = Vector((0.0, -0.005, lm.head_top))
    bm = bmesh.new()
    # brim: flat ring
    brim = bmesh.ops.create_circle(bm, cap_ends=True, segments=20, radius=0.158)
    for v in bm.verts:
        d = Vector((v.co.x, v.co.y, 0)).length
        if d > 0.095:
            v.co.z -= 0.030 * ((d - 0.095) / 0.063)  # drooping brim
    # dome
    dome = bmesh.ops.create_uvsphere(bm, u_segments=14, v_segments=8, radius=0.100)
    for v in dome["verts"]:
        if v.co.z < 0:
            v.co.z *= 0.10
        else:
            v.co.z *= 0.85
    bmesh.ops.translate(bm, verts=dome["verts"], vec=(0, 0, 0.008))
    # brim rides the forehead line, not the crown
    bm.transform(Matrix.Translation(top - Vector((0, 0, 0.055))))
    ob = _primitive_to_object(bm, f"{body.name}_slouch")
    set_mat(ob, flat_mat(f"{pal['id']}_slouch_felt", pal["hat"], 0.95))
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
    ob = _box(f"{body.name}_cartbox", (0.09, 0.035, 0.065), "leather", pal)
    place(ob, (lm.waist_halfwidth * 0.55, lm.waist_back + 0.02, lm.waist_z - 0.02),
          (0, 0, -15))
    rigid_skin(ob, rig, "pelvis")
    return ob


def cap_pouch(body, rig, lm, pal):
    ob = _box(f"{body.name}_cappouch", (0.028, 0.02, 0.028), "leather", pal)
    place(ob, (lm.waist_halfwidth * 0.55, lm.waist_front - 0.005, lm.waist_z))
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
    ob = _box(f"{body.name}_haversack", (0.055, 0.11, 0.11), "canvas", pal)
    place(ob, (-(lm.waist_halfwidth + 0.045), 0.02, lm.waist_z - 0.10), (0, 8, 0))
    rigid_skin(ob, rig, "pelvis")
    return ob


def canteen(body, rig, lm, pal):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=14, v_segments=8, radius=0.085)
    for v in bm.verts:
        v.co.x *= 0.35
    ob = _primitive_to_object(bm, f"{body.name}_canteen")
    place(ob, (-(lm.waist_halfwidth + 0.085), 0.055, lm.waist_z - 0.06), (0, 6, 0))
    set_mat(ob, flat_mat(f"{pal['id']}_eq_canteen", pal["canteen"], 0.8))
    rigid_skin(ob, rig, "pelvis")
    return ob


def bayonet_scabbard(body, rig, lm, pal):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=8,
                          radius1=0.013, radius2=0.006, depth=0.50)
    ob = _primitive_to_object(bm, f"{body.name}_scabbard")
    place(ob, (-(lm.waist_halfwidth * 0.7), lm.waist_back - 0.01, lm.waist_z - 0.28),
          (12, 0, 8))
    set_mat(ob, flat_mat(f"{pal['id']}_eq_leather", pal["leather"], 0.55))
    rigid_skin(ob, rig, "pelvis")
    return ob


def strap(body, rig, lm, pal, name, from_shoulder, pal_key="leather", width=0.055):
    """Cross-body sling ribbon from one shoulder to the opposite hip.
    from_shoulder: 'l' or 'r'."""
    s = 1.0 if from_shoulder == 'l' else -1.0
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
    p0 = Vector((0.085, -0.005, lm.shoulder_z + 0.055))
    p2 = Vector((-(lm.waist_halfwidth + 0.02), 0.01, lm.waist_z - 0.06))
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
