using UnityEditor;
using UnityEngine;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        static GameObject Fairy()
        {
            var go=new GameObject("Moonfairy - Moon Goddess Silhouette");
            var dress=new MoonwingMesh(); var trim=new MoonwingMesh(); var body=new MoonwingMesh(); var locks=new MoonwingMesh(); var jewels=new MoonwingMesh();
            dress.Lathe(Vector3.zero,new[]{new Vector2(0.48f,-0.88f),new Vector2(0.43f,-0.7f),new Vector2(0.3f,-0.34f),new Vector2(0.17f,0.04f)},Lavender,32,0.85f,0.06f);
            dress.Lathe(Vector3.zero,new[]{new Vector2(0.17f,0.02f),new Vector2(0.19f,0.18f),new Vector2(0.235f,0.34f),new Vector2(0.2f,0.42f),new Vector2(0.07f,0.49f)},new Color(0.4f,0.57f,0.77f),24,0.7f);
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;
                Vector3 lower=new Vector3(Mathf.Cos(a)*0.49f,-0.87f,Mathf.Sin(a)*0.42f);
                trim.Tube(MoonwingMesh.Curve(new Vector3(Mathf.Cos(a)*0.18f,0.01f,Mathf.Sin(a)*0.14f),new Vector3(Mathf.Cos(a)*0.26f,-0.38f,Mathf.Sin(a)*0.22f),lower),0.012f,0.007f,Pearl,6);
            }
            trim.Tube(Circle(new Vector3(0,-0.88f,0),0.49f,0.42f),0.014f,0.014f,Pearl,6);
            trim.Tube(Circle(new Vector3(0,0.04f,0),0.185f,0.14f),0.025f,0.025f,Pearl,8);
            body.Ellipsoid(new Vector3(0,0.67f,0.01f),new Vector3(0.185f,0.23f,0.18f),new Color(0.78f,0.62f,0.61f));
            body.Line(new Vector3(0,0.43f,0),new Vector3(0,0.55f,0),0.067f,new Color(0.78f,0.62f,0.61f));
            foreach(int side in new[]{-1,1})
            {
                var arm=MoonwingMesh.Curve(new Vector3(side*0.18f,0.34f,0),new Vector3(side*0.35f,0.17f,0.02f),new Vector3(side*0.41f,-0.07f,0.09f));
                dress.Tube(arm,0.105f,0.065f,new Color(0.43f,0.51f,0.75f));
                body.Ellipsoid(arm[arm.Length-1],new Vector3(0.065f,0.08f,0.055f),new Color(0.78f,0.62f,0.61f));
                trim.Ellipsoid(new Vector3(side*0.13f,-0.91f,0.06f),new Vector3(0.085f,0.09f,0.15f),Pearl);
                jewels.Ellipsoid(new Vector3(side*0.075f,0.69f,0.175f),new Vector3(0.02f,0.03f,0.012f),new Color(0.10f,0.16f,0.3f),10,6);
            }
            locks.Ellipsoid(new Vector3(0,0.77f,-0.035f),new Vector3(0.207f,0.2f,0.19f),new Color(0.40f,0.36f,0.62f));
            for(int i=0;i<9;i++)
            {
                float x=(i-4)*0.047f;
                locks.Tube(MoonwingMesh.Curve(new Vector3(x,0.79f,-0.12f),new Vector3(x*1.5f,0.25f,-0.25f),new Vector3(x*1.65f+0.035f*Mathf.Sin(i),-0.51f,-0.27f),16),0.062f,0.012f,Color.Lerp(Lavender,Pearl,i/12f),8);
            }
            for(int i=-2;i<=2;i++)
            {
                Vector3 p=new Vector3(i*0.073f,0.88f+0.08f*(1-Mathf.Abs(i)/3f),0.08f);
                trim.Line(p-Vector3.up*0.045f,p+Vector3.up*0.04f,0.012f,Pearl);
                jewels.Diamond(p+Vector3.up*0.04f,new Vector3(0.025f,0.045f,0.022f),Cyan);
            }
            jewels.Diamond(new Vector3(0,0.29f,0.175f),new Vector3(0.046f,0.075f,0.025f),Cyan);
            Mesh("Moonfairy Layered Dress",dress,silk,go.transform); Mesh("Moonfairy Silver Embroidery",trim,silver,go.transform);
            Mesh("Moonfairy Face and Hands",body,skin,go.transform); Mesh("Moonfairy Long Hair",locks,hair,go.transform); Mesh("Moonfairy Crown and Moonstone",jewels,glow,go.transform);
            var motion=go.AddComponent<MoonwingVisualMotion>();
            foreach(int side in new[]{-1,1})
            {
                Transform wing=Group(side<0?"Left Moonwing":"Right Moonwing",go.transform); wing.localPosition=new Vector3(side*0.13f,0.25f,-0.19f);
                var membrane=new MoonwingMesh(); var veins=new MoonwingMesh(); membrane.Wing(side,true,Pearl,veins); membrane.Wing(side,false,Lavender,veins);
                Mesh(side<0?"Left Wing Membrane":"Right Wing Membrane",membrane,veil,wing); Mesh(side<0?"Left Silver Wing Veins":"Right Silver Wing Veins",veins,glow,wing);
                if(side<0) motion.leftWing=wing; else motion.rightWing=wing;
            }
            Motes("Moon Goddess Pixie Aura",go.transform,Vector3.zero,dust,new Color(0.35f,0.8f,1,0.65f),new Color(0.7f,0.48f,1,0.55f),30,9,0.065f,0.68f,3);
            Motes("Faint Silhouette Wisps",go.transform,new Vector3(0,0.1f,0),dust,new Color(0.4f,0.6f,1,0.075f),new Color(0.6f,0.4f,1,0.065f),9,2,0.4f,0.43f,3);
            return Prefab(go);
        }

        static GameObject Caretaker()
        {
            var go=new GameObject("Caretaker - Lantern Keeper");
            var robe=new MoonwingMesh(); var head=new MoonwingMesh(); var details=new MoonwingMesh(); var light=new MoonwingMesh();
            robe.Lathe(Vector3.zero,new[]{new Vector2(0.38f,-0.95f),new Vector2(0.37f,-0.67f),new Vector2(0.28f,-0.1f),new Vector2(0.32f,0.3f),new Vector2(0.23f,0.46f),new Vector2(0.10f,0.5f)},new Color(0.27f,0.3f,0.23f),28,0.9f,0.035f);
            head.Ellipsoid(new Vector3(0,0.67f,0.03f),new Vector3(0.22f,0.25f,0.2f),new Color(0.67f,0.48f,0.32f));
            details.Ellipsoid(new Vector3(0,0.79f,-0.04f),new Vector3(0.235f,0.16f,0.19f),new Color(0.56f,0.57f,0.6f));
            details.Tube(MoonwingMesh.Curve(new Vector3(0,0.61f,0.20f),new Vector3(0,0.41f,0.24f),new Vector3(0,0.3f,0.23f)),0.11f,0.02f,new Color(0.7f,0.72f,0.74f));
            details.Tube(Circle(new Vector3(0,-0.16f,0),0.31f,0.275f),0.028f,0.028f,Gold);
            details.Line(new Vector3(0,0.4f,0.27f),new Vector3(0,-0.83f,0.34f),0.028f,Gold);
            foreach(int side in new[]{-1,1})
            {
                Vector3 hand=new Vector3(side*0.44f,-0.02f,0.16f);
                robe.Tube(MoonwingMesh.Curve(new Vector3(side*0.23f,0.35f,0),new Vector3(side*0.43f,0.2f,0.06f),hand),0.16f,0.10f,new Color(0.32f,0.28f,0.23f));
                head.Ellipsoid(hand,new Vector3(0.075f,0.085f,0.065f),new Color(0.67f,0.48f,0.32f));
                details.Ellipsoid(new Vector3(side*0.09f,0.7f,0.215f),new Vector3(0.019f,0.017f,0.009f),new Color(0.06f,0.075f,0.11f),8,5);
            }
            details.Tube(MoonwingMesh.Curve(new Vector3(0.48f,-0.92f,0.12f),new Vector3(0.44f,0.3f,0.12f),new Vector3(0.6f,0.97f,0.12f)),0.045f,0.026f,new Color(0.25f,0.16f,0.12f));
            Vector3 lantern=new Vector3(0.65f,0.55f,0.12f);
            details.Line(new Vector3(0.6f,0.97f,0.12f),lantern,0.014f,Gold);
            details.Box(lantern+Vector3.up*0.15f,new Vector3(0.24f,0.04f,0.2f),Gold); details.Box(lantern-Vector3.up*0.15f,new Vector3(0.24f,0.04f,0.2f),Gold);
            light.Ellipsoid(lantern,new Vector3(0.10f,0.13f,0.08f),new Color(1,0.6f,0.23f));
            Mesh("Caretaker Moss Robe",robe,silk,go.transform); Mesh("Caretaker Gentle Face",head,skin,go.transform); Mesh("Caretaker Hair Staff and Trim",details,silver,go.transform); Mesh("Caretaker Lantern Flame",light,warm,go.transform);
            Motes("Lantern Embers",go.transform,lantern,dust,new Color(1,0.7f,0.35f,0.4f),new Color(1,0.45f,0.1f,0.3f),6,2,0.035f,0.12f,2);
            return Prefab(go);
        }

        static GameObject Spirit()
        {
            var go=new GameObject("Forest Spirit - Gentle Moon Wisp"); var core=new MoonwingMesh(); var robes=new MoonwingMesh(); var lines=new MoonwingMesh();
            core.Ellipsoid(new Vector3(0,0.62f,0),new Vector3(0.19f,0.23f,0.17f),new Color(0.35f,0.65f,0.8f));
            robes.Lathe(Vector3.zero,new[]{new Vector2(0.02f,-0.81f),new Vector2(0.28f,-0.5f),new Vector2(0.36f,-0.24f),new Vector2(0.24f,0.05f),new Vector2(0.23f,0.35f),new Vector2(0.08f,0.44f)},new Color(0.25f,0.7f,0.9f,0.65f),24,0.8f);
            for(int i=0;i<5;i++)
            {
                float a=i*Mathf.PI*2/5;
                lines.Tube(MoonwingMesh.Curve(new Vector3(Mathf.Cos(a)*0.2f,0.35f,Mathf.Sin(a)*0.17f),new Vector3(Mathf.Cos(a+0.8f)*0.48f,-0.15f,Mathf.Sin(a+0.8f)*0.4f),new Vector3(Mathf.Cos(a+1.5f)*0.13f,-0.86f,Mathf.Sin(a+1.5f)*0.1f)),0.022f,0.003f,Color.Lerp(Cyan,Lavender,i/5f),7);
            }
            foreach(int side in new[]{-1,1})
            {
                core.Ellipsoid(new Vector3(side*0.065f,0.65f,0.155f),new Vector3(0.025f,0.015f,0.009f),Pearl,10,5);
                lines.Tube(MoonwingMesh.Curve(new Vector3(side*0.12f,0.81f,0),new Vector3(side*0.31f,0.96f,0),new Vector3(side*0.25f,1.11f,-0.04f)),0.035f,0.004f,Cyan);
                robes.Tube(MoonwingMesh.Curve(new Vector3(side*0.19f,0.28f,0),new Vector3(side*0.44f,0.06f,0.04f),new Vector3(side*0.5f,-0.17f,0.13f)),0.08f,0.005f,new Color(0.4f,0.65f,1,0.5f));
            }
            Mesh("Spirit Moonlit Face",core,silver,go.transform); Mesh("Spirit Translucent Robes",robes,veil,go.transform); Mesh("Spirit Flowing Wisps",lines,glow,go.transform);
            var motion=go.AddComponent<MoonwingVisualMotion>(); motion.floatHeight=0.065f; motion.floatSpeed=1.2f;
            Motes("Spirit Drifting Lights",go.transform,Vector3.zero,dust,new Color(0.2f,0.8f,1,0.5f),new Color(0.7f,0.5f,1,0.4f),22,6,0.07f,0.52f,3);
            return Prefab(go);
        }

        static void DressAssassin()
        {
            var visual=new GameObject("Assassin - Hooded Shadow"); var cloth=new MoonwingMesh(); var hood=new MoonwingMesh(); var armor=new MoonwingMesh(); var sigils=new MoonwingMesh();
            cloth.Lathe(Vector3.zero,new[]{new Vector2(0.40f,-0.96f),new Vector2(0.36f,-0.69f),new Vector2(0.23f,-0.14f),new Vector2(0.31f,0.23f),new Vector2(0.36f,0.4f),new Vector2(0.14f,0.52f)},new Color(0.045f,0.023f,0.075f),24,0.88f,0.04f);
            hood.Ellipsoid(new Vector3(0,0.68f,-0.025f),new Vector3(0.27f,0.32f,0.23f),new Color(0.065f,0.035f,0.1f));
            // Dark face plate sits forward of the hood; glinting eyes stay readable without a bright face.
            hood.Ellipsoid(new Vector3(0,0.66f,0.175f),new Vector3(0.175f,0.22f,0.06f),new Color(0.008f,0.005f,0.016f));
            armor.Tube(Circle(new Vector3(0,-0.08f,0),0.265f,0.23f),0.035f,0.035f,new Color(0.16f,0.09f,0.21f));
            foreach(int side in new[]{-1,1})
            {
                cloth.Tube(MoonwingMesh.Curve(new Vector3(side*0.3f,0.34f,0),new Vector3(side*0.4f,0.07f,0.06f),new Vector3(side*0.36f,-0.23f,0.16f)),0.125f,0.065f,new Color(0.055f,0.03f,0.085f));
                sigils.Ellipsoid(new Vector3(side*0.07f,0.7f,0.232f),new Vector3(0.027f,0.012f,0.007f),new Color(0.58f,0.09f,0.23f),10,5);
                armor.Ellipsoid(new Vector3(side*0.31f,0.35f,0),new Vector3(0.18f,0.09f,0.17f),new Color(0.13f,0.075f,0.2f));
            }
            sigils.Diamond(new Vector3(0,0.22f,0.275f),new Vector3(0.035f,0.10f,0.012f),new Color(0.46f,0.08f,0.28f));
            Mesh("Assassin Deep Violet Cloak",cloth,shadow,visual.transform); Mesh("Assassin Cowl and Face",hood,shadow,visual.transform); Mesh("Assassin Shoulder Armor",armor,silk,visual.transform);
            var sigil=Mesh("Assassin Restrained Shadow Sigil",sigils,red,visual.transform);
            Motes("Shadow Veil",visual.transform,new Vector3(0,-0.1f,0),smoke,new Color(0.018f,0.008f,0.045f,0.28f),new Color(0.08f,0.025f,0.12f,0.20f),16,5,0.46f,0.42f,2.6f);
            var accents=Motes("Violet Shadow Embers",visual.transform,Vector3.zero,dust,new Color(0.4f,0.04f,0.12f,0.3f),new Color(0.3f,0.08f,0.5f,0.25f),14,3,0.04f,0.45f,2);
            var reaction=visual.AddComponent<MoonwingShadowReaction>(); reaction.accents=accents; reaction.sigil=sigil;
            var prefab=Prefab(visual);
            string path="Assets/Prefabs/Assassin_1.prefab"; var root=PrefabUtility.LoadPrefabContents(path);
            try { root.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly; Place(prefab,root.transform,Vector3.zero); PrefabUtility.SaveAsPrefabAsset(root,path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
