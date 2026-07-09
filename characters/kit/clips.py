"""Phase 6 character kit — animation vocabulary (plan §7.3).

Every clip is a bpy Action authored on the MPFB game_engine rig by a small
pose compiler:

  * world-space primitives (`rot` about a world axis at the bone head,
    `aim` a bone's Y axis at a world direction) so nothing depends on the
    rig's undocumented bone rolls;
  * temporary-IK limb solves (the spike's proven method, ik_solve.py)
    against wrist/ankle targets, applied via visual_transform_apply;
  * the musket/ramrod as pose-driven prop bones — hand targets are derived
    from the musket matrix, so grips stay glued through every stage.

Deterministic: no random anywhere; every pose is rebuilt from rest.

The reload cycle is the historically ordered percussion rifle-musket
sequence (hero requirement, plan §7.3): LOAD (butt to ground) -> HANDLE
CARTRIDGE (from cartridge box) -> TEAR CARTRIDGE (teeth) -> CHARGE
CARTRIDGE (pour at muzzle) -> DRAW RAMMER -> RAM CARTRIDGE (two strokes)
-> RETURN RAMMER -> PRIME (half-cock, cap from cap pouch onto the nipple)
-> SHOULDER. Stage timing constants below are exported for the Unity
choreography tests.
"""
import math

import bpy
from mathutils import Euler, Matrix, Vector

import musket as mk

D = math.radians
FPS = 24

TORSO = ("pelvis", "spine_01", "spine_02", "spine_03", "neck_01", "head")
SPINE = ("spine_01", "spine_02", "spine_03")
ARM_L = ("clavicle_l", "upperarm_l", "lowerarm_l", "hand_l")
ARM_R = ("clavicle_r", "upperarm_r", "lowerarm_r", "hand_r")
LEG_L = ("thigh_l", "calf_l", "foot_l", "ball_l")
LEG_R = ("thigh_r", "calf_r", "foot_r", "ball_r")
FINGERS = tuple(f"{f}_{i:02d}_{s}" for f in
                ("thumb", "index", "middle", "ring", "pinky")
                for i in (1, 2, 3) for s in ("l", "r"))
PROPS = ("prop_musket", "prop_ramrod")
LOC_BONES = ("Root", "pelvis", "prop_musket", "prop_ramrod")

# Reload stage boundaries in seconds (Unity choreography mirrors these).
RELOAD_STAGES = (
    ("load",             0.0),
    ("handle_cartridge", 1.5),
    ("tear_cartridge",   4.5),
    ("charge_cartridge", 6.0),
    ("draw_rammer",      8.5),
    ("ram_cartridge",   11.0),
    ("return_rammer",   13.5),
    ("prime",           16.0),
    ("shoulder",        18.0),
)
RELOAD_SECONDS = 20.0


def frame(t):
    return int(round(t * FPS)) + 1


