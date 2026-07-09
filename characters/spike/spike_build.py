"""Phase 6 feasibility spike — headless bpy build.

Input:  CC0 Blender Human Base Meshes bundle v1.4.1 (GEO-body_male_realistic)
Output: spike_soldier.blend (intermediate), spike_soldier.fbx (Unity),
        preview PNGs (Workbench) at hero distance.

Stages: extract -> rig -> skin -> garment -> materials -> march clip -> export.
Deterministic: no random anywhere.
"""
import bpy
import math
import os
import sys
from mathutils import Vector

SP = "/private/tmp/claude-501/-Users-wmitchell-Documents-jetsons-warface/6273d195-6c7d-4985-96ff-e6d0198315ef/scratchpad"
BUNDLE = f"{SP}/downloads/hbm-bundle/human-base-meshes-bundle-v1.4.1/human_base_meshes_bundle.blend"
OUT_BLEND = f"{SP}/spike_soldier.blend"
OUT_FBX = f"{SP}/spike_soldier.fbx"
PREVIEW_DIR = f"{SP}/spike_previews"
os.makedirs(PREVIEW_DIR, exist_ok=True)

# ---------------------------------------------------------------- extract
bpy.ops.wm.open_mainfile(filepath=BUNDLE)
body = bpy.data.objects["GEO-body_male_realistic"]
eyes = [bpy.data.objects["GEO-body_male_realistic.eye.L"],
        bpy.data.objects["GEO-body_male_realistic.eye.R"]]

# Move to a clean scene: delete every other object.
keep = {body.name} | {e.name for e in eyes}
for ob in list(bpy.data.objects):
    if ob.name not in keep:
        bpy.data.objects.remove(ob, do_unlink=True)

scene = bpy.context.scene
# Make sure our objects are linked to the active collection
for ob in [body] + eyes:
    if ob.name not in scene.collection.objects and not any(
            ob.name in c.objects for c in bpy.data.collections if c.users > 0):
        scene.collection.objects.link(ob)

# Reset object transform so the figure stands at origin.
body.location = (0, 0, 0)
for e in eyes:
    e.location = (0, 0, 0)  # children keep parent-relative placement below

# Drop multires (use base level), join eyes into body.
for ob in [body] + eyes:
    ob.hide_set(False)
    ob.hide_viewport = False
    ob.hide_select = False

bpy.ops.object.select_all(action='DESELECT')
body.select_set(True)
bpy.context.view_layer.objects.active = body
for m in list(body.modifiers):
    body.modifiers.remove(m)

# Eyes are parented to body with their own transforms; clear parent keep transform,
# then zero body-relative offset is already correct (they were modeled in place).
for e in eyes:
    e.select_set(True)
bpy.ops.object.join()
body = bpy.context.view_layer.objects.active
body.name = "SpikeSoldierBody"

H = max(v.co.z for v in body.data.vertices)
print(f"[spike] body joined, verts={len(body.data.vertices)}, H={H:.3f}")

# ---------------------------------------------------------------- armature
# Landmarks measured from mesh analysis (local coords, character faces -Y,
# character-left is +X).  Heights in metres.
def mirror(p):
    return (-p[0], p[1], p[2])

J = {
    "hips":      (0.0,  0.005, 0.96),
    "spine":     (0.0,  0.010, 1.07),
    "chest":     (0.0,  0.000, 1.18),
    "neck":      (0.0, -0.030, 1.42),
    "head":      (0.0, -0.040, 1.50),
    "head_tip":  (0.0, -0.030, 1.684),
    "shoulder.L": (0.025, -0.020, 1.395),
    "uparm.L":   (0.175, -0.010, 1.375),
    "elbow.L":   (0.300, -0.030, 1.060),
    "wrist.L":   (0.405, -0.100, 0.805),
    "hand_tip.L": (0.440, -0.115, 0.715),
    "upleg.L":   (0.098,  0.000, 0.900),
    "knee.L":    (0.115, -0.015, 0.495),
    "ankle.L":   (0.150,  0.050, 0.090),
    "ball.L":    (0.205, -0.075, 0.020),
    "toe_tip.L": (0.245, -0.120, 0.010),
}

arm_data = bpy.data.armatures.new("SpikeRig")
rig = bpy.data.objects.new("SpikeRig", arm_data)
scene.collection.objects.link(rig)
bpy.ops.object.select_all(action='DESELECT')
rig.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode='EDIT')

