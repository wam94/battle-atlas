"""Project-owned officer's horse (Angle-v2 vocabulary, P5 — owner ruling
2026-07-15: "ship p5 mounted officers falling").

Modeled procedurally from public-domain reference proportions (no
external asset — the CC0/CC-BY-only rule): a ~15.2-hand bay saddle
horse, low/mid poly, rigid-skinned to a 13-bone FK armature. Local
frame matches the soldiers: faces -Y, up +Z, origin at the ground under
the barrel's center.

Clips (authored here, sampled in Unity by HorseClips.cs names):

  Horse_Stand   3.0 s loop   easy halt: head bob, tail sway, breathing
  Horse_Walk    1.6 s loop   4-beat walk (LH LF RH RF), head nod
  Horse_Rear    2.5 s        the hit: gathers, rears high (pivot at the
                             rear hooves), paws once, drops back
  Horse_Bolt    0.8 s loop   riderless bolt: gallop reach, neck flat,
                             tail streaming

Sober per violence-and-representation.md: the horse itself is never a
casualty here — it rears under the hit and leaves the field. No wound
staging, no fall.

Deterministic: constants only, no random anywhere.
"""
import math

import bmesh
import bpy
from mathutils import Matrix, Vector

from soldier_factory import flat_mat

D = math.radians
FPS = 24

# Proportions (meters). SADDLE_Z is shared with clips.py's rider seat
# (RIDER_SADDLE_Z = SADDLE_Z + 0.04 seat compression allowance).
WITHERS_Z = 1.52
BODY_Z = 1.14          # barrel center height
SADDLE_Z = 1.38
BODY_LEN = 1.70
FRONT_Y = -0.52        # front leg pair station
REAR_Y = 0.62          # rear leg pair station
LEG_X_F = 0.19
LEG_X_R = 0.21
KNEE_Z = 0.55
REAR_PIVOT = Vector((0.0, REAR_Y, 0.0))

LEGS = (
    ("fl", -LEG_X_F, FRONT_Y),
    ("fr", +LEG_X_F, FRONT_Y),
    ("rl", -LEG_X_R, REAR_Y),
    ("rr", +LEG_X_R, REAR_Y),
)


def frame(t):
    return int(round(t * FPS)) + 1


# ---------------------------------------------------------------- mesh

def _tube(bm, p0, p1, r0, r1, segments=10):
    """Tapered tube from p0 to p1 (world/armature space)."""
    p0 = Vector(p0)
    p1 = Vector(p1)
    d = p1 - p0
    ret = bmesh.ops.create_cone(bm, cap_ends=True, segments=segments,
                                radius1=r0, radius2=r1, depth=d.length)
    align = d.normalized().to_track_quat('Z', 'Y').to_matrix().to_4x4()
    mat = Matrix.Translation((p0 + p1) / 2) @ align
    bmesh.ops.transform(bm, matrix=mat, verts=ret["verts"])
    return ret


def _ball(bm, center, scale, u=12, v=8):
    ret = bmesh.ops.create_uvsphere(bm, u_segments=u, v_segments=v,
                                    radius=1.0)
    for vt in ret["verts"]:
        vt.co.x *= scale[0]
        vt.co.y *= scale[1]
        vt.co.z *= scale[2]
    bmesh.ops.translate(bm, verts=ret["verts"], vec=center)
    return ret


def _box(bm, size, center, rot_x=0.0):
    ret = bmesh.ops.create_cube(bm, size=1.0)
    for vt in ret["verts"]:
        vt.co.x *= size[0]
        vt.co.y *= size[1]
        vt.co.z *= size[2]
    if rot_x:
        bmesh.ops.rotate(bm, verts=ret["verts"], cent=(0, 0, 0),
                         matrix=Matrix.Rotation(D(rot_x), 3, 'X'))
    bmesh.ops.translate(bm, verts=ret["verts"], vec=center)
    return ret


def _part(name, build, mat, bone):
    """One rigid-skinned part: build(bm) fills a bmesh; the whole part
    weights to `bone`."""
    bm = bmesh.new()
    build(bm)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    ob.data.materials.append(mat)
    vg = ob.vertex_groups.new(name=bone)
    vg.add(range(len(ob.data.vertices)), 1.0, 'REPLACE')
    return ob