class Poser:
    """Pose compiler on the game_engine rig. All inputs in armature space
    (character faces -Y, up +Z)."""

    def __init__(self, rig, lm):
        self.rig = rig
        self.lm = lm
        self.pb = rig.pose.bones
        # character-right sign along X (MPFB: right hand at negative or
        # positive X depending on export — measure, don't assume)
        self.rs = 1.0 if rig.data.bones["hand_r"].head_local.x > 0 else -1.0
        self._prev_euler = {}
        self._ik = []       # (bone, constraint, target_empty, pole_empty)
        bpy.ops.object.select_all(action='DESELECT')
        rig.select_set(True)
        bpy.context.view_layer.objects.active = rig
        bpy.ops.object.mode_set(mode='POSE')
        for b in self.pb:
            b.rotation_mode = 'XYZ'
            # MPFB ships transform locks on some bones (Root X/Y rotation);
            # the pose compiler owns every channel, so clear them all
            b.lock_location = (False, False, False)
            b.lock_rotation = (False, False, False)
            b.lock_rotation_w = False
            b.lock_scale = (False, False, False)
        self._calibrate_fingers()

    # ---------------------------------------------------------- primitives
    def reset(self):
        self._clear_ik()
        for b in self.pb:
            b.rotation_euler = (0.0, 0.0, 0.0)
            b.location = (0.0, 0.0, 0.0)
        self.upd()

    def upd(self):
        bpy.context.view_layer.update()

    def head(self, bone):
        return (self.rig.matrix_world @ self.pb[bone].matrix).to_translation()

    def rot(self, bone, axis, deg):
        """Rotate the bone's current pose about a WORLD axis through its
        head. axis: 'X'|'Y'|'Z' or Vector."""
        pb = self.pb[bone]
        ax = Vector({'X': (1, 0, 0), 'Y': (0, 1, 0), 'Z': (0, 0, 1)}[axis]
                    if isinstance(axis, str) else axis)
        h = pb.matrix.to_translation()
        R = Matrix.Rotation(D(deg), 4, ax)
        pb.matrix = Matrix.Translation(h) @ R @ Matrix.Translation(-h) @ pb.matrix
        self.upd()

    def lean(self, deg):
        """Forward lean distributed over the spine (forward = -Y; positive
        world-X rotation tips +Z toward -Y)."""
        for b in SPINE:
            self.rot(b, 'X', deg / 3.0)

    def twist(self, deg, bones=SPINE):
        for b in bones:
            self.rot(b, 'Z', deg / len(bones))

    def look(self, yaw=0.0, pitch=0.0, tilt=0.0):
        """Head/neck: yaw about Z (+ = character-left), pitch about X
        (+ = chin down), tilt about Y."""
        for b, w in (("neck_01", 0.4), ("head", 0.6)):
            if yaw:
                self.rot(b, 'Z', yaw * w)
            if pitch:
                self.rot(b, 'X', pitch * w)
            if tilt:
                self.rot(b, 'Y', tilt * w)

    def root(self, loc=None, rot=None):
        """Root bone: gross body placement. rot = (axis, deg) pairs applied
        in order about world axes at the ROOT head (origin, at the feet —
        the right pivot for falls)."""
        pb = self.pb["Root"]
        # compose in Python and assign ONCE: pb.matrix reads are cached by
        # the depsgraph, so read-assign-read-assign loses the first write
        m = pb.matrix.copy()
        if rot:
            for axis, deg in rot:
                ax = Vector({'X': (1, 0, 0), 'Y': (0, 1, 0),
                             'Z': (0, 0, 1)}[axis])
                m = Matrix.Rotation(D(deg), 4, ax) @ m
        if loc is not None:
            for i in range(3):
                m[i][3] = loc[i]
        pb.matrix = m
        self.upd()

    # ------------------------------------------------------------- ik limbs
    def _ik_solve(self, lower_bone, target, pole):
        t = bpy.data.objects.new(f"ikt_{lower_bone}", None)
        p = bpy.data.objects.new(f"ikp_{lower_bone}", None)
        t.location = Vector(target)
        p.location = Vector(pole)
        sc = bpy.context.scene.collection
        sc.objects.link(t)
        sc.objects.link(p)
        c = self.pb[lower_bone].constraints.new('IK')
        c.target = t
        c.pole_target = p
        c.pole_angle = 0.0
        c.chain_count = 2
        self._ik.append((lower_bone, c, t, p))

    def hand(self, side, target, pole=None):
        """IK the wrist (hand bone head) to `target`."""
        s = self.rs if side == 'r' else -self.rs
        if pole is None:
            pole = (s * 0.55, 0.55, 0.75)   # elbows back-down-out
        self._ik_solve(f"lowerarm_{side}", target, pole)

    def foot(self, side, target, pole=None):
        s = self.rs if side == 'r' else -self.rs
        if pole is None:
            pole = (s * 0.18, -1.2, 0.55)   # knees forward
        self._ik_solve(f"calf_{side}", target, pole)

    def apply_ik(self):
        """Evaluate constraints, bake them into the pose, drop them."""
        if not self._ik:
            return
        self.upd()
        bpy.ops.pose.select_all(action='SELECT')
        bpy.ops.pose.visual_transform_apply()
        self._clear_ik()
        self.upd()

    def _clear_ik(self):
        for bone, c, t, p in self._ik:
            try:
                self.pb[bone].constraints.remove(c)
            except Exception:
                pass
            for e in (t, p):
                try:
                    bpy.data.objects.remove(e, do_unlink=True)
                except Exception:
                    pass
        self._ik = []

    # ------------------------------------------------------------- fingers
    def _calibrate_fingers(self):
        """Find, per finger bone, the local euler axis+sign that curls the
        tip toward the wrist (grip). Done once at rest; local axes travel
        with the hand so the result is pose-independent."""
        self.reset()
        self._curl_axis = {}
        for name in FINGERS:
            pb = self.pb[name]
            side = name[-1]
            wrist = self.pb[f"hand_{side}"].matrix.to_translation()
            fore = self.pb[f"lowerarm_{side}"].matrix.to_translation()
            palm_ref = (wrist + fore) / 2.0
            best = ('X', 1.0, 1e9)
            for ax_i, ax in enumerate(('X', 'Y', 'Z')):
                for sign in (1.0, -1.0):
                    e = [0.0, 0.0, 0.0]
                    e[ax_i] = D(40.0) * sign
                    pb.rotation_euler = e
                    self.upd()
                    tip = pb.matrix.to_translation() + pb.matrix.col[1].to_3d() * pb.length
                    d = (tip - palm_ref).length
                    if d < best[2]:
                        best = (ax_i, sign, d)
                    pb.rotation_euler = (0.0, 0.0, 0.0)
            self._curl_axis[name] = best[:2]
        self.upd()

    def curl(self, side, amount, thumb=None):
        """Grip: 0 = open, 1 = full fist. thumb overrides thumb amount."""
        if thumb is None:
            thumb = amount * 0.7
        for name in FINGERS:
            if not name.endswith(f"_{side}"):
                continue
            a = thumb if name.startswith("thumb") else amount
            seg = int(name.split("_")[-2])
            degs = (45.0, 55.0, 40.0)[seg - 1] * a
            ax_i, sign = self._curl_axis[name]
            e = [0.0, 0.0, 0.0]
            e[ax_i] = D(degs) * sign
            self.pb[name].rotation_euler = e
        self.upd()

    # -------------------------------------------------------------- musket
    def musket(self, butt, muzzle_dir, top_dir=None):
        """Place the musket by butt position + muzzle direction (+ optional
        barrel-top direction for roll control). Returns the musket world
        matrix (armature space)."""
        y = Vector(muzzle_dir).normalized()
        if top_dir is None:
            up = Vector((0, 0, 1))
            if abs(y.dot(up)) > 0.92:
                up = Vector((0, -1, 0))   # vertical musket: top faces front
            z = (up - y * up.dot(y)).normalized()
        else:
            t = Vector(top_dir)
            z = (t - y * t.dot(y)).normalized()
        x = y.cross(z)
        M = Matrix((
            (x.x, y.x, z.x, butt[0]),
            (x.y, y.y, z.y, butt[1]),
            (x.z, y.z, z.z, butt[2]),
            (0.0, 0.0, 0.0, 1.0)))
        self.pb["prop_musket"].matrix = M
        self.upd()
        return M

    def musket_carry(self):
        """Shoulder arms: musket vertical against the right shoulder, butt
        supported by the right hand at the hip. Returns (M, grips)."""
        rs = self.rs
        lm = self.lm
        butt = (rs * 0.14, 0.045, lm.waist_z - 0.14)
        M = self.musket(butt, (0, 0.10, 0.99))   # slight backward cant
        self.hand('r', M @ Vector((0, 0.14, -0.045)))
        return M

    def ramrod(self, head_musket_local=None, flipped=False):
        """Key-ready ramrod placement in MUSKET-local space. None = seated
        in the channel (rest)."""
        pb = self.pb["prop_ramrod"]
        if head_musket_local is None:
            pb.matrix_basis = Matrix.Identity(4)
            self.upd()
            return
        Mm = self.pb["prop_musket"].matrix
        y = Mm.col[1].to_3d().normalized() * (-1.0 if flipped else 1.0)
        z = Mm.col[2].to_3d().normalized()
        z = (z - y * z.dot(y)).normalized()
        x = y.cross(z)
        h = Mm @ Vector(head_musket_local)
        pb.matrix = Matrix((
            (x.x, y.x, z.x, h.x),
            (x.y, y.y, z.y, h.y),
            (x.z, y.z, z.z, h.z),
            (0.0, 0.0, 0.0, 1.0)))
        self.upd()

    def rod_end(self):
        """World position of the rod's free (head) end for hand targets."""
        pb = self.pb["prop_ramrod"]
        return pb.matrix.to_translation() + pb.matrix.col[1].to_3d() * mk.ROD_LEN

    # -------------------------------------------------------------- keying
    def key(self, action, t):
        """Key EVERY bone at time t (seconds): rotation always, location
        for LOC_BONES. Absolute poses; euler continuity enforced."""
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
            if b.name in LOC_BONES:
                b.keyframe_insert("location", frame=f)


# ======================================================================
# Clip builders. Each takes (P: Poser) and returns the finished Action.
# ======================================================================

def _new_action(name):
    a = bpy.data.actions.get(name)
    if a is not None:
        bpy.data.actions.remove(a)
    a = bpy.data.actions.new(name)
    # survive .blend save/reload (build_kit.clear_scene purges actions
    # unconditionally between variants, so no cross-variant leakage)
    a.use_fake_user = True
    return a