def add_bone(name, head, tail, parent=None, connect=False):
    b = arm_data.edit_bones.new(name)
    b.head = Vector(head)
    b.tail = Vector(tail)
    if parent is not None:
        b.parent = arm_data.edit_bones[parent]
        b.use_connect = connect
    return b

add_bone("Hips", J["hips"], J["spine"])
add_bone("Spine", J["spine"], J["chest"], "Hips", True)
add_bone("Chest", J["chest"], J["neck"], "Spine", True)
add_bone("Neck", J["neck"], J["head"], "Chest", True)
add_bone("Head", J["head"], J["head_tip"], "Neck", True)

for side, fl in (("L", lambda p: p), ("R", mirror)):
    add_bone(f"Shoulder.{side}", fl(J["shoulder.L"]), fl(J["uparm.L"]), "Chest")
    add_bone(f"UpperArm.{side}", fl(J["uparm.L"]), fl(J["elbow.L"]), f"Shoulder.{side}", True)
    add_bone(f"LowerArm.{side}", fl(J["elbow.L"]), fl(J["wrist.L"]), f"UpperArm.{side}", True)
    add_bone(f"Hand.{side}", fl(J["wrist.L"]), fl(J["hand_tip.L"]), f"LowerArm.{side}", True)
    add_bone(f"UpperLeg.{side}", fl(J["upleg.L"]), fl(J["knee.L"]), "Hips")
    add_bone(f"LowerLeg.{side}", fl(J["knee.L"]), fl(J["ankle.L"]), f"UpperLeg.{side}", True)
    add_bone(f"Foot.{side}", fl(J["ankle.L"]), fl(J["ball.L"]), f"LowerLeg.{side}", True)
    add_bone(f"Toe.{side}", fl(J["ball.L"]), fl(J["toe_tip.L"]), f"Foot.{side}", True)

bpy.ops.object.mode_set(mode='OBJECT')
print(f"[spike] armature bones={len(arm_data.bones)}")

# ---------------------------------------------------------------- skin
bpy.ops.object.select_all(action='DESELECT')
body.select_set(True)
rig.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.parent_set(type='ARMATURE_AUTO')
ng = len(body.vertex_groups)
print(f"[spike] auto weights done, vertex groups={ng}")
if ng == 0:
    print("[spike] FATAL: automatic weights produced no groups")
    sys.exit(2)

# ---------------------------------------------------------------- garment: sack coat
# Duplicate torso + arm faces, offset outward, solidify.  Skirt to upper
# thigh (z>0.72 near body core), sleeves to wrist, exclude head/neck and hands.
me = body.data
sel_polys = set()
for p in me.polygons:
    c = p.center
    ax = abs(c.x)
    on_arm = ax > 0.16 and ax < 0.385 and c.z > 0.72   # sleeve band
    on_torso = ax <= 0.22 and 0.74 < c.z < 1.44        # coat body up to collar
    near_neckline = c.z >= 1.44
    if near_neckline:
        continue
    if on_arm or on_torso:
        sel_polys.add(p.index)

import bmesh
# Build coat object from selected faces
bm = bmesh.new()
bm.from_mesh(me)
bm.faces.ensure_lookup_table()
del_faces = [f for f in bm.faces if f.index not in sel_polys]
bmesh.ops.delete(bm, geom=del_faces, context='FACES')
coat_me = bpy.data.meshes.new("SackCoat")
bm.to_mesh(coat_me)
bm.free()
coat = bpy.data.objects.new("SackCoat", coat_me)
scene.collection.objects.link(coat)
# copy vertex groups (names) — duplicated mesh keeps deform layer only if we copy groups
for vg in body.vertex_groups:
    coat.vertex_groups.new(name=vg.name)
# The bmesh copy preserves the deform layer data by vertex-group index, matching order.

# Offset outward along normals + solidify
disp = coat.modifiers.new("fatten", 'DISPLACE')
disp.strength = 0.012
disp.mid_level = 0.0
solid = coat.modifiers.new("shell", 'SOLIDIFY')
solid.thickness = 0.006
arm_mod = coat.modifiers.new("rig", 'ARMATURE')
arm_mod.object = rig
coat.parent = rig

# ---------------------------------------------------------------- materials
def flat_mat(name, rgba, rough=0.85):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = rgba
    bsdf.inputs["Roughness"].default_value = rough
    m.diffuse_color = rgba
    return m

