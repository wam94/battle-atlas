#!/usr/bin/env python3
"""Project-owned environment props for the Angle (plan §12 Phase 7).

Everything here is modeled procedurally in bpy from public-domain dimensions:

- 3-inch Ordnance rifle, Model 1861 — tube 69 in / bore 3 in / No. 1 field
  carriage, 57 in wheels on a 60 in track (Ordnance Manual 1862 pp. 9, 17-19,
  74; `claim-ordnance-rifle-dimensions`), in intact / disabled / wrecked
  states (ED-16).
- Limber and caisson (Ordnance Manual 1862 pp. 45, 47; Instruction for Field
  Artillery 1863 pp. 2-3; `claim-battery-organization`).
- Ammunition chest and wrecked-wheel detritus (`claim-corridor-trampled`).
- Codori house and barn massing per ED-13 (reconstruction-grade silhouettes;
  HABS Trostle PA-1962 as the bank-barn analogue, LOC 2012647714 photo as the
  plain-massing reference).

Run with the characters/kit bpy venv:
  <venv>/bin/python environment/props/build_props.py <out-dir>

Deterministic: fixed dimensions, no randomness. Blender frame: Z-up, props
face -Y (imports into Unity facing +Z at yaw 0). Material slots are named
(gun_iron, carriage_wood, chest_wood, siding, roof, stone, dark_wood); the
Unity stage binds them to the Phase 7 palette.
"""
import math
import sys

import bpy
import bmesh
from mathutils import Matrix, Vector

IN = 0.0254

WHEEL_DIA = 57 * IN          # OM 1862 p. 74
TRACK = 60 * IN              # OM 1862 p. 74
TUBE_LEN = 69 * IN           # OM 1862 pp. 17-19 via antietam-aotw
AXLE_H = WHEEL_DIA / 2


def _mat(name):
    m = bpy.data.materials.get(name)
    if m is None:
        m = bpy.data.materials.new(name)
    return m


def _new_obj(name, mat_name):
    mesh = bpy.data.meshes.new(name)
    ob = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(ob)
    ob.data.materials.append(_mat(mat_name))
    return ob


def _bm_to(ob, bm):
    for f in bm.faces:
        f.material_index = 0
    bm.to_mesh(ob.data)
    bm.free()


def box(name, mat, size, loc, rot=None):
    ob = _new_obj(name, mat)
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    mx = Matrix.LocRotScale(Vector(loc), rot, Vector(size))
    bmesh.ops.transform(bm, matrix=mx, verts=bm.verts)
    _bm_to(ob, bm)
    return ob


def cylinder(name, mat, radius, depth, loc, rot=None, segments=14):
    ob = _new_obj(name, mat)
    bm = bmesh.new()
    bmesh.ops.create_cone(
        bm, cap_ends=True, segments=segments,
        radius1=radius, radius2=radius, depth=depth)
    mx = Matrix.LocRotScale(Vector(loc), rot, Vector((1, 1, 1)))
    bmesh.ops.transform(bm, matrix=mx, verts=bm.verts)
    _bm_to(ob, bm)
    return ob


def cone(name, mat, r1, r2, depth, loc, rot=None, segments=14):
    ob = _new_obj(name, mat)
    bm = bmesh.new()
    bmesh.ops.create_cone(
        bm, cap_ends=True, segments=segments,
        radius1=r1, radius2=r2, depth=depth)
    mx = Matrix.LocRotScale(Vector(loc), rot, Vector((1, 1, 1)))
    bmesh.ops.transform(bm, matrix=mx, verts=bm.verts)
    _bm_to(ob, bm)
    return ob


ROT_X90 = Matrix.Rotation(math.pi / 2, 4, "X").to_quaternion()
ROT_Y90 = Matrix.Rotation(math.pi / 2, 4, "Y").to_quaternion()


