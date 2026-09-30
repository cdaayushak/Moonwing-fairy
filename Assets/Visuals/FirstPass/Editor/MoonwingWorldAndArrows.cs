using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        static void ExpandForest(Transform parent)
        {
            var forest=Group("Forest Extension - Southern Glades and Borders",parent);
            string[] trees={"Silverbranch Arch","Lavender Willow","Midnight Alder","Moonbloom Crown"};
            for(int i=0;i<18;i++)
            {
                float x=-23+(i%7)*7.5f,z=-23+(i/7)*7;
                if(Mathf.Abs(x)<4 || (x<-9 && z>-20 && z<-7) || (new Vector2(x,z)).magnitude<9) continue;
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/"+trees[i%4]+".prefab");
                Place(prefab,forest,new Vector3(x+R(-1,1),0,z+R(-1,1)),R(0,360),R(0.70f,1.2f));
            }
            var fern=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Indigo Fern Island.prefab");
            var flower=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Starlace Moonflowers.prefab");
            var fungi=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Opaline Mushroom Family.prefab");
            var rock=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Weathered Slate Group.prefab");
            Material caps=AssetDatabase.LoadAssetAtPath<Material>(Clearing+"/Materials/Luminous Mushroom Caps.mat"); caps.SetFloat("_Emission",3.5f); EditorUtility.SetDirty(caps);
            Material petals=AssetDatabase.LoadAssetAtPath<Material>(Clearing+"/Materials/Moonflower Petals.mat"); petals.SetFloat("_Emission",2.2f); EditorUtility.SetDirty(petals);
            var profile=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(Clearing+"/Settings/Moonwing Clearing Atmosphere.asset");
            if(profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom)) { bloom.intensity.Override(0.6f); EditorUtility.SetDirty(bloom); }
            var softCaps=new Material(caps) { name="Quiet Lavender Mushroom Caps" }; softCaps.SetFloat("_Emission",1.65f); AssetDatabase.CreateAsset(softCaps,Root+"/Materials/Quiet Lavender Mushroom Caps.mat");
            var brightCaps=new Material(caps) { name="Moonwell Mushroom Caps" }; brightCaps.SetFloat("_Emission",5f); AssetDatabase.CreateAsset(brightCaps,Root+"/Materials/Moonwell Mushroom Caps.mat");
            for(int i=0;i<64;i++)
            {
                Vector3 p=new Vector3(R(-23,23),0,R(-23,23));
                // Clear central battle area, connecting paths, and the residence footprint.
                if(new Vector2(p.x,p.z).magnitude<7 || Mathf.Abs(p.x-Mathf.Sin(p.z*0.17f)*2.4f)<3.6f || Mathf.Abs(p.z)<2.5f || (p.x<-9 && p.z>-20 && p.z<-8)) continue;
                Place(i%4==0?rock:fern,forest,p,R(0,360),R(0.65f,1.25f));
                var plant=Place(i%2==0?fungi:flower,forest,p+new Vector3(0.6f,0,0.3f),R(0,360),R(0.7f,1.2f));
                if(i%2==0) plant.transform.Find("Opaline Caps").GetComponent<MeshRenderer>().sharedMaterial=i%6==0?brightCaps:softCaps;
            }
            // Vary existing mushroom families as well; the brightest clusters are accents, not all plants.
            var existing=GameObject.Find("Moonlit Clearing - Visual Study"); int family=0;
            foreach(var r in existing.GetComponentsInChildren<MeshRenderer>())
                if(r.name=="Opaline Caps") { r.sharedMaterial=family++%4==0?brightCaps:family%2==0?caps:softCaps; }
            var ambience=GameObject.Find("Quiet Pixie Drift").GetComponent<ParticleSystem>();
            var am=ambience.main; am.maxParticles=120; am.useUnscaledTime=true;
            var ae=ambience.emission; ae.rateOverTime=10;
            var ashape=ambience.shape; ashape.scale=new Vector3(46,3,46); ambience.transform.position=new Vector3(0,1.2f,0);
            var southMist=Motes("Southern Glade Mist",forest,new Vector3(0,0.65f,-18),dust,new Color(0.2f,0.3f,0.5f,0.016f),new Color(0.3f,0.2f,0.5f,0.012f),12,0.45f,6,1,22);
            var shape=southMist.shape; shape.shapeType=ParticleSystemShapeType.Box; shape.scale=new Vector3(44,0.2f,9);
            // Two shared glade accents imply light cast by nearby clusters; never one light per plant.
            foreach(Vector3 p in new[]{new Vector3(-6,0.55f,4),new Vector3(10,0.55f,-12)})
            {
                var light=Group("Bioluminescent Glade Accent - No Shadows",forest).gameObject.AddComponent<Light>(); light.type=LightType.Point;
                light.transform.localPosition=p; light.color=new Color(0.32f,0.63f,1); light.intensity=0.55f; light.range=3.8f; light.shadows=LightShadows.None;
            }
        }

        static void DressArrow()
        {
            var wakeObject=new GameObject("Detached Moonlight Wake");
            var particles=Motes("Streaming Pixie Dust",wakeObject.transform,Vector3.zero,dust,new Color(0.35f,0.85f,1,0.9f),new Color(0.8f,0.55f,1,0.75f),90,65,0.065f,0.035f,1.1f);
            var pm=particles.main; pm.prewarm=false; pm.startSpeed=new ParticleSystem.MinMaxCurve(0.1f,0.55f);
            var ribbon=wakeObject.AddComponent<TrailRenderer>(); ribbon.sharedMaterial=trail; ribbon.time=0.38f; ribbon.minVertexDistance=0.06f;
            ribbon.widthCurve=new AnimationCurve(new Keyframe(0,0.075f),new Keyframe(0.35f,0.045f),new Keyframe(1,0)); ribbon.widthMultiplier=1;
            ribbon.numCapVertices=3; ribbon.numCornerVertices=2; ribbon.shadowCastingMode=ShadowCastingMode.Off; ribbon.receiveShadows=false;
            var gradient=new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(Pearl,0),new GradientColorKey(Cyan,0.35f),new GradientColorKey(Lavender,1)},new[]{new GradientAlphaKey(0.9f,0),new GradientAlphaKey(0,1)}); ribbon.colorGradient=gradient;
            var wake=wakeObject.AddComponent<MoonwingFadingWake>(); wake.dust=particles; wake.ribbon=ribbon;
            var wakePrefab=Prefab(wakeObject).GetComponent<MoonwingFadingWake>();

            var impactRoot=new GameObject("Moonlight Meets Shadow - Impact");
            var impact=Motes("Moonlight Impact Sparks",impactRoot.transform,Vector3.zero,dust,new Color(0.48f,0.88f,1,0.9f),new Color(0.75f,0.55f,1,0.7f),28,0,0.08f,0.035f,0.75f);
            var im=impact.main; im.loop=false; im.prewarm=false; im.duration=0.1f; im.startSpeed=new ParticleSystem.MinMaxCurve(0.4f,1.6f); im.stopAction=ParticleSystemStopAction.Destroy;
            var ie=impact.emission; ie.SetBursts(new[]{new ParticleSystem.Burst(0,22)});
            // Save the particle system itself as the prefab root, so stopAction cleans up the whole effect.
            impact.transform.SetParent(null); UnityEngine.Object.DestroyImmediate(impactRoot); impact.gameObject.name="Moonlight Impact Burst";
            var impactPrefab=Prefab(impact.gameObject).GetComponent<ParticleSystem>();

            var arrow=new GameObject("Enchanted Arrow - Igniting Moonlight");
            var shaft=new MoonwingMesh(); shaft.Line(new Vector3(0,0,-0.4f),new Vector3(0,0,0.22f),0.018f,Pearl);
            var physical=Mesh("Arrow Silver Before Ignition",shaft,silver,arrow.transform);
            var energy=Group("Released Moonlight Energy",arrow.transform);
            var core=new MoonwingMesh(); core.Tube(new[]{new Vector3(0,0,-0.35f),Vector3.zero,new Vector3(0,0,0.26f)},0.026f,0.019f,Cyan);
            core.Diamond(new Vector3(0,0,0.29f),new Vector3(0.075f,0.045f,0.2f),Pearl);
            for(int i=0;i<3;i++)
            {
                float angle=i*Mathf.PI*2/3; Vector3 side=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0);
                core.Quad(new Vector3(0,0,-0.15f),side*0.10f+new Vector3(0,0,-0.28f),side*0.06f+new Vector3(0,0,-0.43f),new Vector3(0,0,-0.37f),Lavender);
            }
            Mesh("Arrow Moonlight Core and Fletching",core,glow,energy);
            var magic=arrow.AddComponent<MoonwingArrowMagic>(); magic.wakePrefab=wakePrefab; magic.silverShaft=physical; magic.energy=energy;
            var prefab=Prefab(arrow);
            string path="Assets/Prefabs/Arrow.prefab"; var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                root.GetComponent<MeshRenderer>().enabled=false;
                var visual=Place(prefab,root.transform,Vector3.zero);
                Vector3 scale=root.transform.localScale;
                visual.transform.localScale=new Vector3(1/scale.x,1/scale.y,1/scale.z);
                var hit=root.AddComponent<MoonwingImpactFeedback>(); hit.impactPrefab=impactPrefab;
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        static void Home(Transform parent)
        {
            var home=Group("Moonfairy Residence - Lantern Garden Foundation",parent); home.localPosition=new Vector3(-15,0,-14); home.localRotation=Quaternion.Euler(0,22,0);
            var wood=new MoonwingMesh(); var roof=new MoonwingMesh(); var stonework=new MoonwingMesh(); var trim=new MoonwingMesh(); var lamps=new MoonwingMesh();
            Color timber=new Color(0.12f,0.10f,0.20f),tile=new Color(0.15f,0.19f,0.32f),pale=new Color(0.29f,0.31f,0.42f);
            stonework.Box(new Vector3(0,0.17f,0),new Vector3(7,0.34f,5.8f),pale);
            for(int i=0;i<3;i++) stonework.Box(new Vector3(0,0.05f+i*0.09f,3.7f-i*0.43f),new Vector3(2.7f,0.1f+i*0.18f,0.65f),pale);
            foreach(float x in new[]{-2.8f,2.8f}) foreach(float z in new[]{-2.1f,2.1f})
            {
                wood.Line(new Vector3(x,0.34f,z),new Vector3(x,3.65f,z),0.12f,timber);
                trim.Tube(Circle(new Vector3(x,0.5f,z),0.16f,0.16f),0.035f,0.035f,Gold);
            }
            wood.Box(new Vector3(0,3.45f,0),new Vector3(6.0f,0.16f,4.7f),timber);
            // Swept roof strips make an original open pavilion with gently rising eaves.
            for(int side=-1;side<=1;side+=2)
            {
                for(int strip=0;strip<20;strip++)
                {
                    float x=-3.65f+strip*0.365f;
                    for(int segment=0;segment<10;segment++)
                    {
                        float a=segment/10f,b=(segment+1)/10f;
                        float ya=4.9f-1.55f*a+0.65f*Mathf.Pow(a,5),yb=4.9f-1.55f*b+0.65f*Mathf.Pow(b,5);
                        roof.Quad(new Vector3(x,ya,side*a*3.2f),new Vector3(x+0.365f,ya,side*a*3.2f),new Vector3(x+0.365f,yb,side*b*3.2f),new Vector3(x,yb,side*b*3.2f),Color.Lerp(tile,Lavender,strip%3*0.07f));
                    }
                }
                trim.Line(new Vector3(-3.65f,4.0f,side*3.2f),new Vector3(3.65f,4.0f,side*3.2f),0.04f,Gold);
            }
            trim.Line(new Vector3(-3.8f,4.9f,0),new Vector3(3.8f,4.9f,0),0.055f,Pearl);
            wood.Box(new Vector3(0,1.65f,-2.08f),new Vector3(5.5f,2.4f,0.09f),new Color(0.20f,0.15f,0.27f));
            for(int i=-5;i<=5;i++) trim.Line(new Vector3(i*0.46f,0.7f,-2.0f),new Vector3(i*0.46f,2.8f,-2.0f),0.022f,Gold);
            for(int i=0;i<4;i++) trim.Line(new Vector3(-2.4f,0.9f+i*0.55f,-1.99f),new Vector3(2.4f,0.9f+i*0.55f,-1.99f),0.02f,Gold);
            foreach(float x in new[]{-2.5f,2.5f})
            {
                Vector3 p=new Vector3(x,2.65f,2);
                trim.Line(p+Vector3.up*0.65f,p,0.012f,Gold);
                lamps.Ellipsoid(p,new Vector3(0.16f,0.29f,0.16f),new Color(1,0.62f,0.22f));
                trim.Tube(Circle(p+Vector3.up*0.25f,0.17f,0.17f),0.025f,0.025f,Gold); trim.Tube(Circle(p-Vector3.up*0.25f,0.17f,0.17f),0.025f,0.025f,Gold);
            }
            Mesh("Residence Timber Structure",wood,silk,home); Mesh("Residence Swept Lavender Roof",roof,silk,home); Mesh("Residence Moonstone Steps",stonework,silk,home); Mesh("Residence Silver Gold Lattice",trim,silver,home); Mesh("Residence Warm Lanterns",lamps,warm,home);
            var lamp=Group("Garden Lantern Fill - No Shadows",home).gameObject.AddComponent<Light>(); lamp.type=LightType.Point; lamp.color=new Color(1,0.65f,0.36f); lamp.intensity=1.4f; lamp.range=6; lamp.shadows=LightShadows.None; lamp.transform.localPosition=new Vector3(0,2.1f,1.5f);
            var flowers=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Starlace Moonflowers.prefab");
            var mushrooms=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Opaline Mushroom Family.prefab");
            for(int i=0;i<8;i++) { float side=i%2==0?-1:1; Place(i%3==0?mushrooms:flowers,home,new Vector3(side*(2+R(0,1.6f)),0,3.5f+i/2*0.55f),R(0,360),0.9f); }
        }
    }
}