def build_horse(name="Horse"):
    """Returns (body_object, rig)."""
    bay = flat_mat("horse_bay", (0.230, 0.130, 0.070, 1.0), 0.6)
    dark = flat_mat("horse_dark", (0.055, 0.045, 0.040, 1.0), 0.6)
    leather = flat_mat("horse_leather", (0.160, 0.090, 0.050, 1.0), 0.5)
    blanket = flat_mat("horse_blanket", (0.300, 0.310, 0.360, 1.0), 0.7)

    # --- armature -----------------------------------------------------
    arm = bpy.data.armatures.new(f"{name}_arm")
    rig = bpy.data.objects.new(f"{name}_rig", arm)
    bpy.context.scene.collection.objects.link(rig)
    bpy.ops.object.select_all(action='DESELECT')
    rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode='EDIT')
    eb = arm.edit_bones

    def bone(nm, head, tail, parent=None):
        b = eb.new(nm)
        b.head = Vector(head)
        b.tail = Vector(tail)
        if parent:
            b.parent = eb[parent]
        return b

    bone("Root", (0, 0, 0), (0, 0.4, 0))
    bone("body", (0, 0.60, BODY_Z), (0, -0.60, BODY_Z + 0.06), "Root")
    bone("neck", (0, -0.62, 1.30), (0, -1.02, 1.66), "body")
    bone("head", (0, -1.02, 1.66), (0, -1.34, 1.50), "neck")
    bone("tail", (0, 0.98, 1.28), (0, 1.34, 0.90), "body")
    for nm, x, y in LEGS:
        bone(f"leg_{nm}_u", (x, y, 1.02), (x, y, KNEE_Z), "body")
        bone(f"leg_{nm}_l", (x, y, KNEE_Z), (x, y, 0.05), f"leg_{nm}_u")
    bpy.ops.object.mode_set(mode='OBJECT')

    # --- mesh parts (rigid per-bone weights) ---------------------------
    parts = []

    def torso(bm):
        _ball(bm, (0, 0.06, BODY_Z), (0.26, 0.80, 0.27))
        _ball(bm, (0, -0.46, BODY_Z + 0.05), (0.225, 0.34, 0.26))   # chest
        _ball(bm, (0, 0.64, BODY_Z + 0.06), (0.235, 0.36, 0.27))    # rump
        _ball(bm, (0, -0.54, WITHERS_Z - 0.14), (0.11, 0.20, 0.13))  # withers
    parts.append(_part("horse_torso", torso, bay, "body"))

    def neck(bm):
        _tube(bm, (0, -0.56, 1.22), (0, -0.98, 1.60), 0.16, 0.10)
    parts.append(_part("horse_neck", neck, bay, "neck"))

    def mane(bm):
        _box(bm, (0.020, 0.26, 0.10), (0, -0.76, 1.52), rot_x=-42.0)
        _box(bm, (0.020, 0.16, 0.08), (0, -0.95, 1.68), rot_x=-42.0)
    parts.append(_part("horse_mane", mane, dark, "neck"))

    def head(bm):
        # skull overlapping the neck end, muzzle running down-forward
        _ball(bm, (0, -1.02, 1.62), (0.085, 0.13, 0.11))
        _tube(bm, (0, -1.00, 1.64), (0, -1.24, 1.44), 0.085, 0.052)
        # ears rooted in the skull ball
        _tube(bm, (-0.04, -0.99, 1.68), (-0.06, -0.95, 1.80),
              0.024, 0.006, segments=6)
        _tube(bm, (0.04, -0.99, 1.68), (0.06, -0.95, 1.80),
              0.024, 0.006, segments=6)
    parts.append(_part("horse_head", head, bay, "head"))

    def muzzle(bm):
        _tube(bm, (0, -1.24, 1.44), (0, -1.33, 1.36), 0.052, 0.040,
              segments=8)
    parts.append(_part("horse_muzzle", muzzle, dark, "head"))

    def tail(bm):
        _tube(bm, (0, 0.90, 1.30), (0, 1.24, 0.78), 0.055, 0.014,
              segments=8)
    parts.append(_part("horse_tail", tail, dark, "tail"))

    for nm, x, y in LEGS:
        def upper(bm, x=x, y=y):
            _tube(bm, (x, y, 1.06), (x, y, KNEE_Z), 0.078, 0.048)
        def lower(bm, x=x, y=y):
            _tube(bm, (x, y, KNEE_Z), (x, y, 0.055), 0.043, 0.030)
        def hoof(bm, x=x, y=y):
            _box(bm, (0.048, 0.062, 0.052), (x, y - 0.008, 0.032))
        parts.append(_part(f"horse_leg_{nm}_u", upper, bay, f"leg_{nm}_u"))
        parts.append(_part(f"horse_leg_{nm}_l", lower, dark, f"leg_{nm}_l"))
        parts.append(_part(f"horse_hoof_{nm}", hoof, dark, f"leg_{nm}_l"))

    def saddle(bm):
        _box(bm, (0.30, 0.26, 0.045), (0, 0.02, SADDLE_Z + 0.045))
        _box(bm, (0.09, 0.05, 0.06), (0, -0.13, SADDLE_Z + 0.09))  # pommel
        _box(bm, (0.10, 0.05, 0.065), (0, 0.17, SADDLE_Z + 0.09))  # cantle
    parts.append(_part("horse_saddle", saddle, leather, "body"))

    def cloth(bm):
        _box(bm, (0.46, 0.34, 0.016), (0, 0.02, SADDLE_Z + 0.030))
        _box(bm, (0.026, 0.30, 0.18), (-0.265, 0.02, SADDLE_Z - 0.06))
        _box(bm, (0.026, 0.30, 0.18), (0.265, 0.02, SADDLE_Z - 0.06))
    parts.append(_part("horse_blanket", cloth, blanket, "body"))

    # join into one skinned object
    bpy.ops.object.select_all(action='DESELECT')
    for ob in parts:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    body = bpy.context.view_layer.objects.active
    body.name = name
    mod = body.modifiers.new("rig", 'ARMATURE')
    mod.object = rig
    body.parent = rig
    return body, rig


