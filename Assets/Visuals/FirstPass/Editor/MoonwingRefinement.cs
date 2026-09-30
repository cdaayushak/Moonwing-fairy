using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        static readonly Color Ivory = new Color(0.88f,0.85f,0.78f);
        static readonly Color RoseGold = new Color(0.60f,0.37f,0.28f);
        static readonly Color Brown = new Color(0.075f,0.032f,0.026f);
        static Material pearlCloth, roseMetal, brownHair, gossamer;

        public static void RefineBatch()
        {
            EditorSceneManager.OpenScene(ScenePath);
            if (GameObject.Find("Second Pass - Garden Details")) throw new InvalidOperationException("Refinement already applied; edit the saved assets.");
            var state=UnityEngine.Random.state;
            try
            {
                UnityEngine.Random.InitState(18429);
                silk=LoadMat("Moonfairy Silks"); silver=LoadMat("Moon Silver"); skin=LoadMat("Warm Porcelain");
                hair=LoadMat("Lavender Silver Hair"); glow=LoadMat("Moon Goddess Light"); veil=LoadMat("Translucent Moon Wings");
                warm=LoadMat("Caretaker Lantern Gold"); shadow=LoadMat("Assassin Shadow Cloth"); red=LoadMat("Shadow Sigils");
                dust=LoadMat("Moonlight Pixie Dust"); smoke=LoadMat("Violet Shadow Smoke"); trail=LoadMat("Flowing Moonlight");
                pearlCloth=Mat("Pearl Ivory Silk","Moonwing/Clearing Botanical",Color.white,0.16f);
                roseMetal=Mat("Rose Gold Ornament","Moonwing/Clearing Botanical",Color.white,0.23f);
                brownHair=Mat("Deep Chocolate Hair","Moonwing/Clearing Botanical",Color.white,0.025f);
                gossamer=Mat("Gossamer Moonwing","Moonwing/Gossamer",new Color(0.85f,0.91f,1,0.55f),0.8f);
                RefinedFairy(); RefinedAssassin(); RefinedArrow(); RefinedInterface(); RefinedGarden();
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                Validate(); CapturePreviews();
                Debug.Log("MOONWING_SECOND_PASS_BUILT");
            }
            finally { UnityEngine.Random.state=state; }
        }
        static Material LoadMat(string name) => AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+name+".mat");

        public static void FinalizeRefinement()
        {
            EditorSceneManager.OpenScene(ScenePath);
            pearlCloth=LoadMat("Pearl Ivory Silk"); roseMetal=LoadMat("Rose Gold Ornament"); brownHair=LoadMat("Deep Chocolate Hair"); gossamer=LoadMat("Gossamer Moonwing");
            skin=LoadMat("Warm Porcelain"); silver=LoadMat("Moon Silver"); dust=LoadMat("Moonlight Pixie Dust");
            RefinedFairy(); AssetDatabase.SaveAssets(); Validate(); CapturePreviews();
        }

        static void RefinedFairy()
        {
            var go=new GameObject("Moonfairy - Moon Goddess Silhouette");
            var dress=new MoonwingMesh(); var drapes=new MoonwingMesh(); var trim=new MoonwingMesh(); var face=new MoonwingMesh(); var locks=new MoonwingMesh(); var gems=new MoonwingMesh();
            Color skinTone=new Color(0.67f,0.48f,0.40f);
            // Narrow waist, long torso and irregular layered hems, avoiding a rigid bell/cone.
            dress.Lathe(Vector3.zero,new[]{new Vector2(0.34f,-0.94f),new Vector2(0.32f,-0.77f),new Vector2(0.26f,-0.45f),new Vector2(0.19f,-0.12f),new Vector2(0.135f,0.14f),new Vector2(0.185f,0.35f),new Vector2(0.20f,0.46f),new Vector2(0.085f,0.51f)},Ivory,48,0.78f,0.035f);
            for(int i=0;i<9;i++)
            {
                float a=i*Mathf.PI*2/9;
                // Individual overlapping silk panels have curved, tapering ends and soft folds.
                var points=new Vector3[13];
                for(int j=0;j<points.Length;j++)
                {
                    float t=j/12f, r=Mathf.Lerp(0.14f,0.40f,t);
                    points[j]=new Vector3(Mathf.Cos(a)*r,0.14f-t*(0.91f+0.15f*Mathf.Sin(a*2)),Mathf.Sin(a)*r*0.83f);
                }
                for(int j=0;j<12;j++)
                {
                    float t=j/12f, next=(j+1)/12f;
                    Vector3 tangent=new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a))*0.12f;
                    drapes.Quad(points[j]-tangent*(0.5f+t),points[j+1]-tangent*(0.5f+next),points[j+1]+tangent*(0.5f+next),points[j]+tangent*(0.5f+t),Color.Lerp(Ivory,new Color(0.68f,0.66f,0.63f),i%3*0.07f));
                }
                trim.Tube(points,0.004f,0.002f,new Color(0.55f,0.53f,0.49f),5);
            }
            trim.Tube(Circle(new Vector3(0,0.135f,0),0.147f,0.115f),0.013f,0.013f,RoseGold,8);
            trim.Tube(MoonwingMesh.Curve(new Vector3(-0.16f,0.45f,0.10f),new Vector3(0,0.36f,0.18f),new Vector3(0.16f,0.45f,0.10f)),0.007f,0.007f,Pearl);
            face.Line(new Vector3(0,0.47f,0),new Vector3(0,0.62f,0),0.055f,skinTone);
            face.Ellipsoid(new Vector3(0,0.755f,0.018f),new Vector3(0.122f,0.167f,0.117f),skinTone,28,20);
            face.Ellipsoid(new Vector3(0,0.741f,0.134f),new Vector3(0.017f,0.029f,0.020f),skinTone,12,8);
            face.Tube(MoonwingMesh.Curve(new Vector3(-0.031f,0.698f,0.116f),new Vector3(0,0.692f,0.133f),new Vector3(0.031f,0.698f,0.116f)),0.003f,0.003f,new Color(0.35f,0.16f,0.14f),6);
            foreach(int side in new[]{-1,1})
            {
                // Arms gathered in a forward bow-bearing pose.
                Vector3 hand=side<0?new Vector3(-0.26f,0.22f,0.43f):new Vector3(0.05f,0.25f,0.27f);
                dress.Tube(MoonwingMesh.Curve(new Vector3(side*0.18f,0.43f,0),new Vector3(side*0.31f,0.16f,0.15f),hand),0.08f,0.048f,Ivory,12);
                drapes.Ribbon(MoonwingMesh.Curve(new Vector3(side*0.28f,0.23f,0.13f),new Vector3(side*0.39f,-0.05f,0.11f),new Vector3(side*0.34f,-0.32f,0.16f)),0.10f,Ivory);
                face.Ellipsoid(hand,new Vector3(0.045f,0.06f,0.045f),skinTone,12,8);
                trim.Ellipsoid(new Vector3(side*0.115f,-0.92f,0.035f),new Vector3(0.059f,0.06f,0.13f),Ivory);
                locks.Ellipsoid(new Vector3(side*0.05f,0.778f,0.128f),new Vector3(0.021f,0.007f,0.006f),Brown,12,6);
                locks.Line(new Vector3(side*0.032f,0.812f,0.121f),new Vector3(side*0.078f,0.804f,0.116f),0.003f,Brown);
                trim.Line(new Vector3(side*0.142f,0.72f,0.014f),new Vector3(side*0.15f,0.60f,0.02f),0.005f,RoseGold);
                gems.Diamond(new Vector3(side*0.15f,0.59f,0.02f),new Vector3(0.012f,0.024f,0.012f),Pearl);
            }
            locks.Ellipsoid(new Vector3(0,0.818f,-0.03f),new Vector3(0.142f,0.134f,0.133f),Brown,28,16);
            for(int i=0;i<13;i++)
            {
                float x=(i-6)*0.026f;
                locks.Tube(MoonwingMesh.Curve(new Vector3(x,0.83f,-0.09f),new Vector3(x*1.6f,0.26f,-0.23f),new Vector3(x*1.6f+0.03f*Mathf.Sin(i),-0.61f+(i%3)*0.04f,-0.27f),24),0.045f,0.009f,Color.Lerp(Brown,new Color(0.16f,0.073f,0.05f),i%4*0.11f),10);
            }
            foreach(int side in new[]{-1,1}) locks.Tube(MoonwingMesh.Curve(new Vector3(side*0.10f,0.88f,0.065f),new Vector3(side*0.21f,0.55f,0.12f),new Vector3(side*0.18f,0.23f,0.15f),18),0.032f,0.005f,Brown,9);
            // Small comb and a pendant replace the oversized pointed crown.
            trim.Tube(MoonwingMesh.Curve(new Vector3(-0.13f,0.89f,-0.07f),new Vector3(0,0.97f,-0.03f),new Vector3(0.13f,0.89f,-0.07f)),0.009f,0.009f,RoseGold);
            gems.Diamond(new Vector3(0,0.375f,0.164f),new Vector3(0.023f,0.043f,0.014f),Pearl);
            Mesh("Refined Ivory Foundation",dress,pearlCloth,go.transform); Mesh("Refined Layered Silk Panels",drapes,pearlCloth,go.transform);
            Mesh("Refined Rose Gold Embroidery",trim,roseMetal,go.transform); Mesh("Refined Porcelain Features",face,skin,go.transform);
            Mesh("Refined Chocolate Hair",locks,brownHair,go.transform); Mesh("Refined Moonstone Jewelry",gems,silver,go.transform);
            var motion=go.AddComponent<MoonwingVisualMotion>(); motion.wingAngle=5; motion.floatHeight=0.014f;
            foreach(int side in new[]{-1,1})
            {
                var wing=Group(side<0?"Left Moonwing":"Right Moonwing",go.transform); wing.localPosition=new Vector3(side*0.10f,0.22f,-0.18f); wing.localScale=new Vector3(0.65f,0.72f,0.8f);
                var membrane=new MoonwingMesh(); var veins=new MoonwingMesh(); membrane.Wing(side,true,Pearl,veins); membrane.Wing(side,false,new Color(0.75f,0.71f,0.87f),veins);
                Mesh(side<0?"Refined Left Gossamer":"Refined Right Gossamer",membrane,gossamer,wing);
                Mesh(side<0?"Refined Left Wing Veins":"Refined Right Wing Veins",veins,gossamer,wing);
                if(side<0) motion.leftWing=wing; else motion.rightWing=wing;
            }
            var bow=new MoonwingMesh(); var stringMesh=new MoonwingMesh();
            Vector3 grip=new Vector3(-0.26f,0.22f,0.43f);
            foreach(int side in new[]{-1,1})
            {
                bow.Tube(MoonwingMesh.Curve(grip,new Vector3(-0.27f,0.22f+side*0.30f,0.63f),new Vector3(-0.25f,0.22f+side*0.61f,0.38f),20),0.025f,0.009f,Ivory,9);
                stringMesh.Line(grip+new Vector3(0,side*0.61f,-0.05f),grip+new Vector3(0,0,-0.22f),0.003f,Pearl);
            }
            bow.Line(grip-Vector3.up*0.075f,grip+Vector3.up*0.075f,0.031f,RoseGold);
            Mesh("Refined Crescent Longbow",bow,roseMetal,go.transform); Mesh("Refined Silver Bowstring",stringMesh,silver,go.transform);
            Motes("Moon Goddess Pixie Aura",go.transform,Vector3.zero,dust,new Color(0.5f,0.75f,1,0.36f),new Color(0.75f,0.65f,1,0.32f),24,5,0.04f,0.55f,3);
            Motes("Faint Silhouette Wisps",go.transform,new Vector3(0,0.05f,0),dust,new Color(0.5f,0.65f,1,0.028f),new Color(0.7f,0.55f,1,0.025f),6,1,0.30f,0.38f,3);
            Prefab(go);
        }

        static void RefinedAssassin()
        {
            string path=Root+"/Prefabs/Assassin - Hooded Shadow.prefab";
            var go=PrefabUtility.LoadPrefabContents(path);
            try
            {
                go.transform.Find("Assassin Cowl and Face").localScale=new Vector3(0.87f,0.96f,0.88f);
                var layers=new MoonwingMesh(); var armor=new MoonwingMesh(); var blade=new MoonwingMesh();
                var charcoal=new Color(0.035f,0.034f,0.048f);
                for(int i=0;i<7;i++)
                {
                    float a=i*Mathf.PI*2/7;
                    Vector3 p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                    layers.Tube(MoonwingMesh.Curve(Vector3.Scale(p,new Vector3(0.22f,0,0.20f))+Vector3.up*0.02f,Vector3.Scale(p,new Vector3(0.32f,0,0.28f))-Vector3.up*0.48f,Vector3.Scale(p,new Vector3(0.34f,0,0.30f))-Vector3.up*0.91f),0.09f,0.065f,charcoal,7);
                }
                for(int i=0;i<4;i++) armor.Box(new Vector3(0,0.08f+i*0.075f,0.275f),new Vector3(0.37f-i*0.015f,0.055f,0.045f),new Color(0.09f,0.075f,0.105f));
                foreach(int side in new[]{-1,1})
                {
                    for(int i=0;i<3;i++) armor.Box(new Vector3(side*(0.30f+i*0.035f),0.40f-i*0.035f,0.015f),new Vector3(0.16f,0.035f,0.32f),charcoal);
                    armor.Box(new Vector3(side*0.365f,-0.10f,0.12f),new Vector3(0.13f,0.19f,0.15f),charcoal);
                }
                armor.Box(new Vector3(0,0.59f,0.228f),new Vector3(0.25f,0.095f,0.035f),charcoal);
                // Original fantasy sheathed blade; visual only, no new combat or collision component.
                blade.Tube(MoonwingMesh.Curve(new Vector3(-0.4f,-0.22f,0.12f),new Vector3(-0.48f,-0.57f,0.40f),new Vector3(-0.56f,-0.87f,0.69f)),0.032f,0.012f,new Color(0.15f,0.15f,0.19f),7);
                blade.Line(new Vector3(-0.39f,-0.2f,0.10f),new Vector3(-0.33f,0.02f,-0.10f),0.028f,new Color(0.09f,0.025f,0.037f));
                blade.Box(new Vector3(-0.4f,-0.2f,0.12f),new Vector3(0.15f,0.03f,0.10f),RoseGold*0.35f);
                Mesh("Refined Assassin Layered Robes",layers,shadow,go.transform); Mesh("Refined Assassin Lamellar and Mask",armor,silk,go.transform); Mesh("Refined Assassin Dark Blade",blade,silver,go.transform);
                PrefabUtility.SaveAsPrefabAsset(go,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(go); }
        }

        static void RefinedArrow()
        {
            string path=Root+"/Prefabs/Enchanted Arrow - Igniting Moonlight.prefab";
            var go=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var magic=go.GetComponent<MoonwingArrowMagic>();
                var shaft=new MoonwingMesh();
                shaft.Line(new Vector3(0,0,-0.55f),new Vector3(0,0,0.30f),0.017f,Ivory);
                shaft.Diamond(new Vector3(0,0,0.35f),new Vector3(0.085f,0.026f,0.19f),new Color(0.68f,0.73f,0.81f));
                for(int i=0;i<3;i++)
                {
                    float a=i*Mathf.PI*2/3; var side=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                    shaft.Quad(new Vector3(0,0,-0.24f),side*0.125f+new Vector3(0,0,-0.38f),side*0.09f+new Vector3(0,0,-0.57f),new Vector3(0,0,-0.53f),Ivory);
                }
                magic.silverShaft.GetComponent<MeshFilter>().sharedMesh=shaft.Save("Refined Physical Arrow Shaft Head Feathers");
                magic.silverShaft.sharedMaterial=silver;
                var energy=new MoonwingMesh();
                energy.Line(new Vector3(0,0,-0.51f),new Vector3(0,0,0.28f),0.023f,new Color(0.4f,0.65f,0.83f,0.25f));
                energy.Diamond(new Vector3(0,0,0.35f),new Vector3(0.098f,0.035f,0.20f),new Color(0.65f,0.70f,0.92f,0.15f));
                var renderer=magic.energy.GetComponentInChildren<MeshRenderer>(); renderer.GetComponent<MeshFilter>().sharedMesh=energy.Save("Refined Arrow Enchantment Veil"); renderer.sharedMaterial=veil;
                PrefabUtility.SaveAsPrefabAsset(go,path);
            }
            finally { PrefabUtility.UnloadPrefabContents(go); }
        }
    }
}