def wheel(name, x, y, z=AXLE_H, lean=None):
    """57-inch field wheel: rim, hub, 14 spokes (axis along X)."""
    obs = []
    rot = ROT_Y90 if lean is None else lean @ ROT_Y90
    center = Vector((x, y, z))

    rim = _new_obj(name + "_rim", "wheel_wood")
    bm = bmesh.new()
    bmesh.ops.create_circle(bm, cap_ends=False, segments=24, radius=WHEEL_DIA / 2 - 0.03)
    for e in list(bm.edges):
        pass
    # extrude the circle into a band, then solidify-ish by scaling a copy
    ret = bmesh.ops.extrude_edge_only(bm, edges=list(bm.edges))
    verts = [v for v in ret["geom"] if isinstance(v, bmesh.types.BMVert)]
    bmesh.ops.translate(bm, vec=(0, 0, 0.07), verts=verts)
    bmesh.ops.solidify(bm, geom=list(bm.faces), thickness=0.05)
    mx = Matrix.LocRotScale(center - Vector((0, 0, 0)), rot, Vector((1, 1, 1)))
    bmesh.ops.translate(bm, vec=(0, 0, -0.035), verts=list(bm.verts))
    bmesh.ops.transform(bm, matrix=mx, verts=bm.verts)
    _bm_to(rim, bm)
    obs.append(rim)

    obs.append(cylinder(name + "_hub", "wheel_wood", 0.075, 0.24,
                        center, rot=rot, segments=10))
    for i in range(14):
        a = i * 2 * math.pi / 14
        r = WHEEL_DIA / 2 - 0.05
        mid = center + rot @ Vector((math.cos(a) * r / 2, math.sin(a) * r / 2, 0))
        spoke_rot = rot @ Matrix.Rotation(a + math.pi / 2, 4, "Z").to_quaternion() @ ROT_X90
        obs.append(cylinder(name + f"_spoke{i}", "wheel_wood", 0.018, r,
                            mid, rot=spoke_rot, segments=6))
    return obs


def gun_tube(name, y_breech, z, pitch=0.0):
    """69-inch wrought-iron tube, muzzle toward -Y."""
    obs = []
    rot = Matrix.Rotation(math.pi / 2 + pitch, 4, "X").to_quaternion()
    center = Vector((0, y_breech - TUBE_LEN / 2 * math.cos(pitch),
                     z + TUBE_LEN / 2 * math.sin(pitch)))
    obs.append(cone(name + "_tube", "gun_iron", 0.075, 0.048, TUBE_LEN,
                    center, rot=rot, segments=16))
    muzzle = Vector((0, y_breech - TUBE_LEN * math.cos(pitch),
                     z + TUBE_LEN * math.sin(pitch)))
    obs.append(cylinder(name + "_muzzleswell", "gun_iron", 0.058, 0.10,
                        muzzle + Vector((0, 0.05, 0)), rot=rot, segments=16))
    obs.append(cylinder(name + "_knob", "gun_iron", 0.045, 0.12,
                        Vector((0, y_breech + 0.05, z)), rot=rot, segments=10))
    # trunnions at 1/3 from breech
    ty = y_breech - TUBE_LEN * 0.38
    obs.append(cylinder(name + "_trunnion", "gun_iron", 0.037, 0.42,
                        Vector((0, ty, z)), rot=ROT_Y90, segments=8))
    return obs, ty


def carriage(name, tilt=0.0, missing_wheel=False):
    """No. 1 field carriage: cheeks, axle, trail, two 57-in wheels."""
    obs = []
    tube_z = AXLE_H + 0.10
    # axle
    obs.append(cylinder(name + "_axle", "gun_iron", 0.045, TRACK + 0.15,
                        Vector((0, 0, AXLE_H)), rot=ROT_Y90, segments=8))
    # cheeks flanking the tube
    for side in (-1, 1):
        obs.append(box(name + f"_cheek{side}", "carriage_wood",
                       (0.09, 0.85, 0.38),
                       (side * 0.17, -0.05, AXLE_H + 0.08)))
    # single-piece trail stock to the ground, ~2.4 m back
    trail_rot = Matrix.Rotation(-0.30, 4, "X").to_quaternion()
    obs.append(box(name + "_trail", "carriage_wood",
                   (0.16, 2.45, 0.22),
                   (0, 1.15, AXLE_H - 0.34), rot=trail_rot))
    obs.append(cylinder(name + "_elev", "gun_iron", 0.03, 0.30,
                        Vector((0, 0.45, AXLE_H - 0.02)), segments=8))
    if missing_wheel:
        obs += wheel(name + "_wheelL", -TRACK / 2, 0)
        lean = Matrix.Rotation(1.35, 4, "Y").to_quaternion()
        obs += wheel(name + "_wheelR_fallen", TRACK / 2 + 0.5, 0.3,
                     z=0.12, lean=lean)
    else:
        obs += wheel(name + "_wheelL", -TRACK / 2, 0)
        obs += wheel(name + "_wheelR", TRACK / 2, 0)
    if tilt:
        rot = Matrix.Rotation(tilt, 4, "Y")
        for ob in obs:
            ob.matrix_world = rot @ ob.matrix_world
    return obs, tube_z


