using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        static void RefinedInterface()
        {
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            panelSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/UI/Midnight Glass.png");
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            opening.narrationSeconds=26;
            opening.GetComponent<Image>().color=new Color(0.017f,0.021f,0.045f,0.57f);
            var narration=opening.narration.transform;
            var panel=Image("Caretaker Story Panel",narration,panelSprite,new Color(0.8f,0.78f,0.93f,0.88f));
            Rect(panel.rectTransform,new Vector2(0.5f,0.5f),new Vector2(70,-95),new Vector2(930,265));
            panel.transform.SetAsFirstSibling();
            Frame(panel.rectTransform,Vector2.zero,Vector2.one,new Color(0.60f,0.52f,0.43f,0.48f));
            var narrative=narration.Find("Caretaker Words").GetComponent<TextMeshProUGUI>();
            narrative.transform.SetParent(panel.transform,false);
            Rect(narrative.rectTransform,new Vector2(0.5f,0.5f),new Vector2(82,4),new Vector2(665,140));
            narrative.text="Welcome to Moonwing Forest."; narrative.fontSize=27; narrative.lineSpacing=12;
            narrative.enableAutoSizing=true; narrative.fontSizeMin=23; narrative.fontSizeMax=27;
            narrative.alignment=TextAlignmentOptions.Left; narrative.color=new Color(0.88f,0.86f,0.82f); narrative.characterSpacing=0.6f;
            opening.narrationText=narrative; opening.words=narrative.gameObject.AddComponent<CanvasGroup>();
            var name=Text("Caretaker Speaker",panel.transform,"THE CARETAKER",14,new Color(0.73f,0.59f,0.43f)); name.characterSpacing=4;
            Rect(name.rectTransform,new Vector2(0.5f,0.5f),new Vector2(82,94),new Vector2(665,30)); name.alignment=TextAlignmentOptions.Left;
            var signature=Text("Caretaker Signature",panel.transform,"— The Caretaker",17,new Color(0.69f,0.67f,0.72f)); signature.fontStyle=FontStyles.Italic;
            Rect(signature.rectTransform,new Vector2(0.5f,0.5f),new Vector2(82,-92),new Vector2(665,30)); signature.alignment=TextAlignmentOptions.Left;
            var portrait=Image("Caretaker Portrait",panel.transform,CaretakerPortrait(),Color.white);
            Rect(portrait.rectTransform,new Vector2(0,0.5f),new Vector2(20,58),new Vector2(245,365)); portrait.preserveAspect=true;
            var skip=Image("SKIP",opening.transform,null,new Color(0.03f,0.03f,0.07f,0.8f));
            Rect(skip.rectTransform,new Vector2(1,1),new Vector2(-94,-49),new Vector2(112,36)); skip.raycastTarget=true;
            opening.skipButton=skip.gameObject.AddComponent<Button>(); opening.skipButton.targetGraphic=skip;
            UnityEventTools.AddPersistentListener(opening.skipButton.onClick,opening.Skip);
            var skipLabel=Text("Skip Label",skip.transform,"SKIP",13,new Color(0.77f,0.76f,0.82f)); skipLabel.characterSpacing=3; Stretch(skipLabel.rectTransform,Vector2.zero,Vector2.zero);
            Frame(skip.rectTransform,Vector2.zero,Vector2.one,new Color(0.55f,0.53f,0.66f,0.38f));
            var title=opening.title.transform;
            var heading=title.Find("Moonfairy Title").GetComponent<TextMeshProUGUI>(); heading.fontSize=54; heading.characterSpacing=9; heading.color=Ivory;
            title.Find("Title Moon").GetComponent<RectTransform>().sizeDelta=new Vector2(38,38);
            var sub=title.Find("Moonwing Subtitle").GetComponent<TextMeshProUGUI>(); sub.fontSize=23; sub.characterSpacing=2;
            var controls=Text("Controls",title,"WASD  Move     •     LEFT SHIFT  Sprint     •     MOUSE  Aim     •     CLICK / SPACE  Shoot",14,new Color(0.66f,0.67f,0.74f));
            Rect(controls.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,-220),new Vector2(1080,40));
            var playText=opening.playButton.GetComponentInChildren<TextMeshProUGUI>(); playText.text="PLAY"; playText.characterSpacing=5; playText.fontSize=19;
            opening.playButton.GetComponent<RectTransform>().sizeDelta=new Vector2(220,52);
            var slider=GameObject.Find("Player").GetComponent<FairyMagic>().moonlightBar;
            slider.transform.Find("Moon Goddess Crescent").GetComponent<RectTransform>().sizeDelta=new Vector2(29,29);
            var meterLabel=slider.transform.Find("Moonlight Label").GetComponent<TextMeshProUGUI>(); meterLabel.text="MOONLIGHT"; meterLabel.characterSpacing=4; meterLabel.fontSize=13;
            foreach(var ending in opening.endingPanels)
            {
                var text=ending.GetComponentInChildren<TextMeshProUGUI>(true); text.fontSize=30; text.fontSizeMax=30; text.fontSizeMin=22; text.characterSpacing=2;
                ending.transform.Find("Ending Crescent").GetComponent<RectTransform>().sizeDelta=new Vector2(38,38);
                var verse=ending.transform.Find("Ending Verse").GetComponent<TextMeshProUGUI>(); verse.fontSize=20; verse.characterSpacing=0.6f;
                foreach(var constellation in ending.GetComponentsInChildren<MoonwingUIConstellation>(true))
                    foreach(var star in constellation.stars) star.rectTransform.sizeDelta*=0.55f;
            }
            foreach(var constellation in opening.GetComponentsInChildren<MoonwingUIConstellation>(true))
                foreach(var star in constellation.stars) star.rectTransform.sizeDelta*=0.55f;
        }

        static Sprite CaretakerPortrait()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Caretaker - Lantern Keeper.prefab");
            var actor=(GameObject)PrefabUtility.InstantiatePrefab(prefab); actor.transform.position=new Vector3(1000,0,0);
            var cameraObject=new GameObject("Portrait Capture Only"); var camera=cameraObject.AddComponent<Camera>();
            camera.CopyFrom(Camera.main); camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=Color.clear;
            camera.transform.position=actor.transform.position+new Vector3(0.15f,0.70f,2.4f); camera.transform.LookAt(actor.transform.position+new Vector3(0.05f,0.42f,0)); camera.fieldOfView=31;
            var rt=new RenderTexture(384,512,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB); rt.Create();
            var previous=RenderTexture.active; bool fog=RenderSettings.fog;
            string path=Root+"/UI/Caretaker Portrait.png";
            try
            {
                RenderSettings.fog=false; camera.targetTexture=rt;
                RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest { destination=rt });
                RenderTexture.active=rt; var texture=new Texture2D(384,512,TextureFormat.RGBA32,false);
                texture.ReadPixels(new Rect(0,0,384,512),0,0); texture.Apply(); File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            }
            finally { RenderSettings.fog=fog; RenderTexture.active=previous; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(cameraObject); Object.DestroyImmediate(actor); }
            AssetDatabase.ImportAsset(path); var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void RefinedGarden()
        {
            var pass=GameObject.Find(PassName).transform;
            var home=pass.Find("Moonfairy Residence - Lantern Garden Foundation");
            // Retain the established footprint and roof; refine the palette and add signs of habitation.
            foreach(string name in new[]{"Residence Timber Structure","Residence Moonstone Steps"})
            {
                var filter=home.Find(name).GetComponent<MeshFilter>(); var mesh=Object.Instantiate(filter.sharedMesh);
                var colors=mesh.colors;
                for(int i=0;i<colors.Length;i++) colors[i]=Color.Lerp(colors[i],new Color(0.62f,0.60f,0.54f),0.72f);
                mesh.colors=colors; mesh.name="Refined "+name; AssetDatabase.CreateAsset(mesh,Root+"/Meshes/"+mesh.name+".asset"); filter.sharedMesh=mesh;
            }
            var garden=Group("Second Pass - Garden Details",home);
            var stone=new MoonwingMesh(); var wood=new MoonwingMesh(); var vines=new MoonwingMesh(); var blossoms=new MoonwingMesh(); var water=new MoonwingMesh();
            for(int i=0;i<10;i++)
            {
                float z=4.3f+i*0.60f;
                stone.Ellipsoid(new Vector3(Mathf.Sin(i*0.36f)*0.8f,0.04f,z),new Vector3(0.48f,0.06f,0.31f),new Color(0.30f,0.33f,0.39f),12,5);
            }
            wood.Box(new Vector3(0,0.65f,-0.50f),new Vector3(1.5f,0.09f,0.75f),new Color(0.24f,0.17f,0.14f));
            foreach(float x in new[]{-0.62f,0.62f}) wood.Box(new Vector3(x,0.48f,-0.50f),new Vector3(0.08f,0.26f,0.55f),new Color(0.22f,0.16f,0.14f));
            foreach(float x in new[]{-1.15f,1.15f}) stone.Ellipsoid(new Vector3(x,0.40f,-0.50f),new Vector3(0.32f,0.07f,0.32f),new Color(0.32f,0.28f,0.32f));
            stone.Lathe(new Vector3(0,0.70f,-0.5f),new[]{new Vector2(0.10f,0),new Vector2(0.14f,0.1f),new Vector2(0.085f,0.20f),new Vector2(0.065f,0.26f)},Ivory,18);
            foreach(float x in new[]{-0.32f,0.32f}) stone.Lathe(new Vector3(x,0.70f,-0.4f),new[]{new Vector2(0.04f,0),new Vector2(0.055f,0.06f)},Ivory,12);
            foreach(float side in new[]{-1f,1f})
            {
                wood.Box(new Vector3(side*2.8f,1.02f,-0.5f),new Vector3(0.07f,0.07f,2.3f),new Color(0.39f,0.34f,0.29f));
                for(int i=0;i<5;i++) wood.Line(new Vector3(side*2.8f,0.36f,-1.5f+i*0.5f),new Vector3(side*2.8f,1.02f,-1.5f+i*0.5f),0.025f,new Color(0.40f,0.37f,0.33f));
                var curve=new Vector3[35];
                for(int i=0;i<curve.Length;i++)
                {
                    float t=i/(float)(curve.Length-1); curve[i]=new Vector3(side*2.8f+0.16f*Mathf.Cos(t*19),0.4f+t*3,2.1f+0.16f*Mathf.Sin(t*19));
                    vines.Ellipsoid(curve[i]+Vector3.right*0.08f,new Vector3(0.13f,0.045f,0.065f),new Color(0.11f,0.22f,0.19f),8,5);
                    if(i%4==0) for(int j=0;j<5;j++) { float a=j*Mathf.PI*2/5; blossoms.Ellipsoid(curve[i]+new Vector3(Mathf.Cos(a)*0.052f,Mathf.Sin(a)*0.052f,0.08f),new Vector3(0.04f,0.04f,0.023f),new Color(0.42f,0.35f,0.53f),8,5); }
                }
                vines.Tube(curve,0.014f,0.006f,new Color(0.12f,0.15f,0.12f),6);
            }
            // Shallow garden basin uses a static translucent surface, no reflection camera or physics.
            stone.Lathe(new Vector3(4.6f,0,2.8f),new[]{new Vector2(0.9f,0.02f),new Vector2(0.94f,0.14f),new Vector2(0.78f,0.20f),new Vector2(0.74f,0.06f)},new Color(0.25f,0.29f,0.32f),32,0.7f);
            water.Ellipsoid(new Vector3(4.6f,0.135f,2.8f),new Vector3(0.77f,0.012f,0.52f),new Color(0.13f,0.32f,0.43f,0.65f),32,5);
            Mesh("Refined Garden Stone and Tea Set",stone,pearlCloth,garden); Mesh("Refined Residence Furniture",wood,silk,garden);
            Mesh("Refined Climbing Vines",vines,silk,garden); Mesh("Refined Vine Blossoms",blossoms,silver,garden); Mesh("Refined Garden Water",water,veil,garden);
            var flowers=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/Starlace Moonflowers.prefab");
            for(int i=0;i<6;i++) Place(flowers,garden,new Vector3(i%2==0?-1.8f:1.8f,0,5+i*0.65f),i*57,0.5f);
            // Sparse distant silhouettes fill peripheral gaps, retaining the current forest and open paths.
            var distant=Group("Second Pass - Distant Forest Layer",pass);
            string[] trees={"Midnight Alder","Lavender Willow","Silverbranch Arch"};
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI*2/12;
                var tree=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/"+trees[i%3]+".prefab");
                Place(tree,distant,new Vector3(Mathf.Cos(a)*33,-0.3f,Mathf.Sin(a)*33),i*47,0.74f+(i%3)*0.16f);
            }
            // Keep dark clusters and isolated highlights; avoid a uniform brightness increase.
            var bright=LoadMat("Moonwell Mushroom Caps"); bright.SetFloat("_Emission",5.4f); EditorUtility.SetDirty(bright);
            var quiet=LoadMat("Quiet Lavender Mushroom Caps"); quiet.SetFloat("_Emission",1.35f); EditorUtility.SetDirty(quiet);
        }
    }
}
