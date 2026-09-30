using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        const string Polish="Assets/Visuals/SubmissionPolish";
        public static void PolishSubmissionBatch()
        {
            EditorSceneManager.OpenScene(ScenePath);
            if (GameObject.Find("Moonwing - Submission Atmosphere")) throw new InvalidOperationException("Submission polish already applied.");
            if (GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>().totalAssassins!=6) throw new InvalidOperationException("Expected the user's six-assassin scene.");
            var state=UnityEngine.Random.state;
            try
            {
                UnityEngine.Random.InitState(77133);
                Directory.CreateDirectory(Polish+"/Resources"); Directory.CreateDirectory(Polish+"/UI"); Directory.CreateDirectory(Polish+"/Materials");
                AssetDatabase.Refresh();
                font=BakeSerif(); PolishTypography(); RoundedDialogue(); StoryMist(); VictoryConfirmation(); ExtendSubmissionForest();
                // The old build entry referenced a deleted template scene. Keep config objects intact.
                var scenes=EditorBuildSettings.scenes.ToList();
                int stale=scenes.FindIndex(x=>x.path=="Assets/Scenes/SampleScene.unity" && !File.Exists(x.path));
                if(stale>=0) scenes[stale]=new EditorBuildSettingsScene(ScenePath,true);
                else if(!scenes.Any(x=>x.path==ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath,true));
                EditorBuildSettings.scenes=scenes.ToArray();
                AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                ValidateSubmission(); SubmissionPreviews();
                Debug.Log("MOONWING_SUBMISSION_POLISH_BUILT");
            }
            finally { UnityEngine.Random.state=state; }
        }

        static TMP_FontAsset BakeSerif()
        {
            var source=AssetDatabase.LoadAssetAtPath<Font>(Polish+"/Fonts/CormorantGaramond-Regular.ttf");
            if(!source) throw new InvalidOperationException("Bundled serif font is missing.");
            var asset=TMP_FontAsset.CreateFontAsset(source,80,9,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,false);
            asset.name="Cormorant Garamond Moonwing";
            AssetDatabase.CreateAsset(asset,Polish+"/Fonts/Moonwing Serif.asset");
            string characters=new string(Enumerable.Range(32,95).Select(x=>(char)x).ToArray())+"—–’‘“”•…";
            if(!asset.TryAddCharacters(characters,out string missing)) throw new InvalidOperationException("Missing required serif glyphs: "+missing);
            foreach(var atlas in asset.atlasTextures) { atlas.name="Moonwing Serif Atlas"; AssetDatabase.AddObjectToAsset(atlas,asset); }
            asset.material.name="Moonwing Serif SDF"; AssetDatabase.AddObjectToAsset(asset.material,asset);
            asset.atlasPopulationMode=AtlasPopulationMode.Static;
            asset.material.SetFloat("_FaceDilate",0.035f);
            EditorUtility.SetDirty(asset); EditorUtility.SetDirty(asset.material); return asset;
        }
        static void PolishTypography()
        {
            var canvas=GameObject.Find("Canvas");
            foreach(var text in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if(text.name=="Controls") continue; // Existing clean small-print font is already bundled.
                text.font=font; text.fontSharedMaterial=font.material;
                text.color=new Color(0.87f,0.86f,0.91f,text.color.a);
                text.characterSpacing=Mathf.Min(text.characterSpacing,2.5f);
                if(text.name=="Caretaker Words") { text.fontSize=32; text.fontSizeMin=28; text.fontSizeMax=32; text.lineSpacing=7; text.characterSpacing=0; }
                if(text.name=="Caretaker Speaker") { text.fontSize=19; text.characterSpacing=3; text.color=new Color(0.77f,0.66f,0.51f); }
                if(text.name=="Caretaker Signature") { text.fontSize=22; text.characterSpacing=0.5f; }
                if(text.name=="Moonfairy Title") { text.fontSize=76; text.characterSpacing=8; text.color=new Color(0.94f,0.91f,0.86f); }
                if(text.name=="Moonwing Subtitle") { text.fontSize=32; text.fontStyle=FontStyles.Italic; text.characterSpacing=1.5f; }
                if(text.name=="Play Label") { text.fontSize=27; text.characterSpacing=4; }
                if(text.name=="Skip Label") { text.fontSize=18; text.characterSpacing=2; }
                if(text.name=="Moonlight Label") { text.fontSize=18; text.characterSpacing=2; }
                if(text.name=="Ending Verse") { text.fontSize=28; text.fontStyle=FontStyles.Italic; text.characterSpacing=0.4f; }
                if(text.text.StartsWith("THE FOREST FALLS") || text.text.StartsWith("YOUR MOONLIGHT"))
                { text.fontSize=39; text.fontSizeMax=39; text.fontSizeMin=30; text.characterSpacing=2.4f; }
            }
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            var controls=opening.title.transform.Find("Controls").GetComponent<TextMeshProUGUI>();
            controls.text="WASD  Move / Strafe    •    SHIFT  Sprint    •    MOUSE  Look / Aim\nCLICK / SPACE  Shoot    •    ESC  Free cursor    •    CLICK  Resume look";
            controls.fontSize=14; controls.lineSpacing=9; controls.rectTransform.sizeDelta=new Vector2(1050,72);
            // Projected marker indicates the horizontal arrow path, including when looking up/down.
            var marker=Image("Moonlight Aim Point",canvas.transform,AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/UI/Moonlight Star.png"),new Color(0.8f,0.9f,1,0.7f));
            Rect(marker.rectTransform,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(12,12)); marker.gameObject.SetActive(false);
            Camera.main.GetComponent<CameraFollow>().aimMarker=marker.rectTransform;
        }
        static Sprite RoundedSprite(string name,bool border)
        {
            const int width=96,height=64; const float radius=16;
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
            for(int y=0;y<height;y++) for(int x=0;x<width;x++)
            {
                Vector2 q=new Vector2(Mathf.Abs(x-(width-1)*0.5f)-(width*0.5f-radius-1),Mathf.Abs(y-(height-1)*0.5f)-(height*0.5f-radius-1));
                float distance=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-radius;
                float alpha=border?Mathf.Clamp01(1-Mathf.Abs(distance+1.0f)):Mathf.Clamp01(-distance*0.5f);
                texture.SetPixel(x,y,new Color(1,1,1,alpha));
            }
            texture.Apply(); string path=Polish+"/UI/"+name+".png"; File.WriteAllBytes(path,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path); var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteBorder=new Vector4(18,18,18,18); importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static void RoundedDialogue()
        {
            var style=ScriptableObject.CreateInstance<MoonwingDialogueStyle>(); style.font=font;
            style.panel=RoundedSprite("Quiet Words Glass",false); style.border=RoundedSprite("Quiet Words Pearl Edge",true);
            AssetDatabase.CreateAsset(style,Polish+"/Resources/MoonwingDialogueStyle.asset");
        }
        static void StoryMist()
        {
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            var material=new Material(Shader.Find("Moonwing/Story Mist")); AssetDatabase.CreateAsset(material,Polish+"/Materials/Story Mist.mat");
            var image=Image("Caretaker Story Ground Mist",opening.transform,null,Color.white);
            Stretch(image.rectTransform,Vector2.zero,Vector2.zero); image.material=material; image.transform.SetAsFirstSibling();
            var mist=image.gameObject.AddComponent<MoonwingIntroMist>(); mist.mist=image; opening.introMist=mist;
        }
        static void VictoryConfirmation()
        {
            var victory=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>().victoryPanel;
            var originalChildren=Enumerable.Range(0,victory.transform.childCount).Select(i=>victory.transform.GetChild(i)).ToArray();
            var poem=UIGroup("Final Forest Poem",victory.transform); var poemGroup=poem.gameObject.AddComponent<CanvasGroup>();
            foreach(var child in originalChildren) child.SetParent(poem,false);
            var confirmation=UIGroup("Assassins Defeated Confirmation",victory.transform); var group=confirmation.gameObject.AddComponent<CanvasGroup>();
            var title=Text("Clear Victory Confirmation",confirmation,"YOU HAVE DEFEATED THE ASSASSINS.",38,new Color(0.9f,0.91f,0.97f));
            title.characterSpacing=2.5f; title.enableAutoSizing=true; title.fontSizeMin=28; title.fontSizeMax=38;
            Rect(title.rectTransform,new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(1050,120));
            var sequence=victory.AddComponent<MoonwingVictorySequence>(); sequence.confirmation=group; sequence.poem=poemGroup; sequence.backdrop=victory.GetComponent<Image>();
            group.alpha=poemGroup.alpha=0; victory.SetActive(false);
            GameObject.Find("Player").GetComponent<FairyMagic>().capturedPanel.SetActive(false);
        }

        public static void ValidateSubmission()
        {
            var player=GameObject.Find("Player"); var spawner=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>();
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            if(spawner.totalAssassins!=6 || spawner.player!=player.transform || Camera.main.GetComponent<CameraFollow>().target!=player.transform) throw new InvalidOperationException("Gameplay binding/count changed.");
            if(spawner.assassinPrefab.GetComponent<EnemyHealth>().health!=3 || player.GetComponent<PlayerShoot>().arrowPrefab.GetComponent<ArrowDamage>().damage!=1) throw new InvalidOperationException("Damage/health changed.");
            if(Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=1 || Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=1) throw new InvalidOperationException("Duplicate camera/EventSystem.");
            if(!opening.introMist || !opening.skipButton || !opening.playButton || !spawner.victoryPanel.GetComponent<MoonwingVictorySequence>()) throw new InvalidOperationException("Presentation binding missing.");
            foreach(var text in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include,FindObjectsSortMode.None)) if(!text.font || !text.fontSharedMaterial) throw new InvalidOperationException("Missing TMP reference: "+text.name);
            foreach(var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)) if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0) throw new InvalidOperationException("Missing script on "+transform.name);
            foreach(string path in AssetDatabase.FindAssets("t:Material",new[]{Root,Clearing,Polish}).Select(AssetDatabase.GUIDToAssetPath))
            { var material=AssetDatabase.LoadAssetAtPath<Material>(path); if(!material.shader || ShaderUtil.ShaderHasError(material.shader)) throw new InvalidOperationException("Shader failed: "+path); }
            if(GameObject.Find("Moonwing - Submission Atmosphere").GetComponentsInChildren<Collider>().Length!=0) throw new InvalidOperationException("Decorative forest gained colliders.");
            Directory.CreateDirectory("Logs/SubmissionReview");
            File.WriteAllText("Logs/SubmissionReview/Validation.txt","Six assassins; health 3; arrow damage 1.\nSingle camera and EventSystem.\nAll scene scripts, fonts, materials and critical references present.\nNo decorative colliders added.\nMesh triangles before culling: "+Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Sum(f=>f.sharedMesh?f.sharedMesh.triangles.Length/3:0)+"\n");
        }
        public static void SubmissionPreviews()
        {
            var victory=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>().victoryPanel;
            var sequence=victory.GetComponent<MoonwingVictorySequence>();
            sequence.poem.alpha=1; sequence.confirmation.alpha=0;
            CapturePreviews(); // Existing preview renderer restores all temporary scene state.
            var camera=Camera.main; var position=camera.transform.position; var rotation=camera.transform.rotation;
            var canvas=GameObject.Find("Canvas").GetComponent<Canvas>(); var mode=canvas.renderMode; var worldCamera=canvas.worldCamera;
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            bool active=opening.gameObject.activeSelf;
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1;
                opening.narration.alpha=0; opening.title.alpha=1; opening.introMist.gameObject.SetActive(false); opening.skipButton.gameObject.SetActive(false);
                Render("03 - Moonfairy Title"); opening.introMist.gameObject.SetActive(true); opening.skipButton.gameObject.SetActive(true);
                opening.narration.alpha=1; opening.title.alpha=0;
                opening.gameObject.SetActive(false); victory.SetActive(true); sequence.poem.alpha=0; sequence.confirmation.alpha=1;
                Render("19 - Victory Confirmation"); victory.SetActive(false); canvas.enabled=false;
                Vector3 target=GameObject.Find("Player").transform.position;
                for(int i=0;i<4;i++)
                {
                    Vector3 offset=Quaternion.Euler(0,i*90,0)*new Vector3(0,6,-10);
                    camera.transform.position=target+offset; camera.transform.LookAt(target+Vector3.up*0.45f);
                    Render("20 - Forest View "+(i*90));
                }
                camera.transform.position=new Vector3(36,9,0); camera.transform.LookAt(new Vector3(62,1,0)); Render("21 - Mist Boundary");
            }
            finally
            {
                camera.transform.SetPositionAndRotation(position,rotation); canvas.enabled=true; canvas.renderMode=mode; canvas.worldCamera=worldCamera;
                victory.SetActive(false); sequence.poem.alpha=sequence.confirmation.alpha=0; opening.gameObject.SetActive(active);
            }
            foreach(string source in Directory.GetFiles("Logs/FirstPassReview","*.png")) File.Copy(source,"Logs/SubmissionReview/"+Path.GetFileName(source),true);
        }
    }
}
