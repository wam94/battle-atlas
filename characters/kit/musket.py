"""Project-owned Springfield Model 1861 rifle-musket (plan §7.2 fallback).

The plan's Sketchfab CC-BY candidate is login-gated (Sketchfab downloads
require an account), so per the executor instructions the musket is
modeled from public-domain reference dimensions instead:

  overall length 56 in (1.422 m), barrel 40 in (1.016 m), ~9 lb;
  full-length walnut stock, three barrel bands, right-side percussion
  lock with hammer, steel ramrod seated in a channel under the barrel.

Local frame: origin at the butt plate center; +Y runs butt -> muzzle
(matches the prop bone axis), +Z is the top of the barrel when the musket
lies lock-side right. Low/mid poly (~2k tris) — hero-distance prop.
"""
import math

import bmesh
import bpy
from mathutils import Matrix, Vector

from soldier_factory import flat_mat

D = math.radians

TOTAL_LEN = 1.422
BARREL_LEN = 1.016
BREECH_Y = TOTAL_LEN - BARREL_LEN   # barrel starts here


def _cyl(bm, r1, r2, y0, y1, z=0.0, segments=10):
    ret = bmesh.ops.create_cone(bm, cap_ends=True, segments=segments,
                                radius1=r1, radius2=r2, depth=(y1 - y0))
    mat = Matrix.Translation(Vector((0, (y0 + y1) / 2, z))) @ Matrix.Rotation(D(-90), 4, 'X')
    bmesh.ops.transform(bm, matrix=mat, verts=ret["verts"])
    return ret


def _boxat(bm, size, center):
    ret = bmesh.ops.create_cube(bm, size=1.0)
    for v in ret["verts"]:
        v.co.x *= size[0]
        v.co.y *= size[1]
        v.co.z *= size[2]
    bmesh.ops.translate(bm, verts=ret["verts"], vec=center)
    return ret


def build_musket(name="Springfield1861"):
    """Returns (object, per-material face assignment done)."""
    walnut = flat_mat("musket_walnut", (0.190, 0.105, 0.055, 1.0), 0.55)
    steel = flat_mat("musket_steel", (0.300, 0.300, 0.310, 1.0), 0.35)
    brass = flat_mat("musket_brass", (0.520, 0.400, 0.130, 1.0), 0.4)

    bm_wood = bmesh.new()
    # butt stock: tapered box with comb drop
    _boxat(bm_wood, (0.020, 0.115, 0.062), (0, 0.058, -0.028))
    # wrist
    _cyl(bm_wood, 0.020, 0.017, 0.11, 0.28, z=-0.012, segments=8)
    # fore stock: full length to near the muzzle
    _cyl(bm_wood, 0.016, 0.0135, 0.28, TOTAL_LEN - 0.075, z=-0.006, segments=8)

    bm_metal = bmesh.new()
    # barrel: tapers toward the muzzle, sits above the fore stock
    _cyl(bm_metal, 0.0105, 0.0080, BREECH_Y - 0.06, TOTAL_LEN, z=0.012, segments=10)
    # bolster + hammer block on the right side (-x is left when +Y muzzle... lock on right = +x)
    _boxat(bm_metal, (0.012, 0.030, 0.014), (0.016, BREECH_Y - 0.02, 0.004))
    _boxat(bm_metal, (0.006, 0.012, 0.024), (0.020, BREECH_Y - 0.035, 0.020))  # hammer spur
    # trigger guard bow
    _boxat(bm_metal, (0.006, 0.055, 0.004), (0, 0.30, -0.040))
    _boxat(bm_metal, (0.006, 0.004, 0.012), (0, 0.272, -0.032))
    _boxat(bm_metal, (0.006, 0.004, 0.012), (0, 0.328, -0.032))
    # ramrod under the barrel, head just short of the muzzle
    _cyl(bm_metal, 0.0038, 0.0038, 0.32, TOTAL_LEN - 0.012, z=-0.0205, segments=6)
    # front sight
    _boxat(bm_metal, (0.002, 0.006, 0.004), (0, TOTAL_LEN - 0.02, 0.024))
    # butt plate
    _boxat(bm_metal, (0.021, 0.004, 0.064), (0, 0.002, -0.028))

    bm_brass = bmesh.new()
    # three barrel bands clamping barrel to stock
    for y in (0.52, 0.80, 1.08):
        _cyl(bm_brass, 0.0175, 0.0175, y, y + 0.018, z=0.003, segments=10)

    obs = []
    for bmx, mat, nm in ((bm_wood, walnut, "wood"), (bm_metal, steel, "metal"),
                         (bm_brass, brass, "bands")):
        me = bpy.data.meshes.new(f"{name}_{nm}")
        bmx.to_mesh(me)
        bmx.free()
        ob = bpy.data.objects.new(f"{name}_{nm}", me)
        bpy.context.scene.collection.objects.link(ob)
        ob.data.materials.append(mat)
        obs.append(ob)

    bpy.ops.object.select_all(action='DESELECT')
    for ob in obs:
        ob.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]
    bpy.ops.object.join()
    musket = bpy.context.view_layer.objects.active
    musket.name = name
    return musket


def attach_musket_to_rig(musket, rig, bone_name="prop_musket"):
    """Add a prop bone to the rig (child of Root) and skin the musket 100%
    to it. Clips key this bone directly."""
    bpy.ops.object.select_all(action='DESELECT')
    rig.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.object.mode_set(mode='EDIT')
    eb = rig.data.edit_bones
    if bone_name not in eb:
        b = eb.new(bone_name)
        b.head = Vector((0, 0, 0))
        b.tail = Vector((0, 0.25, 0))   # +Y like the musket's long axis
        b.parent = eb["Root"]
    bpy.ops.object.mode_set(mode='OBJECT')
    vg = musket.vertex_groups.new(name=bone_name)
    vg.add(range(len(musket.data.vertices)), 1.0, 'REPLACE')
    mod = musket.modifiers.new("rig", 'ARMATURE')
    mod.object = rig
    musket.parent = rig
    return musket