def _stand_legs(P, stance=0.0):
    """Neutral standing feet. stance: +right foot back (oblique)."""
    rs = P.rs
    ank = P.lm.ankle_z
    hw = 0.11
    P.foot('l', (-rs * hw, 0.0, ank))
    P.foot('r', (rs * hw, stance, ank))


def _march_pose(P, phase, stride=0.30, lift=0.16, bob=0.018,
                lean=3.0, counter=5.0, carry=True, swing=0.16,
                head_pitch=0.0, rate_tag=""):
    """One normalized march-cycle pose. phase 0=L contact, 0.25=R passing,
    0.5=R contact, 0.75=L passing."""
    rs = P.rs
    ank = P.lm.ankle_z
    hw = 0.11
    P.reset()
    c = math.cos(phase * 2 * math.pi)          # +1 L fwd, -1 R fwd
    s = math.sin(phase * 2 * math.pi)
    ly = -stride * c
    ry = +stride * c
    lz = ank + lift * max(0.0, math.sin(phase * 2 * math.pi + math.pi))
    rz = ank + lift * max(0.0, s)
    P.root(loc=(0, 0, -bob * abs(c) + bob * 0.4))
    P.lean(lean)
    P.twist(-counter * c, bones=("spine_02", "spine_03"))
    P.rot("pelvis", 'Z', counter * 0.6 * c)
    P.look(pitch=head_pitch)
    P.foot('l', (-rs * hw, ly, lz))
    P.foot('r', (rs * hw, ry, rz))
    if carry:
        M = P.musket_carry()
        P.hand('l', (-rs * 0.24, -swing * c, P.lm.waist_z - 0.28))
    P.apply_ik()
    if carry:
        P.curl('r', 0.75)
        P.curl('l', 0.25)


def march(P, name="March_ShoulderArms", seconds=26.0 / FPS, **kw):
    a = _new_action(name)
    n = 8
    for i in range(n + 1):
        ph = i / n
        _march_pose(P, ph % 1.0, **kw)
        P.key(a, ph * seconds)
    return a


def route_step(P):
    # broken step: same cycle, longer/lazier, slouch, head wandering
    a = _new_action("RouteStep_Advance")
    seconds = 30.0 / FPS
    n = 8
    for i in range(n + 1):
        ph = i / n
        wob = math.sin(ph * 2 * math.pi + 1.3)
        _march_pose(P, ph % 1.0, stride=0.26, lift=0.10, lean=6.0,
                    counter=2.0, swing=0.10)
        P.look(yaw=8.0 * wob, pitch=4.0)
        P.rot("spine_03", 'Y', 3.0 * wob)
        P.key(a, ph * seconds)
    return a


def double_quick(P):
    a = _new_action("DoubleQuick")
    seconds = 18.0 / FPS
    n = 8
    for i in range(n + 1):
        ph = i / n
        _march_pose(P, ph % 1.0, stride=0.42, lift=0.26, bob=0.03,
                    lean=9.0, counter=4.0, swing=0.24)
        P.key(a, ph * seconds)
    return a


def routed_run(P):
    """Panic sprint, musket at the trail in the right hand."""
    a = _new_action("Routed_Run")
    seconds = 16.0 / FPS
    rs = P.rs
    n = 8
    for i in range(n + 1):
        ph = (i / n) % 1.0
        c = math.cos(ph * 2 * math.pi)
        s = math.sin(ph * 2 * math.pi)
        ank = P.lm.ankle_z
        P.reset()
        P.root(loc=(0, 0, 0.02 * abs(s)))
        P.lean(16.0)
        P.look(pitch=-6.0)             # head up/back: panic
        P.foot('l', (-rs * 0.12, -0.5 * c, ank + 0.3 * max(0.0, -s)))
        P.foot('r', (rs * 0.12, 0.5 * c, ank + 0.3 * max(0.0, s)))
        # musket at trail: horizontal at the right side, butt behind
        M = P.musket((rs * 0.30, 0.35, P.lm.waist_z - 0.18), (0, -1, -0.06))
        P.hand('r', M @ Vector((0, 0.42, -0.02)))
        P.hand('l', (-rs * 0.30, 0.30 * c, P.lm.waist_z + 0.10 * s))
        P.apply_ik()
        P.curl('r', 0.8)
        P.key(a, (i / n) * seconds)
    return a


def stand_ready(P):
    a = _new_action("Stand_Ready")
    seconds = 2.0
    for i, t in enumerate((0.0, 0.5, 1.0, 1.5, 2.0)):
        br = math.sin(t / seconds * 2 * math.pi)
        P.reset()
        _stand_legs(P)
        P.lean(2.0)
        P.rot("spine_03", 'X', -1.5 * br)    # breath
        P.root(loc=(0.006 * br, 0, 0))       # weight shift
        P.musket_carry()
        P.hand('l', (-P.rs * 0.22, 0.02, P.lm.waist_z - 0.28))
        P.apply_ik()
        P.curl('r', 0.75)
        P.curl('l', 0.2)
        P.key(a, t)
    return a


def _aim_pose(P, blend=1.0, recoil=0.0, stance=0.28):
    """blend 0 = carry, 1 = full aim. recoil in [0,1] shoves the piece."""
    rs = P.rs
    lm = P.lm
    P.reset()
    _stand_legs(P, stance=stance)
    P.rot("foot_r", 'Z', -rs * 25.0)
    P.twist(-rs * 18.0 * blend)             # bladed stance
    P.lean(3.0 + 2.0 * blend)
    if blend < 1.0:
        M = P.musket_carry()
        P.hand('l', (-rs * 0.22, 0.0, lm.waist_z - 0.28))
    else:
        butt = Vector((rs * 0.13, 0.10 - 0.05 * recoil, lm.shoulder_z - 0.035))
        mdir = Vector((0, -1, 0.03 + 0.10 * recoil)).normalized()
        M = P.musket(butt, mdir, top_dir=(0, 0, 1))
        P.hand('r', M @ Vector((0, 0.17, -0.035)))
        P.hand('l', M @ Vector((0, 0.52, -0.045)),
               pole=(-rs * 0.30, -0.35, lm.waist_z))
        P.look(yaw=-rs * 8.0, pitch=8.0 - 12.0 * recoil, tilt=rs * 6.0)
        P.rot("spine_03", 'X', -5.0 * recoil)   # shoulder shove (backward)
    P.apply_ik()
    P.curl('r', 0.8)
    P.curl('l', 0.55 if blend >= 1.0 else 0.2)
    return M


def aim(P):
    a = _new_action("Aim_Musket")
    for t, b in ((0.0, 0.0), (0.35, 0.55), (0.7, 1.0), (1.4, 1.0)):
        _aim_pose(P, blend=b)
        P.key(a, t)
    return a


def fire(P):
    a = _new_action("Fire_Recoil")
    for t, rc in ((0.0, 0.0), (0.10, 1.0), (0.22, 0.55), (0.5, 0.12), (0.9, 0.0)):
        _aim_pose(P, blend=1.0, recoil=rc)
        P.key(a, t)
    return a


