"""Phase 6 character kit — build orchestrator.

Usage (from characters/kit):
  <bpy-venv>/bin/python build_kit.py [--previews-only]

Builds one Union and one Confederate modular soldier (plan §7.1
silhouette-critical items), the project-owned Springfield 1861, saves
.blend sources, renders Workbench preview sheets, and (unless
--previews-only) exports Unity FBX per variant with all authored clips.

Deterministic: macros and geometry are constants; no random anywhere.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy  # noqa: E402
import math  # noqa: E402
from mathutils import Vector  # noqa: E402

import soldier_factory as sf  # noqa: E402
import garments as g  # noqa: E402
import musket as mk  # noqa: E402

D = math.radians
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.environ.get("KIT_OUT", os.path.join(HERE, "out"))
os.makedirs(OUT, exist_ok=True)

# Deterministic body/equipment/uniform variants (§6.4 discipline:
# constants, not random). Restrained, historically reviewed: Union stays
# in dark-navy sack coats and sky-blue kersey (kepi standard, one slouch —
# both were in Federal use in 1863); Confederate variation stays inside
# the gray/butternut jean-cloth families (NOT random earth tones), with
# bedrolls common and knapsack-less campaign kit. Equipment presence
# varies per variant the way campaign photographs support: everyone keeps
# cartridge box, cap pouch, and belt; haversack/canteen/bedroll vary.
VARIANTS = {
    "union_a": {"faction": "union", "hat": "kepi",
                "macros": {"height": 0.55, "weight": 0.50, "muscle": 0.58, "age": 0.42}},
    "union_b": {"faction": "union", "hat": "kepi", "blanket_roll": True,
                "macros": {"height": 0.62, "weight": 0.56, "muscle": 0.52, "age": 0.55}},
    "union_c": {"faction": "union", "hat": "slouch", "no_haversack": True,
                "macros": {"height": 0.48, "weight": 0.42, "muscle": 0.62, "age": 0.30}},
    "csa_a":   {"faction": "csa", "hat": "slouch", "coat_key": "coat",
                "macros": {"height": 0.52, "weight": 0.44, "muscle": 0.55, "age": 0.38}},
    "csa_b":   {"faction": "csa", "hat": "slouch", "coat_key": "coat_butternut",
                "no_haversack": True,
                "macros": {"height": 0.58, "weight": 0.40, "muscle": 0.60, "age": 0.48}},
    "csa_c":   {"faction": "csa", "hat": "kepi", "coat_key": "coat",
                "no_blanket_roll": True,
                "macros": {"height": 0.46, "weight": 0.48, "muscle": 0.50, "age": 0.26}},
}


def clear_scene():
    # purge objects/data without reloading the homefile: reloading unloads
    # the MPFB extension state and silently breaks macro application
    bpy.ops.object.select_all(action='DESELECT')
    for ob in list(bpy.data.objects):
        bpy.data.objects.remove(ob, do_unlink=True)
    # actions purge unconditionally: each variant re-authors its clips on
    # its own rig, and use_all_actions at export would otherwise bake a
    # previous variant's leftovers
    for x in list(bpy.data.actions):
        bpy.data.actions.remove(x)
    for coll in (bpy.data.meshes, bpy.data.armatures,
                 bpy.data.cameras, bpy.data.lights):
        for x in list(coll):
            if x.users == 0:
                coll.remove(x)


def build_variant(name, spec):
    pal = g.UNION if spec["faction"] == "union" else dict(g.CSA)
    if spec.get("coat_key") == "coat_butternut":
        pal["coat"] = g.CSA["coat_butternut"]
    body, rig = sf.make_body(name, spec["macros"])
    lm = g.Landmarks(body, rig)

    style = "sack" if spec["faction"] == "union" else "shell"
    parts = [
        g.coat(body, rig, lm, pal, style, arm_limit=1.0),
        g.trousers(body, rig, lm, pal),
        g.brogans(body, rig, lm, pal),
        g.cartridge_box(body, rig, lm, pal),
        g.cap_pouch(body, rig, lm, pal),
        g.belt(body, rig, lm, pal),
        g.canteen(body, rig, lm, pal),
        g.bayonet_scabbard(body, rig, lm, pal),
        g.strap(body, rig, lm, pal, "cartsling", from_shoulder='l'),
    ]
    if not spec.get("no_haversack"):
        parts.append(g.haversack(body, rig, lm, pal))
    if spec["hat"] == "kepi":
        parts.append(g.kepi(body, rig, lm, pal))
    else:
        parts.append(g.slouch_hat(body, rig, lm, pal))
    wants_roll = (spec["faction"] == "csa" and not spec.get("no_blanket_roll")) \
        or spec.get("blanket_roll")
    if wants_roll:
        parts.append(g.blanket_roll(body, rig, lm, pal))

    # all garments are extracted; now permanently delete the body faces
    # they cover (defect-1 fix: covered skin can never poke through)
    g.mask_covered_body(body, lm, style)

    # skin material on the body itself; the collar plug reads as shirt
    skin = sf.flat_mat("skin", (0.62, 0.45, 0.34, 1.0), 0.6)
    g.set_mat(body, skin)
    g.collar_shirt(body, lm, pal)

    musket, ramrod = mk.build_musket(f"{name}_musket")
    mk.attach_musket_to_rig(musket, ramrod, rig)
    parts.append(ramrod)

    return body, rig, parts, musket, lm


def preview(name, rig, lm, frame=None):
    scene = bpy.context.scene
    for m in bpy.data.materials:
        if m.use_nodes and "Principled BSDF" in m.node_tree.nodes:
            m.diffuse_color = m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value[:]
    cam_d = bpy.data.cameras.new("pc")
    cam = bpy.data.objects.new("pc", cam_d)
    scene.collection.objects.link(cam)
    cam_d.lens = 55
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
    scene.render.resolution_y = 1400
    if frame is not None:
        scene.frame_set(frame)
    views = (("front34", (1.9, -3.1, 1.5), (0.0, 0.0, 1.05)),
             ("back34", (-2.2, 2.8, 1.6), (0.0, 0.0, 1.05)),
             ("close", (0.75, -1.35, 1.42), (0.0, 0.0, 1.30)))
    for tag, loc, look in views:
        cam.location = loc
        d = Vector(look) - Vector(loc)
        cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = f"{OUT}/preview_{name}_{tag}.png"
        bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(cam, do_unlink=True)


def clip_previews(name, rig):
    """Workbench contact frames for every authored clip (review evidence).
    4 frames per clip from a fixed 3/4 hero-distance camera."""
    scene = bpy.context.scene
    for m in bpy.data.materials:
        if m.use_nodes and "Principled BSDF" in m.node_tree.nodes:
            m.diffuse_color = m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value[:]
    cam_d = bpy.data.cameras.new("cc")
    cam = bpy.data.objects.new("cc", cam_d)
    scene.collection.objects.link(cam)
    cam_d.lens = 45
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
    scene.render.resolution_x = 900
    scene.render.resolution_y = 900
    # wide enough that lying/settled fall poses stay in frame
    cam.location = (3.1, -4.0, 1.8)
    d = Vector((0.0, 0.1, 0.65)) - Vector(cam.location)
    cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    for act in bpy.data.actions:
        rig.animation_data.action = act
        f0, f1 = act.frame_range
        for i, f in enumerate(
                sorted({int(f0), int(f0 + (f1 - f0) * 0.33),
                        int(f0 + (f1 - f0) * 0.66), int(f1)})):
            scene.frame_set(f)
            scene.render.filepath = f"{OUT}/clip_{name}_{act.name}_{i}_{f:03d}.png"
            bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(cam, do_unlink=True)


def export_fbx(name, body, rig, parts, musket, anim=True):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in [body, rig, musket] + parts:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = rig
    path = f"{OUT}/{name}.fbx"
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        object_types={'ARMATURE', 'MESH'},
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        bake_anim=anim,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=True,
        bake_anim_force_startend_keying=True,
        armature_nodetype='NULL',
    )
    print(f"[kit] exported {path}")


def export_near_tier(name, body, rig, parts, musket):
    """Near tier (§7.4, 25-100 m): same skeleton (so the hero clips sample
    onto it in Unity), meshes decimated. No baked animation of its own."""
    meshes = [body] + [p for p in parts] + [musket]
    mods = []
    for ob in meshes:
        m = ob.modifiers.new("neardec", 'DECIMATE')
        m.ratio = 0.35
        # decimate must run BEFORE the armature modifier
        with bpy.context.temp_override(object=ob, active_object=ob):
            bpy.ops.object.modifier_move_to_index(modifier="neardec", index=0)
        mods.append((ob, m))
    export_fbx(f"{name}_near", body, rig, parts, musket, anim=False)
    for ob, m in mods:
        ob.modifiers.remove(m)


# Mid-tier pose set (§7.4, 100-350 m): the poses a distant figure must
# read — march contact/passing, advance, aim/fire, reload (rod high),
# and two persistent casualty poses.
MID_POSES = (
    ("march_a", "March_ShoulderArms", 0.0),
    ("march_b", "March_ShoulderArms", 0.5),
    ("aim", "Aim_Musket", 1.3),
    ("fire", "Fire_Recoil", 0.10),
    ("reload_rod", "Reload_Musket", 10.6),
    ("fallen_back", "Fall_Shot_Front_Back", 1.9),
    ("fallen_side", "Fall_Shot_Left_Side", 1.6),
)


def export_mid_tier(name, rig, body, parts, musket):
    """Bake MID_POSES into static joined decimated meshes (one object per
    pose, materials preserved as submeshes) — RenderMeshInstanced-ready
    for the Phase 8 crowd renderer."""
    import clips
    scene = bpy.context.scene
    pose_obs = []
    meshes = [body] + parts + [musket]
    for tag, act_name, t in MID_POSES:
        rig.animation_data.action = bpy.data.actions[act_name]
        scene.frame_set(clips.frame(t))
        dg = bpy.context.evaluated_depsgraph_get()
        joined = None
        for src in meshes:
            ev = src.evaluated_get(dg)
            me = bpy.data.meshes.new_from_object(
                ev, preserve_all_data_layers=False, depsgraph=dg)
            ob = bpy.data.objects.new(f"tmp_{src.name}", me)
            scene.collection.objects.link(ob)
            ob.matrix_world = src.matrix_world.copy()
            if joined is None:
                joined = ob
            else:
                bpy.ops.object.select_all(action='DESELECT')
                joined.select_set(True)
                ob.select_set(True)
                bpy.context.view_layer.objects.active = joined
                bpy.ops.object.join()
        joined.name = f"pose_{tag}"
        dec = joined.modifiers.new("dec", 'DECIMATE')
        dec.ratio = 0.15
        pose_obs.append(joined)
    bpy.ops.object.select_all(action='DESELECT')
    for ob in pose_obs:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = pose_obs[0]
    path = f"{OUT}/{name}_mid.fbx"
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True,
        apply_scale_options='FBX_SCALE_ALL',
        object_types={'MESH'}, use_mesh_modifiers=True, bake_anim=False)
    print(f"[kit] exported {path}")
    for ob in pose_obs:
        bpy.data.objects.remove(ob, do_unlink=True)


def main():
    previews_only = "--previews-only" in sys.argv
    clip_prev = "--clip-previews" in sys.argv
    only = None
    if "--only" in sys.argv:
        only = sys.argv[sys.argv.index("--only") + 1]
    sf.enable_mpfb()
    for name, spec in VARIANTS.items():
        if only and name != only:
            continue
        clear_scene()
        body, rig, parts, musket, lm = build_variant(name, spec)
        if not previews_only:
            import clips
            clips.author_all(rig, lm)
        preview(name, rig, lm)
        if clip_prev and not previews_only:
            clip_previews(name, rig)
        bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/{name}.blend")
        if not previews_only:
            export_fbx(name, body, rig, parts, musket)
            if name.endswith("_a"):   # one LOD set per faction
                export_mid_tier(name, rig, body, parts, musket)
            export_near_tier(name, body, rig, parts, musket)
    print("[kit] DONE")


if __name__ == "__main__":
    main()
