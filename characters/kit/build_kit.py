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

# Deterministic body variants (§6.4 discipline: constants, not random).
VARIANTS = {
    "union_a": {"faction": "union", "hat": "kepi",
                "macros": {"height": 0.55, "weight": 0.50, "muscle": 0.58, "age": 0.42}},
    "csa_a":   {"faction": "csa", "hat": "slouch", "coat_key": "coat",
                "macros": {"height": 0.52, "weight": 0.44, "muscle": 0.55, "age": 0.38}},
}


def clear_scene():
    # purge objects/data without reloading the homefile: reloading unloads
    # the MPFB extension state and silently breaks macro application
    bpy.ops.object.select_all(action='DESELECT')
    for ob in list(bpy.data.objects):
        bpy.data.objects.remove(ob, do_unlink=True)
    for coll in (bpy.data.meshes, bpy.data.armatures, bpy.data.actions,
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
        g.haversack(body, rig, lm, pal),
        g.canteen(body, rig, lm, pal),
        g.bayonet_scabbard(body, rig, lm, pal),
        g.strap(body, rig, lm, pal, "cartsling", from_shoulder='l'),
    ]
    if spec["hat"] == "kepi":
        parts.append(g.kepi(body, rig, lm, pal))
    else:
        parts.append(g.slouch_hat(body, rig, lm, pal))
    if spec["faction"] == "csa":
        parts.append(g.blanket_roll(body, rig, lm, pal))

    # skin material on the body itself
    skin = sf.flat_mat("skin", (0.62, 0.45, 0.34, 1.0), 0.6)
    g.set_mat(body, skin)

    musket = mk.build_musket(f"{name}_musket")
    mk.attach_musket_to_rig(musket, rig)

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


def export_fbx(name, body, rig, parts, musket):
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
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=True,
        bake_anim_force_startend_keying=True,
        armature_nodetype='NULL',
    )
    print(f"[kit] exported {path}")


def main():
    previews_only = "--previews-only" in sys.argv
    sf.enable_mpfb()
    for name, spec in VARIANTS.items():
        clear_scene()
        body, rig, parts, musket, lm = build_variant(name, spec)
        if not previews_only:
            import clips
            clips.author_all(rig, lm)
        preview(name, rig, lm)
        bpy.ops.wm.save_as_mainfile(filepath=f"{OUT}/{name}.blend")
        if not previews_only:
            export_fbx(name, body, rig, parts, musket)
    print("[kit] DONE")


if __name__ == "__main__":
    main()