def _reload_musket_frame(P):
    """Musket in the LOAD position: butt on the ground ahead of the right
    foot, barrel to the FRONT (muzzle canted slightly away from the face,
    per the drill and so the muzzle-work poses read), left hand steadying."""
    rs = P.rs
    butt = (rs * 0.07, -0.34, 0.005)
    mdir = Vector((-rs * 0.02, -0.07, 0.997)).normalized()
    return P.musket(butt, mdir), Vector(butt), mdir


def reload_musket(P):
    """The hero clip. Historically ordered stages per RELOAD_STAGES."""
    a = _new_action("Reload_Musket")
    rs = P.rs
    lm = P.lm

    box = Vector((rs * 0.17, 0.16, lm.waist_z - 0.02))        # cartridge box
    pouch = Vector((rs * 0.10, -0.17, lm.waist_z + 0.01))     # cap pouch
    mouth = Vector((0.0, -0.135, lm.neck_z + 0.14))

    def base(lean=7.0, look_dn=10.0):
        P.reset()
        _stand_legs(P, stance=0.16)
        P.lean(lean)
        P.look(pitch=look_dn)
        M, butt, mdir = _reload_musket_frame(P)
        muzzle = butt + mdir * mk.TOTAL_LEN
        # left hand steadies the barrel just below the muzzle throughout
        P.hand('l', M @ Vector((0, mk.TOTAL_LEN - 0.28, 0.02)),
               pole=(-rs * 0.5, -0.5, 1.1))
        return M, butt, mdir, muzzle

    def key_at(t, rh_target, rod=None, flipped=False, look=None,
               lean=7.0, rh_pole=None, curl_r=0.55):
        # default right elbow pole: out to the right, forward and high —
        # the rear-down default wraps the arm behind the neck for
        # muzzle-height work (seen in the first Unity stills)
        if rh_pole is None:
            rh_pole = (rs * 0.75, -0.55, 1.25)
        M, butt, mdir, muzzle = base(lean=lean,
                                     look_dn=look if look is not None else 10.0)
        P.ramrod(rod, flipped=flipped)
        P.hand('r', rh_target(M, butt, mdir, muzzle), pole=rh_pole)
        P.apply_ik()
        P.curl('l', 0.5)
        P.curl('r', curl_r)
        P.key(a, t)

    S = dict(RELOAD_STAGES)

    # --- 0.0 LOAD: piece grounded; the right hand rides high near the
    # muzzle (it just guided the piece down — Casey's Load position)
    key_at(S["load"], lambda M, b, d, mz: M @ Vector((0, mk.TOTAL_LEN - 0.38, -0.045)),
           look=8.0)
    key_at(1.1, lambda M, b, d, mz: M @ Vector((0, mk.TOTAL_LEN - 0.38, -0.045)))

    # --- 1.5 HANDLE CARTRIDGE: right hand back to the box, then to mouth
    key_at(S["handle_cartridge"] + 0.6,
           lambda M, b, d, mz: box, rh_pole=(rs * 0.6, 0.55, 0.9),
           look=10.0, curl_r=0.3)
    key_at(2.9, lambda M, b, d, mz: box, rh_pole=(rs * 0.6, 0.55, 0.9),
           curl_r=0.85)                                   # grasp round
    key_at(4.1, lambda M, b, d, mz: mouth, look=16.0, curl_r=0.85)

    # --- 4.5 TEAR: head bites, sharp little jerk aside
    key_at(S["tear_cartridge"] + 0.3,
           lambda M, b, d, mz: mouth + Vector((0, 0, -0.01)),
           look=18.0, curl_r=0.9)
    key_at(5.4, lambda M, b, d, mz: mouth + Vector((rs * 0.09, -0.02, 0.02)),
           look=14.0, curl_r=0.9)

    # --- 6.0 CHARGE: pour at the muzzle, press the ball
    key_at(S["charge_cartridge"] + 0.5,
           lambda M, b, d, mz: mz + Vector((0, 0.015, 0.06)),
           look=12.0, curl_r=0.7)
    key_at(7.4, lambda M, b, d, mz: mz + Vector((0, 0.015, 0.025)),
           look=12.0, curl_r=0.5)                          # pour/press
    key_at(8.2, lambda M, b, d, mz: mz + Vector((0, 0.02, 0.05)),
           look=12.0, curl_r=0.4)

    # --- 8.5 DRAW RAMMER: hand to rod head, long pull, flip over muzzle
    rodC = Vector((0, mk.ROD_Y0, mk.ROD_Z))               # channel (rest)
    key_at(S["draw_rammer"] + 0.3,
           lambda M, b, d, mz: M @ Vector((0, mk.ROD_Y1 - 0.04, mk.ROD_Z - 0.02)),
           rod=None, look=16.0, curl_r=0.8)
    # rod half-drawn: head slides +0.5 along the channel
    key_at(9.4,
           lambda M, b, d, mz: M @ Vector((0, mk.ROD_Y0 + 0.62 + mk.ROD_LEN - 0.06, mk.ROD_Z)),
           rod=rodC + Vector((0, 0.62, 0)), look=10.0, curl_r=0.85)
    # rod clear, flipped, poised above the muzzle pointing down
    key_at(10.6,
           lambda M, b, d, mz: M @ Vector((0, mk.TOTAL_LEN + 0.62, 0.01)),
           rod=(0, mk.TOTAL_LEN + 0.60, 0.012), flipped=True,
           look=6.0, curl_r=0.85)

    # --- 11.0 RAM: two strokes down the bore
    def rod_at(depth):
        # flipped rod, head end above the muzzle; tip sits `depth` into bore
        return (0, mk.TOTAL_LEN - depth + mk.ROD_LEN, 0.012)
    key_at(S["ram_cartridge"] + 0.4,
           lambda M, b, d, mz: M @ Vector(rod_at(0.55)),
           rod=rod_at(0.55), flipped=True, look=12.0, curl_r=0.85)
    key_at(12.1, lambda M, b, d, mz: M @ Vector(rod_at(0.12)),
           rod=rod_at(0.12), flipped=True, look=10.0, curl_r=0.85)
    key_at(12.8, lambda M, b, d, mz: M @ Vector(rod_at(0.60)),
           rod=rod_at(0.60), flipped=True, look=12.0, curl_r=0.85)

    # --- 13.5 RETURN RAMMER: out, flip back, seat in the channel
    key_at(S["return_rammer"] + 0.5,
           lambda M, b, d, mz: M @ Vector((0, mk.TOTAL_LEN + 0.60, 0.012)),
           rod=(0, mk.TOTAL_LEN + 0.58, 0.012), flipped=True,
           look=6.0, curl_r=0.85)
    key_at(14.7,
           lambda M, b, d, mz: M @ Vector((0, mk.ROD_Y0 + 0.55 + mk.ROD_LEN, mk.ROD_Z)),
           rod=rodC + Vector((0, 0.55, 0)), look=12.0, curl_r=0.85)
    key_at(15.6, lambda M, b, d, mz: M @ Vector((0, mk.ROD_Y1 - 0.03, mk.ROD_Z - 0.02)),
           rod=None, look=14.0, curl_r=0.7)

    # --- 16.0 PRIME: piece up across the body, lock high; cap from pouch
    def prime_key(t, rh, look_dn, curl_r=0.6):
        P.reset()
        _stand_legs(P, stance=0.16)
        P.lean(6.0)
        P.look(pitch=look_dn)
        butt = (rs * 0.16, 0.22, lm.waist_z - 0.06)
        mdir = Vector((-rs * 0.25, -0.90, 0.36)).normalized()
        M = P.musket(butt, mdir, top_dir=(-rs * 0.3, 0.3, 1.0))
        P.ramrod(None)
        P.hand('l', M @ Vector((0, 0.60, -0.05)), pole=(-rs * 0.5, -0.4, 1.0))
        P.hand('r', rh(M), pole=(rs * 0.6, 0.5, 0.9))
        P.apply_ik()
        P.curl('l', 0.6)
        P.curl('r', curl_r)
        P.key(a, t)

    lock = Vector((rs * 0.02, mk.BREECH_Y - 0.03, 0.03))
    prime_key(S["prime"] + 0.25, lambda M: M @ lock, 12.0, curl_r=0.5)  # half-cock
    prime_key(16.9, lambda M: pouch, 10.0, curl_r=0.4)                  # to pouch
    prime_key(17.5, lambda M: M @ lock, 12.0, curl_r=0.35)              # cap on

    # --- 18.0 SHOULDER: back to the carry
    P.reset()
    _stand_legs(P)
    P.lean(4.0)
    P.musket_carry()
    P.ramrod(None)
    P.hand('l', (-rs * 0.22, 0.0, lm.waist_z - 0.28))
    P.apply_ik()
    P.curl('r', 0.75)
    P.curl('l', 0.2)
    P.key(a, S["shoulder"] + 0.9)
    P.key(a, RELOAD_SECONDS)
    return a