skin = flat_mat("SpikeSkin", (0.55, 0.38, 0.27, 1.0), 0.6)
wool_navy = flat_mat("SpikeWoolNavy", (0.055, 0.065, 0.12, 1.0), 0.95)
wool_skyblue = flat_mat("SpikeWoolSkyBlue", (0.24, 0.30, 0.42, 1.0), 0.95)
leather_black = flat_mat("SpikeBrogan", (0.03, 0.025, 0.02, 1.0), 0.55)

body.data.materials.clear()
body.data.materials.append(skin)        # 0
body.data.materials.append(wool_skyblue)  # 1 trousers (painted-on for spike)
body.data.materials.append(leather_black) # 2 brogans
for p in body.data.polygons:
    c = p.center
    if c.z < 0.09:
        p.material_index = 2
    elif c.z < 0.93 and abs(c.x) < 0.30 and not (abs(c.x) > 0.28 and c.z > 0.70):
        # legs/hips below belt line -> trousers; hands are x>0.30 at z 0.7-0.85
        if abs(c.x) < 0.22:
            p.material_index = 1
        else:
            p.material_index = 0
    else:
        p.material_index = 0
coat.data.materials.clear()
coat.data.materials.append(wool_navy)
for p in coat.data.polygons:
    p.material_index = 0

# ---------------------------------------------------------------- march clip
# Shoulder-arms march, 110 steps/min => full cycle ~1.09 s.  24 fps -> 26 frames.
scene.render.fps = 24
CYCLE = 26
scene.frame_start = 1
scene.frame_end = CYCLE

bpy.ops.object.select_all(action='DESELECT')
rig.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode='POSE')
pb = rig.pose.bones
for b in pb:
    b.rotation_mode = 'XYZ'

action = bpy.data.actions.new("March_ShoulderArms")
rig.animation_data_create()
rig.animation_data.action = action

D = math.radians

def key(bone, frame, rot=None, loc=None):
    b = pb[bone]
    if rot is not None:
        b.rotation_euler = rot
        b.keyframe_insert("rotation_euler", frame=frame)
    if loc is not None:
        b.location = loc
        b.keyframe_insert("location", frame=frame)

# Static hold: right arm carries the musket at the right shoulder.
# UpperArm hangs near the body, forearm flexed up sharply (holding butt),
# X axis of arm bones roughly along the bone; use crude eulers and check preview.
for f in (1, CYCLE):
    pass

# Pose helper phases: frame 1 = left foot forward contact, 7 = passing,
# 14 = right foot forward contact, 20 = passing, 26 = loop to 1.
def leg_keys(prefix_fwd, prefix_back, f):
    # forward leg: thigh rotated forward (about local X, negative = forward
    # for a bone pointing down when character faces -Y)
    key(f"UpperLeg.{prefix_fwd}", f, rot=(D(-24), 0, 0))
    key(f"LowerLeg.{prefix_fwd}", f, rot=(D(6), 0, 0))
    key(f"Foot.{prefix_fwd}", f, rot=(D(8), 0, 0))
    key(f"UpperLeg.{prefix_back}", f, rot=(D(20), 0, 0))
    key(f"LowerLeg.{prefix_back}", f, rot=(D(28), 0, 0))
    key(f"Foot.{prefix_back}", f, rot=(D(-12), 0, 0))

def passing_keys(f, lift):
    # both legs near vertical, lifted knee on `lift`
    other = "R" if lift == "L" else "L"
    key(f"UpperLeg.{lift}", f, rot=(D(-10), 0, 0))
    key(f"LowerLeg.{lift}", f, rot=(D(38), 0, 0))
    key(f"Foot.{lift}", f, rot=(D(12), 0, 0))
    key(f"UpperLeg.{other}", f, rot=(D(4), 0, 0))
    key(f"LowerLeg.{other}", f, rot=(D(4), 0, 0))
    key(f"Foot.{other}", f, rot=(D(-2), 0, 0))

# contact L fwd (frame 1 and 26 identical for loop)
leg_keys("L", "R", 1)
passing_keys(7, "R")
leg_keys("R", "L", 14)
passing_keys(20, "L")
leg_keys("L", "R", CYCLE)

# pelvis bob: lowest at contact, highest at passing
for f, dz in ((1, -0.015), (7, 0.012), (14, -0.015), (20, 0.012), (CYCLE, -0.015)):
    key("Hips", f, loc=(0, 0, dz))