def ordnance_rifle(state="intact"):
    if state == "wrecked":
        # dismounted tube on the ground beside a broken carriage
        tube, _ = gun_tube("wreck", y_breech=0.9, z=0.09)
        rot = Matrix.Rotation(0.55, 4, "Z")
        for ob in tube:
            ob.matrix_world = rot @ ob.matrix_world
        obs, _ = carriage("wreckcarr", tilt=0.28, missing_wheel=True)
        return tube + obs
    tilt = 0.12 if state == "disabled" else 0.0
    obs, tube_z = carriage("carr", tilt=tilt,
                           missing_wheel=(state == "disabled"))
    tube, ty = gun_tube("gun", y_breech=0.55, z=tube_z,
                        pitch=0.03 if state == "intact" else -0.02)
    if state == "disabled":
        rot = Matrix.Rotation(0.12, 4, "Y")
        for ob in tube:
            ob.matrix_world = rot @ ob.matrix_world
    return obs + tube


def ammo_chest(name="chest", loc=(0, 0, 0)):
    """Field ammunition chest (limber chest form, OM 1862 Pl. 2)."""
    obs = [
        box(name + "_body", "chest_wood", (1.02, 0.45, 0.36),
            (loc[0], loc[1], loc[2] + 0.18)),
        box(name + "_lid", "dark_wood", (1.06, 0.49, 0.05),
            (loc[0], loc[1], loc[2] + 0.385)),
    ]
    return obs


def limber():
    obs = []
    obs.append(cylinder("limb_axle", "gun_iron", 0.045, TRACK + 0.15,
                        Vector((0, 0, AXLE_H)), rot=ROT_Y90, segments=8))
    obs += wheel("limb_wheelL", -TRACK / 2, 0)
    obs += wheel("limb_wheelR", TRACK / 2, 0)
    obs.append(box("limb_frame", "carriage_wood", (0.9, 0.7, 0.08),
                   (0, 0, AXLE_H + 0.10)))
    obs += ammo_chest("limb_chest", (0, 0, AXLE_H + 0.14))
    # pole toward -Y (team gone: pole dropped to the ground)
    pole_rot = Matrix.Rotation(0.36, 4, "X").to_quaternion()
    obs.append(cylinder("limb_pole", "carriage_wood", 0.035, 2.9,
                        Vector((0, -1.55, AXLE_H / 2 + 0.05)),
                        rot=pole_rot @ ROT_X90, segments=8))
    return obs


def caisson():
    obs = limber()
    # caisson body trails behind with two more chests and a spare wheel
    obs.append(box("cais_stock", "carriage_wood", (0.18, 2.3, 0.14),
                   (0, 1.4, AXLE_H - 0.05)))
    obs += ammo_chest("cais_chest1", (0, 1.0, AXLE_H + 0.02))
    obs += ammo_chest("cais_chest2", (0, 1.65, AXLE_H + 0.02))
    lean = Matrix.Rotation(1.15, 4, "X").to_quaternion()
    obs += wheel("cais_spare", 0, 2.55, z=0.55, lean=lean)
    return obs


def wheel_wreck():
    lean = Matrix.Rotation(1.45, 4, "Y").to_quaternion()
    obs = wheel("wreck_flat", 0, 0, z=0.10, lean=lean)
    obs.append(box("wreck_plank1", "dark_wood", (0.12, 1.1, 0.06),
                   (0.5, 0.3, 0.05), rot=Matrix.Rotation(0.5, 4, "Z").to_quaternion()))
    obs.append(box("wreck_plank2", "dark_wood", (0.10, 0.8, 0.06),
                   (-0.4, -0.2, 0.04), rot=Matrix.Rotation(-0.9, 4, "Z").to_quaternion()))
    return obs