def fence_cross(P):
    """Cross a rail fence whose top rail is ~1.10 m high, 0.55 m ahead.
    Root motion: 1.30 m forward (-Y) total; the Unity harness owns path
    placement and resumes 1.30 m ahead after the clip."""
    a = _new_action("Cross_RailFence")
    rs = P.rs
    ank = P.lm.ankle_z
    RAIL_Y, RAIL_Z = -0.55, 1.10

    def pose(t, rooty, rootz, lf, rf, lean_deg, lh=None, note_curl=0.6):
        P.reset()
        P.root(loc=(0, rooty, rootz))
        P.lean(lean_deg)
        P.look(pitch=14.0)
        P.foot('l', lf)
        P.foot('r', rf)
        # musket in the right hand, held out vertical-ish for balance
        M = P.musket((rs * 0.30 , rooty + 0.05, 0.95 + rootz), (rs * 0.10, 0.06, 0.99))
        P.hand('r', M @ Vector((0, 0.30, -0.05)))
        if lh is not None:
            P.hand('l', lh, pole=(-rs * 0.5, -0.3, 1.3))
        P.apply_ik()
        P.curl('r', 0.8)
        P.curl('l', note_curl)
        P.key(a, t)

    hw = 0.11
    rail_grip = (-rs * 0.16, RAIL_Y, RAIL_Z + 0.02)
    # approach halt, left hand takes the rail
    pose(0.0, 0.0, 0.0, (-rs * hw, -0.10, ank), (rs * hw, 0.12, ank), 6.0)
    pose(0.5, -0.05, 0.0, (-rs * hw, -0.15, ank), (rs * hw, 0.10, ank), 14.0,
         lh=rail_grip, note_curl=0.85)
    # right leg swings over the rail
    pose(1.0, -0.16, 0.10, (-rs * hw, -0.02, ank), (rs * 0.18, RAIL_Y + 0.06, RAIL_Z + 0.12),
         22.0, lh=rail_grip, note_curl=0.9)
    # weight crosses; right foot lands beyond
    pose(1.6, -0.55, 0.28, (-rs * hw, 0.28, ank + 0.30), (rs * 0.16, -0.95, ank),
         24.0, lh=rail_grip, note_curl=0.9)
    # left leg over the rail
    pose(2.3, -0.85, 0.14, (-rs * 0.16, RAIL_Y - 0.10, RAIL_Z + 0.10),
         (rs * 0.16, -1.05, ank), 18.0, lh=rail_grip, note_curl=0.7)
    # land and recover
    pose(3.0, -1.15, 0.0, (-rs * hw, -1.30, ank), (rs * 0.13, -1.05, ank), 12.0)
    pose(4.0, -1.30, 0.0, (-rs * hw, -1.36, ank), (rs * hw, -1.24, ank), 5.0)
    return a


def hit_nonfatal(P):
    """Ball strike, upper left chest — stagger, clutch, stay standing."""
    a = _new_action("Hit_Nonfatal")
    rs = P.rs
    lm = P.lm
    wound = Vector((-rs * 0.09, -0.13, lm.chest_z + 0.09))

    def pose(t, jerk, stag_y, hunch, clutch, root_z=0.0):
        P.reset()
        _stand_legs(P, stance=0.10 if stag_y else 0.0)
        P.root(loc=(0, stag_y, root_z))
        P.lean(hunch)
        P.twist(rs * jerk, bones=("spine_02", "spine_03"))
        P.look(pitch=hunch * 0.8, yaw=rs * jerk * 0.4)
        M = P.musket((rs * 0.16, 0.05, lm.waist_z - 0.20), (0, 0.05, 1.0))
        P.hand('r', M @ Vector((0, 0.16, -0.04)))
        if clutch:
            P.hand('l', wound, pole=(-rs * 0.45, -0.35, 1.0))
        else:
            P.hand('l', (-rs * 0.24, 0.0, lm.waist_z - 0.28))
        P.apply_ik()
        P.curl('r', 0.75)
        P.curl('l', 0.8 if clutch else 0.2)
        P.key(a, t)

    pose(0.0, 0.0, 0.0, 3.0, False)
    pose(0.12, 14.0, 0.06, 6.0, False)          # impact twist
    pose(0.35, 8.0, 0.14, 14.0, True, -0.05)    # stagger back, clutch
    pose(0.7, 4.0, 0.10, 12.0, True, -0.03)
    pose(1.25, 1.0, 0.06, 8.0, True)            # keeps a hunch: wounded
    return a