# ---------------------------------------------------------------- poser

class HorsePoser:
    """Minimal FK pose compiler on the horse rig — the same world-axis
    primitives and keying discipline as clips.Poser (absolute poses,
    euler continuity, action unassigned after every key)."""

    LOC_BONES = ("Root",)

    def __init__(self, rig):
        self.rig = rig
        self.pb = rig.pose.bones
        self._prev_euler = {}
        bpy.ops.object.select_all(action='DESELECT')
        rig.select_set(True)
        bpy.context.view_layer.objects.active = rig
        bpy.ops.object.mode_set(mode='POSE')
        for b in self.pb:
            b.rotation_mode = 'XYZ'
            b.lock_location = (False, False, False)
            b.lock_rotation = (False, False, False)
            b.lock_scale = (False, False, False)

    def reset(self):
        for b in self.pb:
            b.rotation_euler = (0.0, 0.0, 0.0)
            b.location = (0.0, 0.0, 0.0)
        self.rig.update_tag()
        self.upd()

    def upd(self):
        bpy.context.view_layer.update()

    def rot(self, bone, axis, deg):
        pb = self.pb[bone]
        ax = Vector({'X': (1, 0, 0), 'Y': (0, 1, 0), 'Z': (0, 0, 1)}[axis])
        h = pb.matrix.to_translation()
        R = Matrix.Rotation(D(deg), 4, ax)
        pb.matrix = (Matrix.Translation(h) @ R @
                     Matrix.Translation(-h) @ pb.matrix)
        self.upd()

    def root(self, loc=None, rot=None):
        pb = self.pb["Root"]
        m = self.rig.data.bones["Root"].matrix_local.copy()
        if rot:
            for axis, deg in rot:
                ax = Vector({'X': (1, 0, 0), 'Y': (0, 1, 0),
                             'Z': (0, 0, 1)}[axis])
                m = Matrix.Rotation(D(deg), 4, ax) @ m
        if loc is not None:
            for i in range(3):
                m[i][3] += loc[i]
        pb.matrix = m
        self.upd()

    def rear(self, deg):
        """Pitch the whole horse about the REAR-HOOF line (nose up for
        negative deg — the ground contact stays put)."""
        R = Matrix.Rotation(D(deg), 4, 'X')
        off = REAR_PIVOT - (R @ REAR_PIVOT)
        self.root(loc=tuple(off), rot=[('X', deg)])

    def key(self, action, t):
        if self.rig.animation_data is None:
            self.rig.animation_data_create()
        self.rig.animation_data.action = action
        f = frame(t)
        for b in self.pb:
            e = b.rotation_euler
            prev = self._prev_euler.get((action.name, b.name))
            if prev is not None:
                fixed = []
                for i in range(3):
                    v = e[i]
                    while v - prev[i] > math.pi:
                        v -= 2 * math.pi
                    while prev[i] - v > math.pi:
                        v += 2 * math.pi
                    fixed.append(v)
                b.rotation_euler = fixed
                e = b.rotation_euler
            self._prev_euler[(action.name, b.name)] = tuple(e)
            b.keyframe_insert("rotation_euler", frame=f)
            if b.name in self.LOC_BONES:
                b.keyframe_insert("location", frame=f)
        self.rig.animation_data.action = None


def _new_action(name):
    a = bpy.data.actions.get(name)
    if a is not None:
        bpy.data.actions.remove(a)
    a = bpy.data.actions.new(name)
    a.use_fake_user = True
    return a


