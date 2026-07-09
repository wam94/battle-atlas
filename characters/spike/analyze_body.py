import bpy, collections
p = "/private/tmp/claude-501/-Users-wmitchell-Documents-jetsons-warface/6273d195-6c7d-4985-96ff-e6d0198315ef/scratchpad/downloads/hbm-bundle/human-base-meshes-bundle-v1.4.1/human_base_meshes_bundle.blend"
bpy.ops.wm.open_mainfile(filepath=p)
ob = bpy.data.objects["GEO-body_male_realistic"]
vs = [v.co for v in ob.data.vertices]  # local
xs=[v.x for v in vs]; ys=[v.y for v in vs]; zs=[v.z for v in vs]
print(f"local bbox x [{min(xs):.3f},{max(xs):.3f}] y [{min(ys):.3f},{max(ys):.3f}] z [{min(zs):.3f},{max(zs):.3f}]")
H=max(zs)
buckets=collections.defaultdict(list)
for v in vs:
    if v.x>0.15: buckets[round(v.x,2)].append(v)
ks=sorted(buckets)
for k in ks[::2]:
    b=buckets[k]; my=sum(v.y for v in b)/len(b); mz=sum(v.z for v in b)/len(b)
    print(f"armR x~{k:.2f}: n={len(b)} mean_y={my:.3f} mean_z={mz:.3f} z=({min(v.z for v in b):.3f},{max(v.z for v in b):.3f})")
lb=collections.defaultdict(list)
for v in vs:
    if v.z<0.60*H and v.x>0.005 and v.x<0.18: lb[round(v.z,1)].append(v)
for k in sorted(lb,reverse=True):
    b=lb[k]; mx=sum(v.x for v in b)/len(b); my=sum(v.y for v in b)/len(b)
    print(f"legR z~{k:.1f}: n={len(b)} mean_x={mx:.3f} mean_y={my:.3f} y=({min(v.y for v in b):.3f},{max(v.y for v in b):.3f}) x=({min(v.x for v in b):.3f},{max(v.x for v in b):.3f})")
# torso center: verts |x|<0.05 bucketed by z from 0.5H to 0.95H
tb=collections.defaultdict(list)
for v in vs:
    if abs(v.x)<0.06 and v.z>0.5*H: tb[round(v.z,1)].append(v)
for k in sorted(tb,reverse=True):
    b=tb[k]; my=sum(v.y for v in b)/len(b)
    print(f"torso z~{k:.1f}: n={len(b)} mean_y={my:.3f} y=({min(v.y for v in b):.3f},{max(v.y for v in b):.3f})")