def _drop_musket(P, root_before_key, world_loc, world_dir):
    """Key helper: place the musket at a WORLD pose regardless of the
    Root motion already applied (musket lands beside the body)."""
    P.musket(world_loc, world_dir)


def fall_back(P):
    """Shot from the front — thrown onto the back. Body persists at the
    final pose (last key held to the clip end)."""
    a = _new_action("Fall_Shot_Front_Back")
    rs = P.rs
    ank = P.lm.ankle_z

    def pose(t, tip, root_y, root_z, kneebend, arms_up, musket_world=None):
        P.reset()
        P.root(loc=(0, root_y, root_z), rot=[('X', -tip)])
        P.rot("spine_02", 'X', tip * 0.10)     # spine slightly curled vs rigid
        P.look(pitch=-min(20.0, tip * 0.3))
        if kneebend <= 1.0:
            P.foot('l', (-rs * 0.11, -0.02, ank + 0.02))
            P.foot('r', (rs * 0.11, 0.04, ank + 0.02))
        else:
            # released: knees up as the body lies back
            P.foot('l', (-rs * 0.13, 0.42, 0.14))
            P.foot('r', (rs * 0.16, 0.30, 0.22))
        if musket_world is None:
            M = P.musket_carry()
        else:
            _drop_musket(P, None, *musket_world)
            up = arms_up
            P.hand('r', (rs * (0.30 + 0.25 * up), 0.25 * up, 0.9 - 0.5 * up))
        P.hand('l', (-rs * (0.28 + 0.3 * arms_up), 0.1 * arms_up, 0.85 - 0.4 * arms_up))
        P.apply_ik()
        P.curl('r', 0.5)
        P.curl('l', 0.4)
        P.key(a, t)

    pose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
    pose(0.12, 8.0, 0.02, 0.0, 0.0, 0.4)                       # impact jerk
    pose(0.35, 40.0, 0.10, -0.04, 0.5, 0.9,
         musket_world=((rs * 0.5, -0.05, 0.65), (rs * 0.3, -0.9, 0.2)))
    pose(0.6, 74.0, 0.16, -0.10, 1.2, 1.0,
         musket_world=((rs * 0.55, -0.15, 0.25), (rs * 0.25, -0.95, 0.05)))
    pose(0.85, 86.0, 0.20, -0.13, 1.5, 0.7,
         musket_world=((rs * 0.60, -0.25, 0.035), (rs * 0.2, -0.98, 0.0)))
    # settle; one knee drops, arm flops out
    P.reset()
    P.root(loc=(0, 0.20, -0.14), rot=[('X', -88.0)])
    P.look(pitch=-12.0, tilt=rs * 14.0)
    P.foot('l', (-rs * 0.15, 0.30, 0.10))
    P.foot('r', (rs * 0.18, 0.24, 0.26))
    _drop_musket(P, None, (rs * 0.62, -0.28, 0.035), (rs * 0.2, -0.98, 0.0))
    P.hand('r', (rs * 0.70, 0.55, 0.04))
    P.hand('l', (-rs * 0.45, 0.75, 0.05))
    P.apply_ik()
    P.curl('r', 0.3)
    P.curl('l', 0.3)
    P.key(a, 1.15)
    P.key(a, 2.0)   # persistent body hold
    return a


def fall_crumple(P):
    """Shot from the front — legs give, sinks to knees, folds forward."""
    a = _new_action("Fall_Shot_Front_Crumple")
    rs = P.rs
    ank = P.lm.ankle_z

    def key(t):
        P.apply_ik()
        P.key(a, t)

    # stand
    P.reset()
    _stand_legs(P)
    P.musket_carry()
    P.hand('l', (-rs * 0.24, 0.0, P.lm.waist_z - 0.28))
    key(0.0)
    # hit: small buckle
    P.reset()
    P.root(loc=(0, 0.02, -0.09))
    P.lean(10.0)
    P.foot('l', (-rs * 0.11, -0.02, ank))
    P.foot('r', (rs * 0.11, 0.05, ank))
    M = P.musket((rs * 0.15, 0.02, 0.80), (0, 0.1, 0.99))
    P.hand('r', M @ Vector((0, 0.16, -0.04)))
    P.hand('l', (-rs * 0.26, 0.02, 0.8))
    key(0.2)
    # knees hit the ground (musket drops forward)
    P.reset()
    P.root(loc=(0, 0.06, -0.46))
    P.lean(16.0)
    P.look(pitch=22.0)
    P.foot('l', (-rs * 0.12, 0.38, 0.08))
    P.foot('r', (rs * 0.12, 0.40, 0.08))
    _drop_musket(P, None, (-rs * 0.28, -0.55, 0.03), (-rs * 0.2, -0.96, 0.0))
    P.hand('r', (rs * 0.30, -0.10, 0.55))
    P.hand('l', (-rs * 0.30, -0.12, 0.55))
    key(0.55)
    # fold forward onto hands
    P.reset()
    P.root(loc=(0, 0.10, -0.52), rot=[('X', 38.0)])
    for b in SPINE:
        P.rot(b, 'X', 10.0)   # curl forward
    P.look(pitch=26.0)
    P.foot('l', (-rs * 0.12, 0.55, 0.10))
    P.foot('r', (rs * 0.12, 0.58, 0.10))
    _drop_musket(P, None, (-rs * 0.30, -0.58, 0.03), (-rs * 0.2, -0.96, 0.0))
    P.hand('r', (rs * 0.26, -0.48, 0.05))
    P.hand('l', (-rs * 0.26, -0.50, 0.05))
    key(0.95)
    # flat, face down, slightly on the right side
    P.reset()
    P.root(loc=(0, 0.14, -0.62), rot=[('X', 66.0), ('Z', -rs * 10.0)])
    P.look(pitch=10.0, tilt=rs * 18.0)
    P.foot('l', (-rs * 0.16, 0.70, 0.08))
    P.foot('r', (rs * 0.14, 0.78, 0.06))
    _drop_musket(P, None, (-rs * 0.32, -0.60, 0.035), (-rs * 0.2, -0.96, 0.0))
    P.hand('r', (rs * 0.32, -0.42, 0.03))
    P.hand('l', (-rs * 0.38, -0.35, 0.03))
    key(1.4)
    P.key(a, 2.3)   # persistent hold
    return a


