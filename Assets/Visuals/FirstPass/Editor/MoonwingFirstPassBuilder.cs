using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        const string Root="Assets/Visuals/FirstPass";
        const string Clearing="Assets/Visuals/MoonlitClearing";
        const string ScenePath="Assets/Scenes/MoonwingForest.unity";
        const string PassName="Moonwing - Full Visual Pass";
        static Material silk, silver, skin, hair, glow, veil, warm, shadow, red, dust, smoke, trail;
        static readonly Color Lavender=new Color(0.47f,0.31f,0.72f);
        static readonly Color Cyan=new Color(0.24f,0.76f,0.95f);
        static readonly Color Pearl=new Color(0.72f,0.84f,1f);
        static readonly Color Gold=new Color(0.75f,0.47f,0.2f);

        public static void BuildBatch()
        {
            EditorSceneManager.OpenScene(ScenePath);
            if(GameObject.Find(PassName)) throw new InvalidOperationException("Visual pass already exists; edit saved assets instead of rebuilding.");
            var state=Random.state;
            try
            {
                Random.InitState(94261);
                foreach(string folder in new[]{"Materials","Meshes","Prefabs","UI"}) Directory.CreateDirectory(Root+"/"+folder);
                AssetDatabase.Refresh();
                CreateMaterials();
                var pass=new GameObject(PassName);
                ExpandForest(pass.transform);
                AttachVisual("Player",Fairy());
                AttachVisual("Caretaker",Caretaker());
                AttachVisual("ForestSpirit",Spirit());
                DressAssassin();
                DressArrow();
                StyleUI();
                Home(pass.transform);
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                Validate();
                CapturePreviews();
                Debug.Log("MOONWING_FULL_PASS_BUILT");
            }
            finally { Random.state=state; }
        }

        static void CreateMaterials()
        {
            silk=Mat("Moonfairy Silks","Moonwing/Clearing Botanical",Color.white,0.15f);
            silver=Mat("Moon Silver","Moonwing/Clearing Botanical",Color.white,0.4f);
            skin=Mat("Warm Porcelain","Moonwing/Clearing Botanical",Color.white,0.08f);
            hair=Mat("Lavender Silver Hair","Moonwing/Clearing Botanical",Color.white,0.13f);
            glow=Mat("Moon Goddess Light","Moonwing/Clearing Botanical",Color.white,2.4f);
            veil=Mat("Translucent Moon Wings","Moonwing/Silken Veil",new Color(1,1,1,0.72f),1.5f);
            warm=Mat("Caretaker Lantern Gold","Moonwing/Clearing Botanical",Color.white,2.1f);
            shadow=Mat("Assassin Shadow Cloth","Moonwing/Clearing Botanical",Color.white,0.025f);
            red=Mat("Shadow Sigils","Moonwing/Clearing Botanical",Color.white,0.6f);
            dust=Mat("Moonlight Pixie Dust","Moonwing/Soft Motes",Color.white,0); dust.SetFloat("_Intensity",2.7f);
            smoke=Mat("Violet Shadow Smoke","Moonwing/Shadow Wisps",Color.white,0);
            trail=Mat("Flowing Moonlight","Moonwing/Moonlight Ribbon",Color.white,0); trail.SetFloat("_Intensity",2.3f);
        }
        static Material Mat(string name,string shader,Color color,float emission)
        {
            var s=Shader.Find(shader); if(!s) throw new InvalidOperationException("Shader not found: "+shader);
            var m=new Material(s) { name=name,enableInstancing=true };
            if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Emission")) m.SetFloat("_Emission",emission);
            AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat"); return m;
        }
        static Transform Group(string name,Transform parent)
        { var go=new GameObject(name); go.transform.SetParent(parent,false); return go.transform; }
        static MeshRenderer Mesh(string name,MoonwingMesh data,Material material,Transform parent)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.AddComponent<MeshFilter>().sharedMesh=data.Save(name);
            var r=go.AddComponent<MeshRenderer>(); r.sharedMaterial=material; r.shadowCastingMode=ShadowCastingMode.Off; r.receiveShadows=true; return r;
        }
        static GameObject Prefab(GameObject go)
        {
            var result=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+go.name+".prefab"); UnityEngine.Object.DestroyImmediate(go); return result;
        }
        static GameObject Place(GameObject prefab,Transform parent,Vector3 p,float yaw=0,float scale=1)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent); go.transform.localPosition=p;
            go.transform.localRotation=Quaternion.Euler(0,yaw,0); go.transform.localScale=Vector3.one*scale; return go;
        }
        static void AttachVisual(string rootName,GameObject prefab)
        {
            var root=GameObject.Find(rootName);
            // Retain the old collider-sized shadow without rendering the placeholder capsule.
            root.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.ShadowsOnly;
            Place(prefab,root.transform,Vector3.zero);
        }
        static ParticleSystem Motes(string name,Transform parent,Vector3 p,Material material,Color a,Color b,int count,float rate,float size,float radius,float lifetime=2.4f)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=p;
            var ps=go.AddComponent<ParticleSystem>(); ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.useAutoRandomSeed=false; ps.randomSeed=(uint)Random.Range(1,99999);
            var main=ps.main; main.loop=true; main.prewarm=true; main.duration=lifetime; main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*0.65f,lifetime); main.maxParticles=count;
            main.startSize=new ParticleSystem.MinMaxCurve(size*0.5f,size); main.startSpeed=new ParticleSystem.MinMaxCurve(0.035f,0.14f); main.useUnscaledTime=true;
            main.startColor=new ParticleSystem.MinMaxGradient(a,b); main.simulationSpace=ParticleSystemSimulationSpace.World;
            var e=ps.emission; e.rateOverTime=rate;
            var s=ps.shape; s.shapeType=ParticleSystemShapeType.Sphere; s.radius=radius;
            var velocity=ps.velocityOverLifetime; velocity.enabled=true; velocity.space=ParticleSystemSimulationSpace.World;
            velocity.x=new ParticleSystem.MinMaxCurve(0f,0f); velocity.y=new ParticleSystem.MinMaxCurve(0.04f,0.12f); velocity.z=new ParticleSystem.MinMaxCurve(0f,0f);
            var noise=ps.noise; noise.enabled=true; noise.strength=0.08f; noise.frequency=0.65f; noise.scrollSpeed=0.2f; noise.quality=ParticleSystemNoiseQuality.Low;
            var col=ps.colorOverLifetime; col.enabled=true; col.color=FadeGradient();
            var renderer=ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=material; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            return ps;
        }
        static Gradient FadeGradient()
        {
            var g=new Gradient(); g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(0.8f,0.15f),new GradientAlphaKey(0.5f,0.65f),new GradientAlphaKey(0,1)}); return g;
        }
        static Vector3[] Circle(Vector3 center,float rx,float rz,int count=33)
        {
            var p=new Vector3[count]; for(int i=0;i<count;i++) { float a=i/(float)(count-1)*Mathf.PI*2; p[i]=center+new Vector3(Mathf.Cos(a)*rx,0,Mathf.Sin(a)*rz); } return p;
        }
        static float R(float a,float b) { return Random.Range(a,b); }
    }
}
