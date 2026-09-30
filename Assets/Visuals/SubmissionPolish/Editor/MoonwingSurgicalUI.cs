using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        // Deliberately independent of the forest/camera/full-pass builders.
        public static void SurgicalPresentationOnly()
        {
            EditorSceneManager.OpenScene(ScenePath);
            Directory.CreateDirectory(Polish+"/Resources");
            Directory.CreateDirectory(Polish+"/Materials");
            AssetDatabase.Refresh();
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Polish+"/Fonts/Moonwing Serif.asset");
            if(!font) font=BakeSerif();
            var fontData=new SerializedObject(font);
            fontData.FindProperty("m_SourceFontFileGUID").stringValue=AssetDatabase.AssetPathToGUID(Polish+"/Fonts/CormorantGaramond-Regular.ttf");
            fontData.ApplyModifiedPropertiesWithoutUndo();
            var material=font.material;
            material.SetFloat("_FaceDilate",0);
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor",new Color(0.025f,0.018f,0.06f,0.8f));
            material.SetFloat("_UnderlayOffsetX",0.3f);
            material.SetFloat("_UnderlayOffsetY",-0.4f);
            material.SetFloat("_UnderlayDilate",0.12f);
            material.SetFloat("_UnderlaySoftness",0.45f);
            EditorUtility.SetDirty(material);
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            if(!opening || !opening.narrationText) throw new InvalidOperationException("Active opening bindings missing");
            var panel=opening.narration.transform.Find("Caretaker Story Panel");
            if(!panel) throw new InvalidOperationException("Expected actual Caretaker panel");
            // Keep the portrait, text and layout. Remove all panel/frame graphics, not just their colors.
            panel.GetComponent<Image>().enabled=false;
            var frame=panel.Find("Silver Filigree Frame");
            if(frame) frame.gameObject.SetActive(false);
            foreach(var text in GameObject.Find("Canvas").GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if(text.name=="Controls") continue;
                text.font=font; text.fontSharedMaterial=material;
                text.fontStyle=FontStyles.Normal;
                text.color=new Color(0.96f,0.94f,0.90f,text.color.a);
                text.characterSpacing=2.5f;
                if(text.name=="Caretaker Words") { text.fontSize=36; text.fontSizeMin=31; text.fontSizeMax=36; text.characterSpacing=0; text.lineSpacing=6; }
                if(text.name=="Caretaker Speaker") { text.fontSize=27; text.fontSizeMin=27; text.fontSizeMax=27; text.characterSpacing=5; }
                if(text.name=="Caretaker Signature") { text.fontSize=26; text.fontStyle=FontStyles.Italic; text.characterSpacing=0.5f; }
                if(text.name=="Moonfairy Title") { text.fontSize=82; text.fontSizeMax=82; text.characterSpacing=10; }
                if(text.name=="Moonwing Subtitle" || text.name=="Ending Verse") { text.fontSize=30; text.fontSizeMax=30; text.fontStyle=FontStyles.Italic; text.characterSpacing=0.5f; }
                if(text.name=="Skip Label") { text.fontSize=20; text.characterSpacing=3; }
                if(text.name=="Play Label") { text.fontSize=28; text.characterSpacing=4; }
                if(text.text.StartsWith("YOUR MOONLIGHT") || text.text.StartsWith("THE FOREST FALLS") || text.text.StartsWith("YOU HAVE DEFEATED"))
                { text.fontSize=43; text.fontSizeMax=43; text.fontSizeMin=32; text.characterSpacing=3; }
            }
            var style=AssetDatabase.LoadAssetAtPath<MoonwingDialogueStyle>(Polish+"/Resources/MoonwingDialogueStyle.asset");
            if(!style) { style=ScriptableObject.CreateInstance<MoonwingDialogueStyle>(); AssetDatabase.CreateAsset(style,Polish+"/Resources/MoonwingDialogueStyle.asset"); }
            style.font=font; style.panel=null; style.border=null; EditorUtility.SetDirty(style);
            if(!opening.introMist)
            {
                var mistMaterial=AssetDatabase.LoadAssetAtPath<Material>(Polish+"/Materials/Story Mist.mat");
                if(!mistMaterial) { mistMaterial=new Material(Shader.Find("Moonwing/Story Mist")); AssetDatabase.CreateAsset(mistMaterial,Polish+"/Materials/Story Mist.mat"); }
                var graphic=Image("Caretaker Story Ground Mist",opening.transform,null,Color.white);
                Stretch(graphic.rectTransform,Vector2.zero,Vector2.zero);
                graphic.material=mistMaterial; graphic.transform.SetAsFirstSibling();
                opening.introMist=graphic.gameObject.AddComponent<MoonwingIntroMist>();
                opening.introMist.mist=graphic;
            }
            opening.introMist.mist.material.shader=Shader.Find("Moonwing/Story Mist");
            EditorUtility.SetDirty(opening.introMist.mist.material);
            if(ShaderUtil.ShaderHasError(opening.introMist.mist.material.shader)) throw new Exception("Intro mist shader error");
            EditorUtility.SetDirty(opening);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("MOONWING_SURGICAL_UI_SAVED");
        }
    }
}