def fall_side(P):
    """Canister burst from the character's LEFT — knocked down rightward."""
    a = _new_action("Fall_Shot_Left_Side")
    rs = P.rs
    ank = P.lm.ankle_z
    tipdir = rs  # tip toward character-right

    def pose(t, tip, root_z, brace, musket_world=None):
        P.reset()
        P.root(loc=(tipdir * 0.06 * tip / 30.0, 0.0, root_z),
               rot=[('Y', tipdir * tip)])
        P.rot("spine_02", 'Z', -tipdir * min(14.0, tip * 0.35))
        P.look(tilt=-tipdir * min(16.0, tip * 0.4), pitch=6.0)
        if tip < 45.0:
            P.foot('l', (-rs * 0.11, -0.02, ank))
            P.foot('r', (rs * 0.42 * tip / 45.0 + rs * 0.11, 0.03, ank + 0.02))
        else:
            P.foot('l', (-rs * 0.02, 0.12, 0.14))
            P.foot('r', (rs * 0.30, -0.10, 0.10))
        if musket_world is None:
            M = P.musket_carry()
        else:
            _drop_musket(P, None, *musket_world)
            P.hand('r', (rs * 0.55, 0.05, max(0.06, 0.8 - tip / 100.0)))
        if brace:
            P.hand('r', (rs * 0.78, 0.05, 0.06))
        P.hand('l', (-rs * 0.20, -0.06, 0.9 - tip / 140.0))
        P.apply_ik()
        P.curl('r', 0.5)
        P.curl('l', 0.4)
        P.key(a, t)

    pose(0.0, 0.0, 0.0, False)
    pose(0.10, 14.0, -0.01, False)
    pose(0.32, 48.0, -0.06, False,
         musket_world=((-rs * 0.35, -0.30, 0.55), (-rs * 0.6, -0.7, 0.3)))
    pose(0.55, 78.0, -0.10, True,
         musket_world=((-rs * 0.45, -0.40, 0.05), (-rs * 0.7, -0.7, 0.0)))
    # down on the right side, legs scissored
    P.reset()
    P.root(loc=(tipdir * 0.22, 0.02, -0.16), rot=[('Y', tipdir * 86.0)])
    P.rot("spine_02", 'Z', -tipdir * 10.0)
    P.look(tilt=-tipdir * 10.0, pitch=12.0)
    P.foot('l', (-rs * 0.02, -0.35, 0.16))   # top leg forward
    P.foot('r', (rs * 0.14, 0.28, 0.12))
    _drop_musket(P, None, (-rs * 0.50, -0.42, 0.035), (-rs * 0.7, -0.72, 0.0))
    P.hand('r', (rs * 0.55, 0.15, 0.05))
    P.hand('l', (-rs * 0.15, -0.30, 0.10))
    P.apply_ik()
    P.curl('r', 0.35)
    P.curl('l', 0.35)
    P.key(a, 0.85)
    P.key(a, 1.7)   # persistent hold
    return a


def turn_retreat(P):
    """Look back, 180-degree turn, hunched walk away. Frames after
    RETREAT_LOOP_START seconds form a loopable walk cycle."""
    a = _new_action("Turn_Retreat")
    rs = P.rs
    ank = P.lm.ankle_z
    hw = 0.11

    # look over the shoulder
    P.reset()
    _stand_legs(P)
    P.lean(4.0)
    P.look(yaw=rs * 55.0)
    P.musket_carry()
    P.hand('l', (-rs * 0.24, 0.0, P.lm.waist_z - 0.28))
    P.apply_ik()
    P.curl('r', 0.75)
    P.key(a, 0.0)
    P.key(a, 0.25)

    # stepping turn (three keys around Z)
    for t, yaw in ((0.5, 60.0), (0.75, 120.0), (1.0, 180.0)):
        P.reset()
        P.root(rot=[('Z', rs * yaw)])
        P.lean(10.0)
        P.look(pitch=10.0)
        # feet track the rotated frame: local stance rotated by yaw
        R = Matrix.Rotation(D(rs * yaw), 4, 'Z')
        fl = R @ Vector((-rs * hw, -0.08, ank))
        fr = R @ Vector((rs * hw, 0.10, ank))
        P.foot('l', fl)
        P.foot('r', fr)
        M = P.musket(
            (R @ Vector((rs * 0.16, 0.05, 0.95))),
            (R @ Vector((0, 0.10, 0.99))))
        P.hand('r', M @ Vector((0, 0.14, -0.045)))
        P.hand('l', R @ Vector((-rs * 0.22, -0.10, P.lm.waist_z - 0.20)))
        P.apply_ik()
        P.curl('r', 0.8)
        P.key(a, t)

    # hunched walk cycle facing +Y (turned around), 1.0 s per cycle x2
    for cyc in range(2):
        for i in range(4):
            ph = i / 4.0
            t = 1.0 + cyc * 1.0 + ph * 1.0
            c = math.cos(ph * 2 * math.pi)
            s = math.sin(ph * 2 * math.pi)
            P.reset()
            P.root(rot=[('Z', rs * 180.0)], loc=(0, 0, -0.02 * abs(c)))
            P.lean(14.0)
            P.look(pitch=14.0)
            # walking toward +Y now
            P.foot('l', (rs * hw, +0.30 * c, ank + 0.12 * max(0.0, -s)))
            P.foot('r', (-rs * hw, -0.30 * c, ank + 0.12 * max(0.0, s)))
            M = P.musket((-rs * 0.16, -0.05, 0.92), (0, -0.12, 0.99))
            P.hand('r', M @ Vector((0, 0.14, -0.045)))
            P.hand('l', (rs * 0.24, -0.10 * c, P.lm.waist_z - 0.26))
            P.apply_ik()
            P.curl('r', 0.8)
            P.key(a, t)
    # close the loop exactly
    P.key(a, 3.0)
    return a


RETREAT_LOOP_START = 1.0   # seconds; [1.0 .. 3.0) is the loopable walk


def waver(P):
    a = _new_action("Waver")
    rs = P.rs
    ank = P.lm.ankle_z
    for t, wx, yaw, back in ((0.0, 0.0, 0.0, 0.0), (0.5, 0.05, -35.0, 0.0),
                             (1.0, -0.02, 0.0, 0.0), (1.5, 0.05, 40.0, 0.0),
                             (2.0, 0.0, 15.0, 0.10), (2.6, 0.02, -25.0, 0.14),
                             (3.2, 0.0, 0.0, 0.14), (4.0, 0.0, 0.0, 0.14)):
        P.reset()
        P.root(loc=(rs * wx, back, 0))
        P.lean(6.0)
        P.look(yaw=yaw, pitch=6.0)
        P.foot('l', (-rs * 0.11, back - 0.02, ank))
        P.foot('r', (rs * 0.11, back + 0.12 if back else 0.04, ank))
        M = P.musket((rs * 0.15, back + 0.04, 0.90), (0, 0.14, 0.98))
        P.hand('r', M @ Vector((0, 0.15, -0.04)))
        P.hand('l', M @ Vector((0, 0.55, -0.03)),
               pole=(-rs * 0.5, back - 0.4, 1.1))
        P.apply_ik()
        P.curl('r', 0.8)
        P.curl('l', 0.6)
        P.key(a, t)
    return a