# ---------------------------------------------------------------- clips

def horse_stand(P):
    """Easy halt: breathing, a head bob, the tail sways. 3 s loop."""
    a = _new_action("Horse_Stand")

    def pose(t, head_pitch, tail_yaw, breathe):
        P.reset()
        P.root(loc=(0, 0, -breathe))
        P.rot("neck", 'X', head_pitch * 0.4)
        P.rot("head", 'X', head_pitch * 0.6)
        P.rot("tail", 'Z', tail_yaw)
        P.key(a, t)

    pose(0.0, 0.0, 0.0, 0.0)
    pose(0.75, 3.5, 6.0, 0.006)
    pose(1.5, 0.5, 0.0, 0.010)
    pose(2.25, -2.5, -6.0, 0.004)
    pose(3.0, 0.0, 0.0, 0.0)
    return a


def horse_walk(P):
    """4-beat walk in place (the resolver's track carries the ground
    motion): LH, LF, RH, RF at quarter phases. 1.6 s loop."""
    a = _new_action("Horse_Walk")
    T = 1.6
    phases = {"rl": 0.0, "fl": 0.25, "rr": 0.5, "fr": 0.75}
    steps = 8
    for i in range(steps + 1):
        t = T * i / steps
        p = i / steps
        P.reset()
        P.root(loc=(0, 0, 0.012 * math.sin(4 * math.pi * p)))
        # head nods with the stride
        nod = 3.0 * math.sin(2 * math.pi * p)
        P.rot("neck", 'X', nod * 0.5)
        P.rot("head", 'X', nod * 0.5)
        P.rot("tail", 'Z', 4.0 * math.sin(2 * math.pi * p))
        for nm, ph in phases.items():
            s = math.sin(2 * math.pi * (p + ph))
            c = math.cos(2 * math.pi * (p + ph))
            P.rot(f"leg_{nm}_u", 'X', 16.0 * s)
            # the cannon folds on the swing-through, stays near-straight
            # in stance
            P.rot(f"leg_{nm}_l", 'X', -max(0.0, c) * 14.0)
        P.key(a, t)
    return a


