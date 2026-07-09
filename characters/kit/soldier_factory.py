"""Phase 6 character kit — body factory (MPFB, ADR 0004).

Builds a pruned, game_engine-rigged male body from deterministic macro
parameters. Everything headless bpy; no random anywhere.
"""
import bpy
import bmesh


def enable_mpfb():
    import addon_utils
    addon_utils.enable("bl_ext.user_default.mpfb", default_set=True, persistent=True)


def make_body(name, macros_overrides):
    """Create a pruned rigged male body. Returns (body, rig)."""
    from bl_ext.user_default.mpfb.services.humanservice import HumanService
    from bl_ext.user_default.mpfb.services.targetservice import TargetService

    macros = TargetService.get_default_macro_info_dict()
    macros.update({"gender": 1.0, "age": 0.42, "muscle": 0.58,
                   "weight": 0.5, "height": 0.55, "proportions": 0.55})
    macros["race"] = {"asian": 0.0, "caucasian": 1.0, "african": 0.0}
    macros.update(macros_overrides or {})

    body = HumanService.create_human(
        mask_helpers=True, feet_on_ground=True, macro_detail_dict=macros)
    # MPFB applies macro targets as SHAPE KEYS; every downstream measurement
    # and face extraction reads raw vertex coordinates, so bake the keys in.
    bake_shape_keys(body)
    rig = HumanService.add_builtin_rig(body, "game_engine", import_weights=True)
    rig.name = f"{name}_rig"

    prune_helpers(body)
    body.name = f"{name}_body"
    # drop the now-meaningless mask modifier (helpers deleted for real)
    for m in list(body.modifiers):
        if m.type == 'MASK':
            body.modifiers.remove(m)
    return body, rig


def bake_shape_keys(body):
    """Write the current shape-key mix into the base vertex coordinates and
    drop all keys (modifiers are temporarily disabled so only keys bake)."""
    if body.data.shape_keys is None:
        return
    vis = [(m, m.show_viewport) for m in body.modifiers]
    for m, _ in vis:
        m.show_viewport = False
    dg = bpy.context.evaluated_depsgraph_get()
    dg.update()
    ev_co = [v.co.copy() for v in body.evaluated_get(dg).data.vertices]
    body.shape_key_clear()
    if len(ev_co) != len(body.data.vertices):
        raise RuntimeError(
            f"shape-key bake vertex mismatch: {len(ev_co)} evaluated vs "
            f"{len(body.data.vertices)} raw")
    for v, co in zip(body.data.vertices, ev_co):
        v.co = co
    for m, state in vis:
        m.show_viewport = state


def prune_helpers(body):
    """Delete every vertex not in the 'body' group (clothes/joint helpers)."""
    gi = body.vertex_groups["body"].index
    keep = set()
    for v in body.data.vertices:
        for g in v.groups:
            if g.group == gi and g.weight > 0:
                keep.add(v.index)
                break
    bm = bmesh.new()
    bm.from_mesh(body.data)
    bm.verts.ensure_lookup_table()
    doomed = [v for v in bm.verts if v.index not in keep]
    bmesh.ops.delete(bm, geom=doomed, context='VERTS')
    bm.to_mesh(body.data)
    bm.free()


def flat_mat(name, rgba, rough=0.85, existing=None):
    if existing is not None and name in existing:
        return existing[name]
    m = bpy.data.materials.get(name)
    if m is None:
        m = bpy.data.materials.new(name)
        m.use_nodes = True
        bsdf = m.node_tree.nodes["Principled BSDF"]
        bsdf.inputs["Base Color"].default_value = rgba
        bsdf.inputs["Roughness"].default_value = rough
        m.diffuse_color = rgba
    return m


def bone_head(rig, bone):
    """Bone head position in world space (rig at identity -> armature space)."""
    return rig.matrix_world @ rig.data.bones[bone].head_local


def bone_tail(rig, bone):
    return rig.matrix_world @ rig.data.bones[bone].tail_local