def kneel_ready(P):
    a = _new_action("Kneel_Ready")
    rs = P.rs
    ank = P.lm.ankle_z
    # stand -> right knee down, musket vertical at the right, held
    P.reset()
    _stand_legs(P)
    P.musket_carry()
    P.hand('l', (-rs * 0.24, 0.0, P.lm.waist_z - 0.28))
    P.apply_ik()
    P.curl('r', 0.75)
    P.key(a, 0.0)
    for t, drop in ((0.4, 0.20), (0.9, 0.42)):
        P.reset()
        P.root(loc=(0, 0.04, -drop))
        P.lean(8.0)
        P.foot('l', (-rs * 0.12, -0.16, ank))
        P.foot('r', (rs * 0.12, 0.34, 0.10))   # shin flat behind
        M = P.musket((rs * 0.22, -0.06, 0.005), (rs * 0.03, 0.10, 0.99))
        P.hand('r', M @ Vector((0, 1.00, -0.02)))
        P.hand('l', (-rs * 0.20, -0.16, P.lm.waist_z - drop))
        P.apply_ik()
        P.curl('r', 0.8)
        P.key(a, t)
    P.key(a, 1.5)
    return a


def brace_artillery(P):
    """Nearby shell burst: duck, shoulders up, hold, recover."""
    a = _new_action("Brace_Artillery")
    rs = P.rs
    ank = P.lm.ankle_z

    def pose(t, duck, root_z):
        P.reset()
        P.root(loc=(0, 0.0, root_z))
        P.lean(6.0 + duck * 18.0)
        P.look(pitch=duck * 26.0, yaw=-rs * duck * 12.0)
        # shoulders shrug up: clavicles point sideways, so lift the tips
        # about world Y (opposite signs per side)
        P.rot("clavicle_l", 'Y', -duck * 10.0)
        P.rot("clavicle_r", 'Y', duck * 10.0)
        P.foot('l', (-rs * 0.13, -0.05, ank))
        P.foot('r', (rs * 0.13, 0.08, ank))
        M = P.musket((rs * 0.14, 0.02, 0.85 + root_z), (0, 0.16, 0.98))
        P.hand('r', M @ Vector((0, 0.16, -0.04)))
        P.hand('l', M @ Vector((0, 0.50, -0.03)),
               pole=(-rs * 0.5, -0.4, 1.0))
        P.apply_ik()
        P.curl('r', 0.85)
        P.curl('l', 0.7)
        P.key(a, t)

    pose(0.0, 0.0, 0.0)
    pose(0.15, 1.0, -0.16)
    pose(0.8, 1.0, -0.16)
    pose(1.1, 0.6, -0.10)
    pose(1.5, 0.0, 0.0)
    return a


def flinch(P):
    a = _new_action("Flinch_Recover")
    rs = P.rs
    for t, duck in ((0.0, 0.0), (0.12, 1.0), (0.45, 0.5), (0.85, 0.0)):
        P.reset()
        _stand_legs(P)
        P.lean(4.0 + duck * 10.0)
        P.look(pitch=duck * 18.0, yaw=rs * duck * 10.0, tilt=-rs * duck * 8.0)
        P.musket_carry()
        P.hand('l', (-rs * 0.24, 0.0, P.lm.waist_z - 0.28))
        P.apply_ik()
        P.curl('r', 0.78)
        P.key(a, t)
    return a


def halt_dress(P):
    """Halt from the march, then dress right: head snap, side shuffle."""
    a = _new_action("Halt_DressLine")
    rs = P.rs
    ank = P.lm.ankle_z
    hw = 0.11
    # closing step
    _march_pose(P, 0.0)
    P.key(a, 0.0)
    P.reset()
    _stand_legs(P)
    P.lean(3.0)
    P.musket_carry()
    P.hand('l', (-rs * 0.24, 0.0, P.lm.waist_z - 0.28))
    P.apply_ik()
    P.curl('r', 0.75)
    P.key(a, 0.35)
    # dress right: head snaps, two small side shuffles
    for t, dx, yaw in ((0.7, 0.0, -rs * 50.0), (1.1, rs * 0.05, -rs * 50.0),
                       (1.5, rs * 0.09, -rs * 50.0), (2.0, rs * 0.09, 0.0),
                       (2.5, rs * 0.09, 0.0)):
        P.reset()
        P.root(loc=(dx, 0, 0))
        _stand_legs(P)
        P.lean(2.0)
        P.look(yaw=yaw)
        P.musket_carry()
        P.hand('l', (-rs * 0.24, 0.0, P.lm.waist_z - 0.28))
        P.apply_ik()
        P.curl('r', 0.75)
        P.key(a, t)
    return a


def prone_crawl(P):
    """Wounded drag: prone, alternate elbow pulls (loop)."""
    a = _new_action("Prone_Wounded_Crawl")
    rs = P.rs
    for i in range(5):
        ph = i / 4.0
        c = math.cos(ph * 2 * math.pi)
        P.reset()
        P.root(loc=(0, 0.16, -0.60), rot=[('X', 74.0)])
        P.rot("spine_02", 'X', -8.0)
        P.look(pitch=-18.0)   # head strains up
        P.foot('l', (-rs * 0.15, 0.65, 0.10))
        P.foot('r', (rs * 0.15, 0.72, 0.08))
        P.hand('l', (-rs * 0.32, -0.42 - 0.10 * c, 0.05))
        P.hand('r', (rs * 0.32, -0.42 + 0.10 * c, 0.05))
        _drop_musket(P, None, (rs * 0.45, 0.25, 0.035), (rs * 0.1, -0.99, 0.0))
        P.apply_ik()
        P.curl('l', 0.6)
        P.curl('r', 0.6)
        P.key(a, ph * 1.6)
    return a


# ======================================================================
# entry point
# ======================================================================

GATE_P6_PRIORITY = (
    "March_ShoulderArms", "Aim_Musket", "Fire_Recoil", "Reload_Musket",
    "Cross_RailFence", "Hit_Nonfatal", "Fall_Shot_Front_Back",
    "Fall_Shot_Front_Crumple", "Fall_Shot_Left_Side", "Turn_Retreat",
)


def author_all(rig, lm):
    """Author the full §7.3 vocabulary on this rig. Gate P6 priority set
    first (plan order), then the remainder."""
    P = Poser(rig, lm)
    made = []
    for fn in (march, aim, fire, reload_musket, fence_cross, hit_nonfatal,
               fall_back, fall_crumple, fall_side, turn_retreat,
               stand_ready, route_step, double_quick, halt_dress,
               kneel_ready, brace_artillery, flinch, waver, routed_run,
               prone_crawl):
        a = fn(P)
        made.append(a.name)
        print(f"[clips] authored {a.name}: "
              f"{a.frame_range[0]:.0f}..{a.frame_range[1]:.0f} @ {FPS}fps")
    P.reset()
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.context.scene.render.fps = FPS
    print(f"[clips] {len(made)} clips authored")
    return made
