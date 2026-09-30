using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        static void ExtendSubmissionForest()
        {
            var root=new GameObject("Moonwing - Submission Atmosphere").transform;
            var mid=Group("Outer Walkable Groves",root); var distance=Group("Distant Mist Silhouettes - No Colliders",root);
            var ground=GameObject.Find("Ground").transform; var scale=ground.localScale; ground.localScale=new Vector3(10,scale.y,10);
            var floor=new MoonwingMesh(); floor.Quad(new Vector3(-200,0.006f,-200),new Vector3(-200,0.006f,200),new Vector3(200,0.006f,200),new Vector3(200,0.006f,-200),Color.white);
            Mesh("Submission Endless Mist Floor",floor,AssetDatabase.LoadAssetAtPath<Material>(Clearing+"/Materials/Moonlit Moss and Earth.mat"),root);
            var trees=new GameObject[3];
            for(int i=0;i<trees.Length;i++) trees[i]=SubmissionTree(i);
            for(int i=0;i<26;i++)
            {
                float a=i*2.399963f; float radius=R(29,42);
                Place(trees[i%3],mid,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),R(0,360),R(0.85f,1.4f));
            }
            for(int i=0;i<32;i++)
            {
                float a=i*2.399963f; float radius=R(48,68);
                Place(trees[(i+1)%3],distance,new Vector3(Mathf.Cos(a)*radius,-0.3f,Mathf.Sin(a)*radius),R(0,360),R(0.85f,1.7f));
            }
            string[] plants={"Indigo Fern Island","Starlace Moonflowers","Opaline Mushroom Family","Weathered Slate Group"};
            for(int i=0;i<42;i++)
            {
                float a=i*2.399963f, radius=R(27,40);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/"+plants[i%4]+".prefab");
                var plant=Place(prefab,mid,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),R(0,360),R(0.6f,1.0f));
                if(i%4==2) plant.transform.Find("Opaline Caps").GetComponent<MeshRenderer>().sharedMaterial=LoadMat(i%3==0?"Moonwell Mushroom Caps":"Quiet Lavender Mushroom Caps");
            }
            var mistMaterial=new Material(Shader.Find("Moonwing/Boundary Mist")); AssetDatabase.CreateAsset(mistMaterial,Polish+"/Materials/Boundary Mist.mat");
            foreach(float radius in new[]{43f,57f})
            {
                var ring=new MoonwingMesh();
                float y=radius==43?0.7f:2.1f;
                ring.Quad(new Vector3(-90,y,-90),new Vector3(-90,y,90),new Vector3(90,y,90),new Vector3(90,y,-90),Color.white);
                Mesh("Submission Mist Layer "+radius,ring,mistMaterial,distance);
            }
            // One small system for the perimeter, not one emitter/light for each plant.
            var particles=Motes("Outer Glade Fireflies",mid,Vector3.up,dust?dust:LoadMat("Moonlight Pixie Dust"),new Color(0.4f,0.65f,1,0.28f),new Color(0.65f,0.5f,0.9f,0.22f),36,3,0.055f,35,10);
            var shape=particles.shape; shape.shapeType=ParticleSystemShapeType.Circle; shape.radius=34; shape.radiusThickness=0.18f;
            particles.transform.localRotation=Quaternion.Euler(90,0,0);
        }

        static GameObject SubmissionTree(int variant)
        {
            var go=new GameObject("Submission Mist Tree "+variant); var bark=new MoonwingMesh(); var leaves=new MoonwingMesh();
            float height=6.7f+variant*1.3f;
            Color timber=new Color(0.055f,0.065f,0.10f), foliage=new Color(0.21f+variant*0.018f,0.16f,0.33f+variant*0.02f);
            bark.Tube(MoonwingMesh.Curve(Vector3.zero,new Vector3(0.20f*(variant-1),height*0.5f,0),new Vector3(0.35f*(variant-1),height,0.18f),9),0.32f,0.045f,timber,9);
            for(int i=0;i<5;i++)
            {
                float a=i*2.399963f+variant;
                var tip=new Vector3(Mathf.Cos(a)*(1.7f+variant*0.2f),height*0.65f+i*0.45f,Mathf.Sin(a)*2);
                bark.Tube(MoonwingMesh.Curve(Vector3.up*(height*0.47f+i*0.38f),tip-Vector3.up*0.4f,tip),0.12f,0.025f,timber,6);
                for(int leaf=0;leaf<95;leaf++)
                {
                    Vector3 p=tip+Vector3.Scale(UnityEngine.Random.insideUnitSphere,new Vector3(1.8f,variant==1?1.2f:0.7f,1.45f));
                    Quaternion rotation=Quaternion.Euler(R(-55,55),R(0,360),R(-25,25));
                    Vector3 along=rotation*Vector3.forward*R(0.28f,0.52f), across=rotation*Vector3.right*R(0.075f,0.16f);
                    leaves.Quad(p-along,p-across,p+along,p+across,foliage*R(0.8f,1.3f));
                }
                var root=new Vector3(Mathf.Cos(a)*1.2f,0.025f,Mathf.Sin(a)*1.2f);
                bark.Tube(MoonwingMesh.Curve(Vector3.up*0.3f,root*0.45f,root),0.10f,0.015f,timber,5);
            }
            Mesh("Submission Distant Bark "+variant,bark,AssetDatabase.LoadAssetAtPath<Material>(Clearing+"/Materials/Silver Indigo Bark.mat"),go.transform);
            // Use known shared botanical materials; no new lighting/shadows on silhouettes.
            var canopy=Mesh("Submission Distant Canopy "+variant,leaves,LoadMat("Moonfairy Silks"),go.transform); canopy.receiveShadows=false;
            var trunk=go.GetComponentInChildren<MeshRenderer>(); if(!trunk.sharedMaterial) trunk.sharedMaterial=LoadMat("Assassin Shadow Cloth");
            return Prefab(go);
        }

        public static void FinalizeSubmissionForest()
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath);
            UnityEngine.Random.InitState(77133);
            for(int i=0;i<3;i++) SubmissionTree(i);
            foreach(float radius in new[]{43f,57f})
            {
                float y=radius==43?0.7f:2.1f; var mesh=new MoonwingMesh();
                mesh.Quad(new Vector3(-90,y,-90),new Vector3(-90,y,90),new Vector3(90,y,90),new Vector3(90,y,-90),Color.white);
                var saved=mesh.Save("Submission Mist Layer "+radius);
                GameObject.Find("Submission Mist Layer "+radius).GetComponent<MeshFilter>().sharedMesh=saved;
            }
            AssetDatabase.SaveAssets(); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            ValidateSubmission(); SubmissionPreviews();
        }
    }
}