def horse_rear(P):
    """The hit: gathers onto the haunches, rears (pivot at the rear
    hooves), paws once at the apex, drops back to the stand. 2.5 s."""
    a = _new_action("Horse_Rear")

    def pose(t, rear_deg, gather, fore_tuck, fore_paw, head_up, tail_up):
        P.reset()
        # one composed root op: pitch about the rear-hoof line (nose up)
        # plus the gather crouch — two root() calls would overwrite each
        # other (root composes from the rest matrix)
        R = Matrix.Rotation(D(-rear_deg), 4, 'X')
        off = REAR_PIVOT - (R @ REAR_PIVOT)
        P.root(loc=(0.0, gather * 0.10 + off.y, -gather * 0.08 + off.z),
               rot=[('X', -rear_deg)])
        if gather:
            for nm in ("rl", "rr"):
                P.rot(f"leg_{nm}_u", 'X', gather * 14.0)
                P.rot(f"leg_{nm}_l", 'X', -gather * 10.0)
        for i, nm in enumerate(("fl", "fr")):
            paw = fore_paw if i == 0 else -fore_paw * 0.4
            P.rot(f"leg_{nm}_u", 'X', -fore_tuck - paw)
            P.rot(f"leg_{nm}_l", 'X', fore_tuck * 1.15 + paw * 0.5)
        P.rot("neck", 'X', -head_up * 0.5)
        P.rot("head", 'X', -head_up * 0.5)
        P.rot("tail", 'X', -tail_up)
        P.key(a, t)

    pose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
    pose(0.4, 3.0, 1.0, 14.0, 0.0, 8.0, 6.0)      # gathers, head snaps up
    pose(0.9, 34.0, 0.0, 68.0, 0.0, 14.0, 18.0)   # full rear
    pose(1.2, 36.0, 0.0, 64.0, 10.0, 12.0, 18.0)  # paws at the apex
    pose(1.5, 28.0, 0.0, 62.0, -8.0, 10.0, 14.0)
    pose(2.1, 6.0, 0.5, 20.0, 0.0, 4.0, 6.0)      # comes down
    pose(2.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
    return a


def horse_bolt(P):
    """Riderless bolt: gallop reach in place, neck flat, tail streaming.
    0.8 s loop."""
    a = _new_action("Horse_Bolt")
    T = 0.8
    steps = 8
    for i in range(steps + 1):
        t = T * i / steps
        p = i / steps
        P.reset()
        s = math.sin(2 * math.pi * p)
        P.root(loc=(0, 0, 0.045 + 0.045 * s))
        P.rot("body", 'X', 5.0 * s)
        P.rot("neck", 'X', 14.0 + 3.0 * s)   # neck stretched flat
        P.rot("head", 'X', 10.0)
        P.rot("tail", 'X', -35.0 - 6.0 * s)  # streaming
        for nm, ph, amp in (("fl", 0.62, 36.0), ("fr", 0.72, 36.0),
                            ("rl", 0.10, 34.0), ("rr", 0.20, 34.0)):
            sw = math.sin(2 * math.pi * (p + ph))
            cw = math.cos(2 * math.pi * (p + ph))
            P.rot(f"leg_{nm}_u", 'X', amp * sw)
            P.rot(f"leg_{nm}_l", 'X', -max(0.0, cw) * 30.0)
        P.key(a, t)
    return a


def author_all(rig):
    P = HorsePoser(rig)
    made = []
    for fn in (horse_stand, horse_walk, horse_rear, horse_bolt):
        a = fn(P)
        made.append(a.name)
        print(f"[horse] authored {a.name}: "
              f"{a.frame_range[0]:.0f}..{a.frame_range[1]:.0f} @ {FPS}fps")
    P.reset()
    bpy.ops.object.mode_set(mode='OBJECT')
    return made


# ---------------------------------------------------------------- build

def previews(out, body, rig):
    """Workbench review sheets: 3 static views + per-clip contact
    frames (the P6 review-before-bake discipline)."""
    scene = bpy.context.scene
    for m in bpy.data.materials:
        if m.use_nodes and "Principled BSDF" in m.node_tree.nodes:
            m.diffuse_color = m.node_tree.nodes[
                "Principled BSDF"].inputs["Base Color"].default_value[:]
    cam_d = bpy.data.cameras.new("hc")
    cam = bpy.data.objects.new("hc", cam_d)
    scene.collection.objects.link(cam)
    cam_d.lens = 50
    scene.camera = cam
    if not any(o.type == 'LIGHT' for o in scene.objects):
        sd = bpy.data.lights.new("s", 'SUN')
        sun = bpy.data.objects.new("s", sd)
        scene.collection.objects.link(sun)
        sun.rotation_euler = (D(55), 0, D(-50))
        sd.energy = 4
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'MATERIAL'
    scene.render.resolution_x = 1100
    scene.render.resolution_y = 900

    # ground plane so contact reads in the review frames
    if "horse_ground" not in bpy.data.objects:
        gm = bpy.data.meshes.new("horse_ground")
        gm.from_pydata([(-6, -6, 0), (6, -6, 0), (6, 6, 0), (-6, 6, 0)],
                       [], [(0, 1, 2, 3)])
        go = bpy.data.objects.new("horse_ground", gm)
        scene.collection.objects.link(go)
        go.data.materials.append(
            flat_mat("horse_ground_mat", (0.35, 0.33, 0.28, 1.0), 0.9))

    def shoot(tag, loc, look=(0, 0, 1.0)):
        cam.location = loc
        d = Vector(look) - Vector(loc)
        cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = f"{out}/horse_{tag}.png"
        bpy.ops.render.render(write_still=True)

    if rig.animation_data:
        rig.animation_data.action = None
    scene.frame_set(1)
    shoot("side", (4.2, 0.2, 1.4))
    shoot("front34", (2.6, -3.4, 1.6))
    shoot("back34", (-2.8, 3.2, 1.7))
    for act in bpy.data.actions:
        if not act.name.startswith("Horse_"):
            continue
        rig.animation_data.action = act
        f0, f1 = act.frame_range
        for i, f in enumerate(sorted({int(f0), int(f0 + (f1 - f0) * 0.4),
                                      int(f0 + (f1 - f0) * 0.7), int(f1)})):
            scene.frame_set(f)
            shoot(f"clip_{act.name}_{i}_{f:03d}", (3.6, -2.6, 1.7))
    rig.animation_data.action = None
    bpy.data.objects.remove(cam, do_unlink=True)


def build_and_export(out, previews_only=False):
    body, rig = build_horse()
    author_all(rig)
    previews(out, body, rig)
    bpy.ops.wm.save_as_mainfile(filepath=f"{out}/horse.blend")
    if previews_only:
        return
    bpy.ops.object.select_all(action='DESELECT')
    body.select_set(True)
    rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    path = f"{out}/horse.fbx"
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        object_types={'ARMATURE', 'MESH'},
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=True,
        bake_anim_force_startend_keying=True,
        armature_nodetype='NULL',
    )
    print(f"[horse] exported {path}")
