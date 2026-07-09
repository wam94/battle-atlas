import bpy, math
from mathutils import Vector
SP="/private/tmp/claude-501/-Users-wmitchell-Documents-jetsons-warface/6273d195-6c7d-4985-96ff-e6d0198315ef/scratchpad"
bpy.ops.wm.open_mainfile(filepath=f"{SP}/spike_soldier.blend")
D=math.radians
scene=bpy.context.scene
rig=bpy.data.objects["SpikeRig"]
for m in bpy.data.materials:
    if m.use_nodes and "Principled BSDF" in m.node_tree.nodes:
        m.diffuse_color=m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value[:]
bpy.context.view_layer.objects.active=rig
bpy.ops.object.mode_set(mode='POSE')
pb=rig.pose.bones
rig.animation_data.action=None
for b in pb: b.rotation_euler=(0,0,0); b.location=(0,0,0)

def empty(name,loc):
    e=bpy.data.objects.new(name,None); e.location=loc
    scene.collection.objects.link(e); return e

def ik_solve(lower,target,pole,pole_angle=0.0):
    t=empty(f"t_{lower}",target); p=empty(f"p_{lower}",pole)
    c=pb[lower].constraints.new('IK')
    c.target=t; c.pole_target=p; c.pole_angle=pole_angle; c.chain_count=2
    bpy.context.view_layer.update()

# right arm carry: hand in front of right shoulder, elbow pointing down
ik_solve("LowerArm.R",(-0.16,-0.10,1.30),(-0.28,0.35,0.75))
# left arm hang: wrist beside left thigh
ik_solve("LowerArm.L",(0.21,-0.01,0.80),(0.45,0.35,1.00))
bpy.context.view_layer.update()
# apply visual pose to basis, then remove constraints
bpy.ops.pose.select_all(action='SELECT')
bpy.ops.pose.visual_transform_apply()
for name in ("LowerArm.R","LowerArm.L"):
    for c in list(pb[name].constraints): pb[name].constraints.remove(c)
for name in ("t_LowerArm.R","p_LowerArm.R","t_LowerArm.L","p_LowerArm.L"):
    bpy.data.objects.remove(bpy.data.objects[name],do_unlink=True)
bpy.context.view_layer.update()
for name in ("UpperArm.R","LowerArm.R","UpperArm.L","LowerArm.L"):
    e=pb[name].rotation_euler
    print(f"SOLVED {name}: ({math.degrees(e.x):.1f}, {math.degrees(e.y):.1f}, {math.degrees(e.z):.1f})")
w=rig.matrix_world @ pb["Hand.R"].head
print(f"wristR now ({w.x:.2f},{w.y:.2f},{w.z:.2f})")
w=rig.matrix_world @ pb["Hand.L"].head
print(f"wristL now ({w.x:.2f},{w.y:.2f},{w.z:.2f})")
cam=bpy.data.objects["SpikeCam"]
scene.render.resolution_x=960; scene.render.resolution_y=720
for tag,loc,rot in (("right",(-1.9,-2.4,1.45),(D(86),0,D(-38))),("front",(0.2,-3.0,1.45),(D(87),0,D(4)))):
    cam.location=loc; cam.rotation_euler=rot
    scene.render.filepath=f"{SP}/spike_previews/ik_{tag}.png"
    bpy.ops.render.render(write_still=True)
