using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Moonwing.Visuals.Editor
{
    // Editor-only asset authoring. No runtime generation and no gameplay dependencies.
    public static class MoonlitClearingBuilder
    {
        const string Root = "Assets/Visuals/MoonlitClearing";
        const string ScenePath = "Assets/Scenes/MoonwingForest.unity";
        const string GroupName = "Moonlit Clearing - Visual Study";
        static Material bark, leaves, grass, stone, stem, bloom, mushroom, ground, motes;
        static readonly Color Lavender = new Color(0.38f, 0.24f, 0.56f);
        static readonly Color Cyan = new Color(0.14f, 0.65f, 0.8f);

        [MenuItem("Moonwing/Visuals/Build clearing in current forest scene")]
        public static void BuildFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Open MoonwingForest outside Play mode first.");
            if (GameObject.Find(GroupName) != null)
                throw new InvalidOperationException("Clearing already exists. Edit its objects directly; builder will not overwrite your visual edits.");
            Build();
        }

        // Used only in the isolated authoring project, never auto-runs on import.
        public static void BuildBatch()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Build();
            Capture();
        }

        static void Build()
        {
            if (GameObject.Find(GroupName)) throw new InvalidOperationException("Clearing already exists.");
            var randomState = Random.state;
            try
            {
                Random.InitState(73129);
                foreach (string folder in new[] { "Materials", "Meshes", "Prefabs", "Settings" })
                    Directory.CreateDirectory(Root + "/" + folder);
                AssetDatabase.Refresh();
                Materials();
                var root = new GameObject(GroupName);
                Undo.RegisterCreatedObjectUndo(root, "Create moonlit clearing");
                Transform trees = Group("01 - Framing Trees", root.transform);
                Transform undergrowth = Group("02 - Botanical Borders", root.transform);
                Transform rocks = Group("03 - Mossy Stones", root.transform);
                Transform atmosphere = Group("04 - Moonlight and Motes", root.transform);
                Transform floor = Group("05 - Clearing Floor", root.transform);

                var treePrefabs = new GameObject[4];
                for (int i = 0; i < treePrefabs.Length; i++) treePrefabs[i] = Tree(i);
                var fern = Fern();
                var flower = Flower();
                var fungi = Mushrooms();
                var rock = Rock();

                // Frame the existing fixed camera with open entry/exit lanes on all four sides.
                // Trees stay outside the broad central combat area. All added geometry is non-colliding.
                Vector3[] positions = {
                    new Vector3(-12,0,-3), new Vector3(13,0,-1),
                    new Vector3(-14,0,6), new Vector3(14,0,8),
                    new Vector3(-11,0,13), new Vector3(11,0,16),
                    new Vector3(-17,0,14), new Vector3(18,0,17),
                    new Vector3(-16,0,22), new Vector3(-9,0,24),
                    new Vector3(12,0,24), new Vector3(20,0,23),
                    new Vector3(-22,0,4), new Vector3(22,0,6),
                    new Vector3(-22,0,19), new Vector3(22,0,14),
                    new Vector3(-20,0,-8), new Vector3(20,0,-7)
                };
                for (int i = 0; i < positions.Length; i++)
                {
                    var t = Place(treePrefabs[i % 4], trees, positions[i], R(0,360), R(0.85f,1.22f));
                    if (i >= 8) t.transform.localScale *= 1.14f;
                }
                // A shallow scenic backdrop closes the ground edge in this camera's view.
                // It is part of this composition, not a traversable forest expansion.
                for(int i=0;i<12;i++)
                {
                    Vector3 p=new Vector3(-27+i*5.1f,0,31+(i%3)*5);
                    Place(treePrefabs[(i+2)%4],trees,p,R(0,360),R(0.75f,1.05f));
                }
                // Keep the original tree and all its colliders/references; dress its location.
                var originalTree=GameObject.Find("Tree_1");
                foreach(var renderer in originalTree.GetComponentsInChildren<MeshRenderer>()) renderer.enabled=false;
                Place(treePrefabs[0],trees,new Vector3(7,0,5),145,0.56f).name="Silverbranch - Original Tree Dressing";
                // Smaller botanical islands establish scale, with no decoration in the central 8m radius.
                for (int i = 0; i < 48; i++)
                {
                    float angle = R(0,Mathf.PI*2);
                    float radius = R(9,22);
                    Vector3 p = new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius+3);
                    // Winding north/south lane and an east/west route remain visually open.
                    if (Mathf.Abs(p.x-Mathf.Sin(p.z*0.17f)*2.4f)<4 || Mathf.Abs(p.z)<2.8f) continue;
                    Place(fern, undergrowth, p, R(0,360), R(0.8f,1.6f));
                    if (i%2==0) Place(flower,undergrowth,p+new Vector3(0.7f,0,0.2f),R(0,360),R(0.75f,1.3f));
                    if (i%3==0) Place(fungi,undergrowth,p+new Vector3(-0.6f,0,0.5f),R(0,360),R(0.8f,1.4f));
                    if (i%4==0) Place(rock,rocks,p+new Vector3(1.1f,0,0.8f),R(0,360),R(0.7f,1.4f));
                }
                // Deliberate visible accents, framing the starting view without hiding actors.
                foreach (Vector3 p in new[] { new Vector3(-6,0,4),new Vector3(7,0,6),new Vector3(-8,0,11),new Vector3(8,0,14),new Vector3(-5,0,-3),new Vector3(6,0,-2) })
                {
                    Place(flower,undergrowth,p, R(0,360),1);
                    Place(fungi,undergrowth,p+new Vector3(1,0,0.3f),R(0,360),0.85f);
                    Place(fern,undergrowth,p+new Vector3(-0.6f,0,0.4f),R(0,360),1.1f);
                }
                MeshData floorMesh = new MeshData();
                floorMesh.Quad(new Vector3(-24.9f,0.012f,-24.9f),new Vector3(-24.9f,0.012f,24.9f),new Vector3(24.9f,0.012f,24.9f),new Vector3(24.9f,0.012f,-24.9f),Color.white);
                MeshObject("Moss and Winding Earth",floorMesh.Save("ClearingFloor"),ground,floor,false);
                var backdropFloor=new MeshData();
                backdropFloor.Quad(new Vector3(-65,-0.025f,24.8f),new Vector3(-65,-0.025f,80),new Vector3(65,-0.025f,80),new Vector3(65,-0.025f,24.8f),Color.white);
                MeshObject("Distant Ground Fade - Scenery Only",backdropFloor.Save("BackdropFloor"),ground,floor,false);
                Lighting(atmosphere);
                Particles(atmosphere);
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                Validate();
                Debug.Log("MOONWING_CLEARING_BUILT: " + root.GetComponentsInChildren<Renderer>().Length + " renderers; no gameplay components changed.");
            }
            finally { Random.state = randomState; }
        }

        static void Materials()
        {
            bark = Mat("Silver Indigo Bark", "Universal Render Pipeline/Lit", new Color(0.24f,0.20f,0.29f));
            bark.SetFloat("_Smoothness",0.18f);
            leaves = Mat("Lavender Canopy", "Moonwing/Clearing Botanical", Color.white);
            leaves.SetFloat("_Wind",0.065f);
            grass = Mat("Blue Ferns", "Moonwing/Clearing Botanical",Color.white);
            grass.SetFloat("_Wind",0.025f);
            stone = Mat("Moss Slate", "Universal Render Pipeline/Lit", new Color(0.12f,0.17f,0.21f));
            stone.SetFloat("_Smoothness",0.24f);
            stem = Mat("Botanical Stems", "Moonwing/Clearing Botanical",Color.white);
            bloom = Mat("Moonflower Petals", "Moonwing/Clearing Botanical", Color.white);
            bloom.SetFloat("_Emission",1.65f);
            mushroom = Mat("Luminous Mushroom Caps", "Moonwing/Clearing Botanical",Color.white);
            mushroom.SetFloat("_Emission",1.05f);
            ground = Mat("Moonlit Moss and Earth", "Moonwing/Clearing Ground",new Color(0.095f,0.14f,0.18f));
            motes = Mat("Pixie Motes", "Moonwing/Soft Motes",Color.white);
            motes.SetFloat("_Intensity",2.4f);
        }

        static Material Mat(string name,string shader,Color color)
        {
            Shader s = Shader.Find(shader);
            if (s==null) throw new InvalidOperationException("Missing shader " + shader);
            var m=new Material(s) { name=name, enableInstancing=true };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",color);
            AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");
            return m;
        }

        static GameObject Tree(int variant)
        {
            string[] names={"Silverbranch Arch", "Lavender Willow", "Midnight Alder", "Moonbloom Crown"};
            GameObject go=new GameObject(names[variant]);
            var wood=new MeshData(); var foliage=new MeshData();
            float height=variant==1?10.8f:variant==2?13.5f:11.6f;
            float lean=variant==0?2.0f:variant==2?-0.7f:0.9f;
            Vector3 top=new Vector3(lean,height,0.5f);
            wood.Tube(Curve(Vector3.zero, new Vector3(-0.8f,height*0.46f,0),top,15),0.65f,0.11f,12,Color.white);
            for(int j=0;j<7;j++)
            {
                float a=j*Mathf.PI*2/7;
                Vector3 tip=new Vector3(Mathf.Cos(a)*R(1.5f,2.5f),0.02f,Mathf.Sin(a)*R(1.5f,2.5f));
                wood.Tube(Curve(tip,tip*0.3f+Vector3.up*0.35f,Vector3.up*1.2f,7),0.025f,0.29f,8,Color.white);
            }
            for(int j=0;j<11;j++)
            {
                float a=j*2.39996f+variant;
                float t=0.38f+j*0.047f;
                Vector3 start=Vector3.Lerp(Vector3.zero,top,t);
                float reach=R(2.5f,4.8f)*(1.25f-t*0.6f);
                Vector3 end=start+new Vector3(Mathf.Cos(a)*reach,R(1.1f,2.7f),Mathf.Sin(a)*reach);
                Vector3 bend=Vector3.Lerp(start,end,0.45f)+Vector3.down*0.45f;
                wood.Tube(Curve(start,bend,end,9),0.24f*(1-t*0.5f),0.025f,9,Color.white);
                for(int k=0;k<4;k++)
                {
                    Vector3 twigEnd=end+new Vector3(R(-1.5f,1.5f),R(-0.1f,1.0f),R(-1.5f,1.5f));
                    wood.Tube(Curve(Vector3.Lerp(bend,end,0.7f),end,twigEnd,5),0.045f,0.008f,6,Color.white);
                    int count=variant==2?30:44;
                    for(int q=0;q<count;q++)
                    {
                        Vector3 offset=Random.insideUnitSphere;
                        offset=Vector3.Scale(offset,new Vector3(1.15f,0.48f,1.05f));
                        Vector3 leafPosition=twigEnd+offset;
                        Color c=Color.Lerp(new Color(0.085f,0.12f,0.23f),Lavender,R(0.1f,1));
                        if(variant==3) c=Color.Lerp(c,new Color(0.57f,0.37f,0.65f),0.35f);
                        if(variant==2) c=Color.Lerp(c,new Color(0.08f,0.23f,0.28f),0.65f);
                        c.a=1;
                        foliage.Leaf(leafPosition,Quaternion.Euler(R(-45,45),R(0,360),R(-30,30)),R(0.3f,0.56f),R(0.09f,0.17f),c);
                    }
                    if(variant==1)
                    {
                        Vector3 down=twigEnd+new Vector3(0.4f,-R(1.8f,3.5f),0.2f);
                        wood.Tube(Curve(twigEnd,twigEnd+Vector3.down,down,7),0.018f,0.003f,5,Color.white);
                        for(int q=0;q<16;q++)
                        {
                            Vector3 p=Vector3.Lerp(twigEnd,down,q/16f);
                            foliage.Leaf(p,Quaternion.Euler(55,q*137,25),0.4f,0.075f,new Color(0.24f,0.21f,0.4f,1));
                        }
                    }
                }
            }
            MeshObject("Curved Trunk and Branches",wood.Save(names[variant]+" Wood"),bark,go.transform,true);
            MeshObject("Individual Leaf Canopy",foliage.Save(names[variant]+" Leaves"),leaves,go.transform,false);
            return SavePrefab(go);
        }

        static GameObject Fern()
        {
            var go=new GameObject("Indigo Fern Island"); var mesh=new MeshData();
            for(int plant=0;plant<6;plant++)
            {
                Vector3 origin=new Vector3(R(-0.7f,0.7f),0.02f,R(-0.7f,0.7f));
                for(int frond=0;frond<7;frond++)
                {
                    float a=frond*Mathf.PI*2/7+plant;
                    Vector3 direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    Vector3 side=Vector3.Cross(Vector3.up,direction);
                    float length=R(0.6f,1.1f);
                    for(int leaf=1;leaf<9;leaf++)
                    {
                        float t=leaf/9f;
                        Vector3 p=origin+direction*t*length+Vector3.up*Mathf.Sin(t*2.1f)*length*0.6f;
                        float size=(1-t)*0.32f+0.045f;
                        Color c=Color.Lerp(new Color(0.025f,0.095f,0.13f,1),new Color(0.12f,0.33f,0.37f,1),t);
                        foreach(int sign in new[]{-1,1})
                            mesh.Leaf(p,Quaternion.LookRotation(side*sign+direction*0.35f,Vector3.up),size,0.043f,c);
                    }
                }
            }
            MeshObject("Feathered Fronds",mesh.Save("Fern Island"),grass,go.transform,false);
            return SavePrefab(go);
        }

        static GameObject Flower()
        {
            var go=new GameObject("Starlace Moonflowers"); var stems=new MeshData(); var petals=new MeshData();
            for(int f=0;f<9;f++)
            {
                Vector3 origin=new Vector3(R(-0.7f,0.7f),0,R(-0.6f,0.6f));
                Vector3 tip=origin+new Vector3(R(-0.15f,0.15f),R(0.3f,0.7f),R(-0.15f,0.15f));
                stems.Tube(Curve(origin,Vector3.Lerp(origin,tip,0.5f)+Vector3.right*0.08f,tip,5),0.012f,0.007f,5,new Color(0.07f,0.2f,0.22f,0));
                for(int p=0;p<6;p++)
                {
                    Color c=Color.Lerp(Cyan,new Color(0.58f,0.4f,0.95f),f/9f);
                    petals.Leaf(tip,Quaternion.Euler(-12,p*60+f*33,0),0.19f,0.07f,c);
                }
                petals.Ellipsoid(tip+Vector3.up*0.012f,new Vector3(0.042f,0.033f,0.042f),new Color(0.6f,0.92f,1,0),8,4);
                for(int p=0;p<3;p++) stems.Leaf(Vector3.Lerp(origin,tip,p*0.25f),Quaternion.Euler(-30,p*120+f*33,0),0.24f,0.055f,new Color(0.06f,0.19f,0.25f,1));
            }
            MeshObject("Slender Stems",stems.Save("Moonflower Stems"),stem,go.transform,false);
            MeshObject("Six Petal Stars",petals.Save("Moonflower Petals"),bloom,go.transform,false);
            return SavePrefab(go);
        }

        static GameObject Mushrooms()
        {
            var go=new GameObject("Opaline Mushroom Family"); var stalks=new MeshData(); var caps=new MeshData();
            for(int m=0;m<7;m++)
            {
                Vector3 origin=new Vector3(R(-0.55f,0.55f),0,R(-0.5f,0.5f));
                float h=R(0.14f,0.48f), radius=h*R(0.5f,0.85f);
                Vector3 tip=origin+new Vector3(h*0.15f,h,0);
                stalks.Tube(Curve(origin,origin+Vector3.up*h*0.5f,tip,6),h*0.1f,h*0.065f,8,new Color(0.22f,0.31f,0.4f,0));
                Color c=Color.Lerp(new Color(0.08f,0.65f,0.85f,0),new Color(0.43f,0.22f,0.72f,0),m/7f);
                caps.Ellipsoid(tip,new Vector3(radius,h*0.22f,radius),c,18,8);
                for(int dot=0;dot<9;dot++)
                {
                    float a=dot*2.399f; float r=radius*R(0.2f,0.8f);
                    Vector3 pos=tip+new Vector3(Mathf.Cos(a)*r,h*0.22f*Mathf.Sqrt(1-r*r/(radius*radius)),Mathf.Sin(a)*r);
                    caps.Ellipsoid(pos,Vector3.one*h*0.025f,new Color(0.6f,0.92f,1,0),6,3);
                }
            }
            MeshObject("Curved Stalks",stalks.Save("Mushroom Stalks"),stem,go.transform,false);
            MeshObject("Opaline Caps",caps.Save("Mushroom Caps"),mushroom,go.transform,false);
            return SavePrefab(go);
        }

        static GameObject Rock()
        {
            var go=new GameObject("Weathered Slate Group"); var mesh=new MeshData();
            for(int i=0;i<4;i++)
                mesh.Ellipsoid(new Vector3(R(-0.8f,0.8f),0.12f,R(-0.6f,0.6f)),new Vector3(R(0.4f,0.9f),R(0.3f,0.65f),R(0.3f,0.7f)),Color.white,13,8,0.15f);
            MeshObject("Rounded Slate",mesh.Save("Slate Group"),stone,go.transform,true);
            return SavePrefab(go);
        }

        static void Lighting(Transform parent)
        {
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.ExponentialSquared;
            // RenderSettings colors are authored in sRGB; match the sky shader's linear horizon.
            RenderSettings.fogColor=new Color(0.28f,0.32f,0.44f);
            RenderSettings.fogDensity=0.021f;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(0.30f,0.32f,0.46f);
            RenderSettings.ambientEquatorColor=new Color(0.19f,0.20f,0.32f);
            RenderSettings.ambientGroundColor=new Color(0.085f,0.105f,0.17f);
            RenderSettings.reflectionIntensity=0.3f;
            RenderSettings.skybox=Mat("Moonwing Starfield", "Moonwing/Moonlit Sky",Color.white);
            Light key=GameObject.Find("Directional Light").GetComponent<Light>();
            key.color=new Color(0.78f,0.80f,1);
            key.intensity=1.05f;
            key.transform.rotation=Quaternion.Euler(38,-28,0);
            key.shadowStrength=0.72f;
            RenderSettings.sun=key;
            DynamicGI.UpdateEnvironment();
            Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name="Moonwing Clearing Atmosphere";
            AssetDatabase.CreateAsset(profile,Root+"/Settings/Moonwing Clearing Atmosphere.asset");
            var b=profile.Add<Bloom>(true); b.threshold.Override(1.05f); b.intensity.Override(0.38f); b.scatter.Override(0.62f); b.highQualityFiltering.Override(false);
            var tone=profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>(true); color.postExposure.Override(0.3f); color.contrast.Override(8); color.saturation.Override(6);
            var vignette=profile.Add<Vignette>(true); vignette.intensity.Override(0.16f); vignette.smoothness.Override(0.4f);
            foreach(var component in profile.components) AssetDatabase.AddObjectToAsset(component,profile);
            var volume=new GameObject("Moonwing Atmosphere - Global Volume"); volume.transform.SetParent(parent,false);
            var v=volume.AddComponent<Volume>(); v.isGlobal=true; v.priority=10; v.sharedProfile=profile;
        }

        static void Particles(Transform parent)
        {
            var go=new GameObject("Quiet Pixie Drift"); go.transform.SetParent(parent,false); go.transform.localPosition=new Vector3(0,1.1f,8);
            var ps=go.AddComponent<ParticleSystem>(); ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.useAutoRandomSeed=false; ps.randomSeed=73129;
            var main=ps.main; main.loop=true; main.prewarm=true; main.duration=12; main.startLifetime=new ParticleSystem.MinMaxCurve(7,12); main.startSpeed=new ParticleSystem.MinMaxCurve(0.025f,0.09f); main.startSize=new ParticleSystem.MinMaxCurve(0.025f,0.065f); main.maxParticles=85; main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.startColor=new ParticleSystem.MinMaxGradient(new Color(0.25f,0.8f,1,0.7f),new Color(0.6f,0.4f,1,0.5f));
            var emission=ps.emission; emission.rateOverTime=6;
            var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Box; shape.scale=new Vector3(23,3,26);
            var noise=ps.noise; noise.enabled=true; noise.strength=0.16f; noise.frequency=0.25f; noise.scrollSpeed=0.12f; noise.quality=ParticleSystemNoiseQuality.Low;
            var life=ps.colorOverLifetime; life.enabled=true; var gradient=new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(0.65f,0.2f),new GradientAlphaKey(0.45f,0.75f),new GradientAlphaKey(0,1)}); life.color=gradient;
            var renderer=ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=motes; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            var mist=new GameObject("Low Mist - Back Tree Line"); mist.transform.SetParent(parent,false); mist.transform.localPosition=new Vector3(0,0.8f,24);
            var haze=mist.AddComponent<ParticleSystem>(); haze.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            haze.useAutoRandomSeed=false; haze.randomSeed=2931;
            var hm=haze.main; hm.loop=true; hm.prewarm=true; hm.duration=24; hm.startLifetime=24; hm.startSpeed=0.04f; hm.startSize=new ParticleSystem.MinMaxCurve(5,9); hm.maxParticles=18; hm.startColor=new Color(0.22f,0.28f,0.5f,0.018f); hm.simulationSpace=ParticleSystemSimulationSpace.World;
            var he=haze.emission; he.rateOverTime=0.6f;
            var hs=haze.shape; hs.shapeType=ParticleSystemShapeType.Box; hs.scale=new Vector3(40,0.4f,12);
            var hc=haze.colorOverLifetime; hc.enabled=true; hc.color=gradient;
            var hr=haze.GetComponent<ParticleSystemRenderer>(); hr.sharedMaterial=motes; hr.shadowCastingMode=ShadowCastingMode.Off; hr.receiveShadows=false;
        }

        public static void Validate()
        {
            var root=GameObject.Find(GroupName);
            if(root==null) throw new InvalidOperationException("Missing clearing.");
            if(root.GetComponentsInChildren<Collider>(true).Length!=0) throw new InvalidOperationException("Visuals must not add colliders.");
            if(root.GetComponentsInChildren<MonoBehaviour>(true).Any(x=>x==null)) throw new InvalidOperationException("Missing component in clearing.");
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach(var m in renderer.sharedMaterials)
                    if(m==null || m.shader==null || ShaderUtil.ShaderHasError(m.shader)) throw new InvalidOperationException("Missing or broken material on "+renderer.name);
            var spawner=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>();
            if(spawner.totalAssassins!=1 || spawner.player==null || spawner.assassinPrefab==null || spawner.victoryPanel==null) throw new InvalidOperationException("Spawner baseline changed.");
            var player=GameObject.Find("Player");
            if(player.GetComponent<PlayerShoot>().arrowPrefab==null || player.GetComponent<FairyMagic>().moonlightBar==null || player.GetComponent<FairyMagic>().capturedPanel==null || Camera.main.GetComponent<CameraFollow>().target!=player.transform) throw new InvalidOperationException("Gameplay references changed.");
            int triangles=root.GetComponentsInChildren<MeshFilter>().Sum(x=>x.sharedMesh.triangles.Length/3);
            Directory.CreateDirectory("Logs/ClearingReview");
            File.WriteAllText("Logs/ClearingReview/Validation.txt","Clearing renderers: "+root.GetComponentsInChildren<Renderer>().Length+"\nMesh triangles across instances: "+triangles+"\nAdded colliders: 0\nGameplay references present; totalAssassins = 1\nMaterial shader checks passed\n");
            Debug.Log("MOONWING_VALIDATION_PASSED triangles="+triangles);
        }

        public static void Capture()
        {
            Camera camera=Camera.main;
            var target=new RenderTexture(1440,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            target.Create();
            var previousTarget=camera.targetTexture;
            var previousActive=RenderTexture.active;
            try
            {
                foreach(var ps in GameObject.Find(GroupName).GetComponentsInChildren<ParticleSystem>()) ps.Simulate(8,true,true);
                // The isolated capture excludes the UI overlay so the environment can be reviewed.
                var canvas=GameObject.Find("Canvas"); bool wasActive=canvas.activeSelf; canvas.SetActive(false);
                try
                {
                    // StandardRequest executes the full camera stack, including volume/fog updates.
                    var request=new RenderPipeline.StandardRequest { destination=target };
                    RenderPipeline.SubmitRenderRequest(camera,request);
                    RenderTexture.active=target;
                    var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);
                    texture.ReadPixels(new Rect(0,0,1440,900),0,0); texture.Apply();
                    Directory.CreateDirectory("Logs/ClearingReview");
                    File.WriteAllBytes("Logs/ClearingReview/StartingCamera.png",texture.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(texture);
                }
                finally { canvas.SetActive(wasActive); }
            }
            finally { camera.targetTexture=previousTarget; RenderTexture.active=previousActive; target.Release(); UnityEngine.Object.DestroyImmediate(target); }
            Debug.Log("MOONWING_CAPTURE_COMPLETE");
        }

        static Transform Group(string name,Transform parent) { var go=new GameObject(name); go.transform.SetParent(parent,false); return go.transform; }
        static float R(float a,float b) { return Random.Range(a,b); }
        static GameObject Place(GameObject prefab,Transform parent,Vector3 position,float yaw,float scale)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);
            go.transform.localPosition=position; go.transform.localRotation=Quaternion.Euler(0,yaw,0); go.transform.localScale=Vector3.one*scale; return go;
        }
        static GameObject SavePrefab(GameObject go)
        {
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+go.name+".prefab");
            UnityEngine.Object.DestroyImmediate(go); return prefab;
        }
        static void MeshObject(string name,Mesh mesh,Material material,Transform parent,bool shadows)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>(); r.sharedMaterial=material;
            r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;
            r.receiveShadows=true;
        }
        static Vector3[] Curve(Vector3 a,Vector3 b,Vector3 c,int count)
        {
            var points=new Vector3[count];
            for(int i=0;i<count;i++) { float t=i/(float)(count-1); points[i]=(1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c; }
            return points;
        }

        sealed class MeshData
        {
            readonly List<Vector3> vertices=new List<Vector3>();
            readonly List<Color> colors=new List<Color>();
            readonly List<int> indices=new List<int>();
            int Vertex(Vector3 p,Color c) { vertices.Add(p); colors.Add(c); return vertices.Count-1; }
            void Triangle(int a,int b,int c) { indices.Add(a); indices.Add(b); indices.Add(c); }
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color)
            {
                int n=Vertex(a,color); Vertex(b,color); Vertex(c,color); Vertex(d,color); Triangle(n,n+1,n+2); Triangle(n,n+2,n+3);
            }
            public void Leaf(Vector3 p,Quaternion rotation,float length,float width,Color color)
            {
                int n=vertices.Count;
                Vertex(p,color);
                Vertex(p+rotation*new Vector3(-width,length*0.08f,length*0.42f),color);
                Vertex(p+rotation*new Vector3(0,length*0.18f,length*0.53f),color);
                Vertex(p+rotation*new Vector3(width,length*0.08f,length*0.42f),color);
                Vertex(p+rotation*new Vector3(0,length*0.04f,length),color);
                Triangle(n,n+1,n+2); Triangle(n,n+2,n+3); Triangle(n+1,n+4,n+2); Triangle(n+2,n+4,n+3);
            }
            public void Tube(Vector3[] points,float start,float end,int sides,Color color)
            {
                int n=vertices.Count;
                for(int i=0;i<points.Length;i++)
                {
                    Vector3 tangent=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(i-1,0)]).normalized;
                    Vector3 x=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>0.9f?Vector3.right:Vector3.up).normalized;
                    Vector3 y=Vector3.Cross(tangent,x).normalized;
                    float radius=Mathf.Lerp(start,end,i/(float)(points.Length-1));
                    for(int j=0;j<sides;j++)
                    {
                        float a=j*Mathf.PI*2/sides; float r=radius*(1+0.055f*Mathf.Sin(j*5+i*0.7f));
                        Vertex(points[i]+(x*Mathf.Cos(a)+y*Mathf.Sin(a))*r,color);
                    }
                }
                for(int i=0;i<points.Length-1;i++) for(int j=0;j<sides;j++)
                {
                    int a=n+i*sides+j,b=n+i*sides+(j+1)%sides,c=b+sides,d=a+sides;
                    Triangle(a,b,c); Triangle(a,c,d);
                }
            }
            public void Ellipsoid(Vector3 p,Vector3 scale,Color color,int sides,int rings,float roughness=0)
            {
                int n=vertices.Count;
                for(int i=0;i<=rings;i++) for(int j=0;j<sides;j++)
                {
                    float theta=i*Mathf.PI/rings,a=j*Mathf.PI*2/sides;
                    Vector3 v=new Vector3(Mathf.Sin(theta)*Mathf.Cos(a),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(a));
                    float r=1+roughness*Mathf.Sin(j*2.3f+i*1.7f);
                    Vertex(p+Vector3.Scale(v,scale)*r,color);
                }
                for(int i=0;i<rings;i++) for(int j=0;j<sides;j++)
                {
                    int a=n+i*sides+j,b=n+i*sides+(j+1)%sides,c=b+sides,d=a+sides;
                    Triangle(a,b,c); Triangle(a,c,d);
                }
            }
            public Mesh Save(string name)
            {
                var mesh=new Mesh { name=name, indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16 };
                mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetTriangles(indices,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
                AssetDatabase.CreateAsset(mesh,Root+"/Meshes/"+name+".asset"); return mesh;
            }
        }
    }
}

