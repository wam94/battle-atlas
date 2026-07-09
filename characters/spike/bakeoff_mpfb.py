import bpy, math, addon_utils
from mathutils import Vector
SP="/private/tmp/claude-501/-Users-wmitchell-Documents-jetsons-warface/6273d195-6c7d-4985-96ff-e6d0198315ef/scratchpad"
D=math.radians
addon_utils.enable("bl_ext.user_default.mpfb", default_set=True, persistent=True)
from bl_ext.user_default.mpfb.services.humanservice import HumanService
scene=bpy.context.scene

# deterministic macro variant (not default) to prove parametric variation
from bl_ext.user_default.mpfb.services.targetservice import TargetService
macros=TargetService.get_default_macro_info_dict()
print("default macros:",macros)
macros.update({"height":0.62,"weight":0.48,"muscle":0.55,"age":0.35})
human=HumanService.create_human(mask_helpers=True, feet_on_ground=True, macro_detail_dict=macros)
print("MPFB human verts:",len(human.data.vertices),"polys:",len(human.data.polygons))
rig=HumanService.add_builtin_rig(human,"game_engine",import_weights=True)
print("rig:",rig, "bones:",len(rig.data.bones))
fingers=[b.name for b in rig.data.bones if "finger" in b.name.lower() or "thumb" in b.name.lower()]
print("finger bones:",len(fingers), fingers[:6])
# vertex groups on mesh
print("mesh vgroups:",len(human.vertex_groups))
# pose deformation test: raise upper arms
bpy.context.view_layer.objects.active=rig
bpy.ops.object.mode_set(mode='POSE')
for b in rig.pose.bones:
    b.rotation_mode='XYZ'
names=[b.name for b in rig.pose.bones]
print("sample bones:",names[:20])
def find(sub):
    for n in names:
        if sub.lower() in n.lower(): return n
    return None
ua=find("upperarm01.L") or find("upperarm.L") or find("upperarm_l") or find("shoulder.L")
print("posing bone:",ua)
if ua:
    rig.pose.bones[ua].rotation_euler=(0,0,D(45))
bpy.ops.object.mode_set(mode='OBJECT')
# materials for workbench
skin=bpy.data.materials.new("Skin"); skin.diffuse_color=(0.55,0.38,0.27,1.0)
human.data.materials.clear(); human.data.materials.append(skin)
# camera/light/render
cam_d=bpy.data.cameras.new("c"); cam=bpy.data.objects.new("c",cam_d); scene.collection.objects.link(cam)
cam_d.lens=60; scene.camera=cam
sun_d=bpy.data.lights.new("s",'SUN'); sun=bpy.data.objects.new("s",sun_d); scene.collection.objects.link(sun)
sun.rotation_euler=(D(55),0,D(-60)); sun_d.energy=4
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.light='STUDIO'; scene.display.shading.color_type='MATERIAL'
scene.render.resolution_x=1200; scene.render.resolution_y=1200
H=max((human.matrix_world @ v.co).z for v in human.data.vertices)
print("height:",round(H,3))
for tag,loc,look in (("head",(0.5,-0.9,H-0.12),(0,0,H-0.15)),("torso",(0.9,-1.8,H*0.75),(0,0,H*0.72)),("full",(1.4,-3.6,H*0.55),(0,0,H*0.5))):
    cam.location=loc
    direction=Vector(look)-Vector(loc)
    cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=f"{SP}/spike_previews/bake_mpfb_{tag}.png"
    bpy.ops.render.render(write_still=True)
    print("wrote",tag)
bpy.ops.wm.save_as_mainfile(filepath=f"{SP}/mpfb_bakeoff.blend")