# slight counter-rotation of hips/chest around Z (bone local Y? crude: use Z)
for f, hr in ((1, 6), (14, -6), (CYCLE, 6)):
    key("Hips", f, rot=(0, 0, D(hr)))
    key("Chest", f, rot=(D(4), 0, D(-hr * 0.8)))
key("Chest", 7, rot=(D(4), 0, 0))
key("Chest", 20, rot=(D(4), 0, 0))

# Arm base poses were IK-solved offline (ik_solve.py) against wrist targets:
# right hand in front of the right shoulder (musket carry), left wrist at
# the left thigh (natural hang).
UA_L = (-26.5, -72.8, 63.9)
LA_L = (-30.6, -4.0, -14.6)
UA_R = (-15.5, -50.7, 43.3)
LA_R = (-137.8, 47.7, 19.4)

# Left arm swings (opposite to left leg) as a delta on the solved hang pose.
for f, sw in ((1, 14), (7, 2), (14, -16), (20, 2), (CYCLE, 14)):
    key("UpperArm.L", f, rot=(D(UA_L[0] + sw), D(UA_L[1]), D(UA_L[2])))
    key("LowerArm.L", f, rot=(D(LA_L[0] - max(0, -sw) * 0.5), D(LA_L[1]), D(LA_L[2])))

# Right arm: static musket carry from the IK solve.
for f in (1, CYCLE):
    key("UpperArm.R", f, rot=(D(UA_R[0]), D(UA_R[1]), D(UA_R[2])))
    key("LowerArm.R", f, rot=(D(LA_R[0]), D(LA_R[1]), D(LA_R[2])))
    key("Hand.R", f, rot=(D(-8), 0, 0))

# head steady
for f in (1, CYCLE):
    key("Neck", f, rot=(D(-4), 0, 0))
    key("Head", f, rot=(D(-2), 0, 0))

# make all fcurves cyclic-friendly linear-ish default (leave bezier)
bpy.ops.object.mode_set(mode='OBJECT')
print(f"[spike] march clip keyed: {CYCLE} frames @24fps, action={action.name}")

# ---------------------------------------------------------------- previews
# Hero-distance Workbench renders at 3 frames.
cam_data = bpy.data.cameras.new("SpikeCam")
cam = bpy.data.objects.new("SpikeCam", cam_data)
scene.collection.objects.link(cam)
cam_data.lens = 50
cam.location = (2.2, -7.5, 1.55)   # ~8 m, slightly camera-left
cam.rotation_euler = (D(87), 0, D(16))
scene.camera = cam

sun_data = bpy.data.lights.new("SpikeSun", 'SUN')
sun_data.energy = 4.0
sun = bpy.data.objects.new("SpikeSun", sun_data)
scene.collection.objects.link(sun)
sun.rotation_euler = (D(50), 0, D(-120))

# ground plane
bpy.ops.mesh.primitive_plane_add(size=40, location=(0, 0, 0))
ground = bpy.context.active_object
ground.name = "Ground"
gm = flat_mat("SpikeGround", (0.25, 0.22, 0.14, 1.0), 1.0)
ground.data.materials.append(gm)

scene.render.engine = 'BLENDER_WORKBENCH'
scene.display.shading.light = 'STUDIO'
scene.display.shading.color_type = 'MATERIAL'
scene.render.resolution_x = 1280
scene.render.resolution_y = 720
for f in (1, 7, 14, 20):
    scene.frame_set(f)
    scene.render.filepath = f"{PREVIEW_DIR}/spike_f{f:02d}.png"
    bpy.ops.render.render(write_still=True)
    print(f"[spike] preview {scene.render.filepath}")

# close-up preview (2.5 m)
cam.location = (0.8, -2.4, 1.5)
cam.rotation_euler = (D(88), 0, D(18))
scene.frame_set(1)
scene.render.filepath = f"{PREVIEW_DIR}/spike_close_f01.png"
bpy.ops.render.render(write_still=True)

# ---------------------------------------------------------------- save + export
bpy.ops.wm.save_as_mainfile(filepath=OUT_BLEND)

bpy.ops.object.select_all(action='DESELECT')
for ob in (body, coat, rig):
    ob.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.export_scene.fbx(
    filepath=OUT_FBX,
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
print(f"[spike] exported {OUT_FBX}")
print("[spike] DONE")