def gable_building(name, w, d, eaves, ridge_extra, siding, roof_mat,
                   chimney=False, forebay=0.0, stone_base=0.0):
    """Simple gabled block: long axis X, gable ends +/-X, faces -Y at yaw 0."""
    obs = []
    if stone_base > 0:
        obs.append(box(name + "_base", "stone", (w, d, stone_base),
                       (0, 0, stone_base / 2)))
    body_h = eaves - stone_base
    obs.append(box(name + "_body", siding, (w, d, body_h),
                   (0, 0, stone_base + body_h / 2)))
    if forebay > 0:
        obs.append(box(name + "_forebay", siding, (w, forebay, body_h * 0.45),
                       (0, -(d / 2 + forebay / 2), eaves - body_h * 0.45 / 2)))
    # gable prism
    ob = _new_obj(name + "_gable", siding)
    bm = bmesh.new()
    hw, hd = w / 2, d / 2 + (forebay if forebay > 0 else 0) / 2
    ridge = eaves + ridge_extra
    v = [bm.verts.new(p) for p in [
        (-hw, -hd, eaves), (hw, -hd, eaves), (hw, hd, eaves), (-hw, hd, eaves),
        (-hw, 0, ridge), (hw, 0, ridge)]]
    bm.faces.new((v[0], v[1], v[5], v[4]))
    bm.faces.new((v[2], v[3], v[4], v[5]))
    bm.faces.new((v[0], v[4], v[3]))
    bm.faces.new((v[1], v[2], v[5]))
    _bm_to(ob, bm)
    obs.append(ob)
    # roof slabs
    slope = math.atan2(ridge_extra, hd)
    slab_len = math.hypot(hd, ridge_extra) + 0.25
    for side in (-1, 1):
        # rotate so the slab descends from the ridge (y=0) to the eaves
        # (y=side*hd): about +X, positive rotation lifts +Y, so negate.
        rot = Matrix.Rotation(-side * slope, 4, "X").to_quaternion()
        mid_y = side * hd / 2
        mid_z = eaves + ridge_extra / 2 + 0.06
        obs.append(box(name + f"_roof{side}", roof_mat,
                       (w + 0.35, slab_len, 0.09), (0, mid_y, mid_z), rot=rot))
    obs.append(box(name + "_ridgecap", roof_mat, (w + 0.35, 0.30, 0.10),
                   (0, 0, ridge + 0.08)))
    if chimney:
        obs.append(box(name + "_chimney", "stone", (0.55, 0.55, 1.6),
                       (w / 2 - 0.5, 0.4, ridge + 0.35)))
    # door + window boards (dark inserts proud of the -Y face)
    face_y = -(d / 2 + (forebay if forebay > 0 else 0)) - 0.02
    obs.append(box(name + "_door", "dark_wood", (0.95, 0.06, 2.0),
                   (-w / 4, face_y, stone_base + 1.0)))
    for i, wx in enumerate((w / 4, w / 4 + 1.4, -w / 4 - 1.4)):
        if abs(wx) < w / 2 - 0.6:
            obs.append(box(name + f"_win{i}", "dark_wood", (0.7, 0.06, 1.0),
                           (wx, face_y, eaves * 0.62)))
    return obs


def codori_house():
    # ED-13: two-story frame block 9.0 x 7.5 m, eaves 5.5 m
    return gable_building("house", 9.0, 7.5, 5.5, 2.4,
                          "siding", "roof", chimney=True)


def codori_barn():
    # ED-13: PA bank barn massing 18 x 11 m, eaves 6.5 m, forebay, stone base
    return gable_building("barn", 18.0, 11.0, 6.5, 3.2,
                          "barn_wood", "roof", forebay=1.8, stone_base=2.0)


PROPS = {
    "ordnance_rifle": lambda: ordnance_rifle("intact"),
    "ordnance_rifle_disabled": lambda: ordnance_rifle("disabled"),
    "ordnance_rifle_wrecked": lambda: ordnance_rifle("wrecked"),
    "limber": limber,
    "caisson": caisson,
    "ammo_chest": lambda: ammo_chest(),
    "wheel_wreck": wheel_wreck,
    "codori_house": codori_house,
    "codori_barn": codori_barn,
}


def export(name, out_dir):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    obs = PROPS[name]()
    # join into one object for a clean single-mesh FBX
    for ob in obs:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    bpy.ops.object.join()
    joined = bpy.context.view_layer.objects.active
    joined.name = name
    # box-project UVs so the palette textures tile sanely
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.cube_project(cube_size=1.0)
    bpy.ops.object.mode_set(mode="OBJECT")
    path = f"{out_dir}/{name}.fbx"
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True,
        apply_scale_options="FBX_SCALE_ALL",
        object_types={"MESH"}, use_mesh_modifiers=True, bake_anim=False,
        path_mode="STRIP")
    tris = sum(len(p.vertices) - 2 for p in joined.data.polygons)
    print(f"[props] {name}: {tris} tris -> {path}")


def main():
    out_dir = sys.argv[-1]
    for name in PROPS:
        export(name, out_dir)
    print("[props] DONE")


if __name__ == "__main__":
    main()
