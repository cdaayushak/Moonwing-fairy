using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        static Sprite barSprite,haloSprite,moonSprite,starSprite,panelSprite;
        static TMP_FontAsset font;
        static void StyleUI()
        {
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            barSprite=SpriteAsset("Moonlight Gradient",128,32,(x,y)=>{
                float edge=1-Smooth(0.7f,1,Mathf.Abs(y*2-1));
                Color c=Color.Lerp(new Color(0.40f,0.85f,1),new Color(0.74f,0.52f,1),x); c=Color.Lerp(c,Color.white,Mathf.Pow(1-Mathf.Abs(y*2-1),6)*0.35f); c.a=edge; return c;
            });
            haloSprite=SpriteAsset("Moonlight Soft Halo",128,32,(x,y)=>new Color(0.6f,0.72f,1,Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(y*2-1)),2)*Smooth(0,0.06f,x)*Smooth(0,0.06f,1-x)));
            moonSprite=SpriteAsset("Silver Crescent",128,128,(x,y)=>{
                Vector2 p=new Vector2(x*2-1,y*2-1); float outer=1-Smooth(0.77f,0.82f,p.magnitude); float inner=Smooth(0.63f,0.68f,(p-new Vector2(0.34f,0.18f)).magnitude); return new Color(0.78f,0.88f,1,outer*inner);
            });
            starSprite=SpriteAsset("Moonlight Star",64,64,(x,y)=>new Color(0.8f,0.9f,1,1-Smooth(0.48f,0.57f,Mathf.Sqrt(Mathf.Abs(x-0.5f))+Mathf.Sqrt(Mathf.Abs(y-0.5f)))));
            panelSprite=SpriteAsset("Midnight Glass",64,64,(x,y)=>new Color(0.025f,0.025f,0.075f,0.95f));
            var canvas=GameObject.Find("Canvas").GetComponent<Canvas>();
            var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1440,900); scaler.matchWidthOrHeight=0.5f;
            var player=GameObject.Find("Player").GetComponent<FairyMagic>(); var slider=player.moonlightBar;
            RectTransform meter=slider.GetComponent<RectTransform>(); meter.anchorMin=meter.anchorMax=new Vector2(0,1); meter.pivot=new Vector2(0,0.5f); meter.anchoredPosition=new Vector2(83,-61); meter.sizeDelta=new Vector2(310,20);
            slider.interactable=false; slider.navigation=new Navigation { mode=Navigation.Mode.None };
            var background=slider.transform.Find("Background").GetComponent<Image>(); background.sprite=panelSprite; background.color=Color.white; Stretch(background.rectTransform,Vector2.zero,Vector2.zero);
            RectTransform fillArea=slider.transform.Find("Fill Area").GetComponent<RectTransform>(); Stretch(fillArea,new Vector2(4,4),new Vector2(-4,-4));
            var fill=slider.fillRect.GetComponent<Image>(); fill.sprite=barSprite; fill.type=UnityEngine.UI.Image.Type.Simple; fill.color=Color.white; slider.fillRect.sizeDelta=Vector2.zero;
            if(slider.handleRect) slider.handleRect.GetComponent<Image>().enabled=false;
            Frame(meter,new Vector2(0,0),new Vector2(1,1),new Color(0.65f,0.64f,0.86f,0.85f));
            var halo=Image("Draining Moonlight Halo",slider.fillRect,haloSprite,new Color(0.65f,0.8f,1,0.25f)); Stretch(halo.rectTransform,new Vector2(-4,-9),new Vector2(4,9));
            var moon=Image("Moon Goddess Crescent",meter,moonSprite,Pearl); Rect(moon.rectTransform,new Vector2(0,0.5f),new Vector2(-35,0),new Vector2(43,43));
            var label=Text("Moonlight Label",meter,"M O O N L I G H T",15,Pearl); Rect(label.rectTransform,new Vector2(0,1),new Vector2(0,20),new Vector2(310,25)); label.alignment=TextAlignmentOptions.Left; label.rectTransform.pivot=new Vector2(0,0.5f);
            var pulse=meter.gameObject.AddComponent<MoonwingMeterGlow>(); pulse.meter=slider; pulse.fill=fill; pulse.halo=halo;
            var meterGroup=meter.gameObject.AddComponent<CanvasGroup>();
            var spawner=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>();
            Ending(spawner.victoryPanel,true); Ending(player.capturedPanel,false);
            Opening(canvas.transform,meterGroup);
        }

        static void Ending(GameObject panel,bool victory)
        {
            panel.GetComponent<Image>().color=victory?new Color(0.025f,0.035f,0.095f,0.89f):new Color(0.025f,0.018f,0.06f,0.93f);
            var original=panel.GetComponentInChildren<TextMeshProUGUI>(true);
            original.font=font; original.text=victory?"THE FOREST FALLS SILENT ONCE MORE.":"YOUR MOONLIGHT HAS FADED.";
            original.fontSize=34; original.enableAutoSizing=true; original.fontSizeMin=24; original.fontSizeMax=34;
            original.alignment=TextAlignmentOptions.Center; original.characterSpacing=3; original.color=victory?Pearl:new Color(0.64f,0.61f,0.77f);
            Rect(original.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,25),new Vector2(1000,100));
            var caption=Text("Ending Verse",panel.transform,victory?"Your moonlight endures. The forest can dream again.":"The forest holds its breath. Even moonlight must rest.",22,new Color(0.65f,0.64f,0.81f));
            Rect(caption.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,-70),new Vector2(880,50));
            var moon=Image("Ending Crescent",panel.transform,moonSprite,victory?Pearl:new Color(0.48f,0.44f,0.64f)); Rect(moon.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,145),new Vector2(70,70));
            Frame(panel.GetComponent<RectTransform>(),new Vector2(0.12f,0.26f),new Vector2(0.88f,0.78f),new Color(0.5f,0.47f,0.7f,0.5f));
            Constellation(panel.transform,14,victory?Pearl:new Color(0.45f,0.38f,0.63f));
            // Their active state and FairyMagic/AssassinSpawner references are intentionally retained.
        }

        static void Opening(Transform canvas,CanvasGroup meter)
        {
            var go=new GameObject("Moonfairy Opening",typeof(RectTransform),typeof(CanvasGroup),typeof(Image)); go.transform.SetParent(canvas,false);
            Stretch(go.GetComponent<RectTransform>(),Vector2.zero,Vector2.zero);
            go.GetComponent<Image>().color=new Color(0.017f,0.021f,0.055f,0.8f);
            var opening=go.AddComponent<MoonwingOpening>(); opening.overlay=go.GetComponent<CanvasGroup>(); opening.meter=meter;
            opening.endingPanels=new[]{GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>().victoryPanel,GameObject.Find("Player").GetComponent<FairyMagic>().capturedPanel};
            var gate=new List<Behaviour>();
            foreach(string name in new[]{"Player","Caretaker","ForestSpirit","AssassinSpawner"})
                foreach(var component in GameObject.Find(name).GetComponents<MonoBehaviour>())
                    if(component is PlayerMovement || component is PlayerShoot || component is FairyMagic || component is NPCWander || component is NPCConversation || component is NPCInteraction || component is CaretakerInteraction || component is AssassinSpawner) gate.Add(component);
            opening.gameplay=gate.ToArray();
            var narration=UIGroup("Caretaker Narration",go.transform); opening.narration=narration.gameObject.AddComponent<CanvasGroup>();
            var narrative=Text("Caretaker Words",narration,"Welcome to Moonwing Forest.\n\nHere lives the Moonfairy, blessed with magic by the Moon Goddess herself.\n\nShe guards a rare potion born of that magic—one coveted by the wealthy and powerful.\n\nI am her caretaker. And I have learned that when this forest grows quiet... we should be afraid.\n\n<size=20>— The Caretaker</size>",25,new Color(0.78f,0.78f,0.89f));
            Rect(narrative.rectTransform,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(930,500)); narrative.lineSpacing=8;
            var title=UIGroup("Moonfairy Title and Play",go.transform); opening.title=title.gameObject.AddComponent<CanvasGroup>();
            var heading=Text("Moonfairy Title",title,"MOONFAIRY",68,Pearl); heading.characterSpacing=13; Rect(heading.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,80),new Vector2(1000,120));
            var sub=Text("Moonwing Subtitle",title,"of Moonwing Forest",26,new Color(0.66f,0.64f,0.83f)); sub.characterSpacing=4; Rect(sub.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,0),new Vector2(800,60));
            var crescent=Image("Title Moon",title,moonSprite,Pearl); Rect(crescent.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,208),new Vector2(80,80));
            var button=Image("PLAY",title,panelSprite,new Color(0.65f,0.66f,0.93f,1)); Rect(button.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,-135),new Vector2(240,62)); button.raycastTarget=true;
            var play=button.gameObject.AddComponent<Button>(); play.targetGraphic=button; opening.playButton=play;
            ColorBlock colors=play.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(0.70f,0.85f,1); colors.pressedColor=new Color(0.5f,0.62f,0.85f); play.colors=colors;
            Frame(button.rectTransform,Vector2.zero,Vector2.one,new Color(0.65f,0.68f,0.87f,0.9f));
            var playText=Text("Play Label",button.transform,"P L A Y",23,Pearl); Stretch(playText.rectTransform,Vector2.zero,Vector2.zero);
            UnityEventTools.AddPersistentListener(play.onClick,opening.Play);
            Constellation(go.transform,22,new Color(0.6f,0.72f,1));
            // Save the initial appearance for edit-mode previews, too.
            opening.title.alpha=0; opening.narration.alpha=1; meter.alpha=0;
        }

        static void Constellation(Transform parent,int count,Color color)
        {
            var root=UIGroup("Quiet Constellation",parent); var stars=new Graphic[count];
            for(int i=0;i<count;i++)
            {
                var star=Image("Star "+i,root,starSprite,color); float size=R(12,25);
                Rect(star.rectTransform,new Vector2(R(0.06f,0.94f),R(0.1f,0.92f)),Vector2.zero,new Vector2(size,size)); stars[i]=star;
            }
            root.gameObject.AddComponent<MoonwingUIConstellation>().stars=stars;
        }
        static RectTransform UIGroup(string name,Transform parent)
        { var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false); var rt=go.GetComponent<RectTransform>(); Stretch(rt,Vector2.zero,Vector2.zero); return rt; }
        static Image Image(string name,Transform parent,Sprite sprite,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false); var image=go.GetComponent<Image>(); image.sprite=sprite; image.color=color; image.raycastTarget=false; return image; }
        static TextMeshProUGUI Text(string name,Transform parent,string text,float size,Color color)
        { var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(parent,false); var t=go.GetComponent<TextMeshProUGUI>(); t.font=font; t.text=text; t.fontSize=size; t.color=color; t.alignment=TextAlignmentOptions.Center; t.raycastTarget=false; return t; }
        static void Rect(RectTransform rt,Vector2 anchor,Vector2 position,Vector2 size)
        { rt.anchorMin=rt.anchorMax=anchor; rt.pivot=new Vector2(0.5f,0.5f); rt.anchoredPosition=position; rt.sizeDelta=size; }
        static void Stretch(RectTransform rt,Vector2 min,Vector2 max)
        { rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=min; rt.offsetMax=max; }
        static void Frame(RectTransform parent,Vector2 min,Vector2 max,Color color)
        {
            var group=UIGroup("Silver Filigree Frame",parent); group.anchorMin=min; group.anchorMax=max; group.offsetMin=new Vector2(-3,-3); group.offsetMax=new Vector2(3,3);
            for(int i=0;i<4;i++)
            {
                var line=Image("Silver Edge "+i,group,null,color).rectTransform;
                if(i<2) { line.anchorMin=new Vector2(0,i); line.anchorMax=new Vector2(1,i); line.sizeDelta=new Vector2(0,1.4f); }
                else { line.anchorMin=new Vector2(i-2,0); line.anchorMax=new Vector2(i-2,1); line.sizeDelta=new Vector2(1.4f,0); }
                line.anchoredPosition=Vector2.zero;
            }
        }
        static Sprite SpriteAsset(string name,int width,int height,System.Func<float,float,Color> sample)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            for(int y=0;y<height;y++) for(int x=0;x<width;x++) texture.SetPixel(x,y,sample(x/(float)(width-1),y/(float)(height-1)));
            texture.Apply(); string path=Root+"/UI/"+name+".png"; File.WriteAllBytes(path,texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path); var importer=(TextureImporter)AssetImporter.GetAtPath(path); importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single; importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.filterMode=FilterMode.Bilinear; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static float Smooth(float start,float end,float value) { return Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,value)); }
    }
}
