using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        public static void FinalFinishingOnly()
        {
            EditorSceneManager.OpenScene(ScenePath);
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Polish+"/Fonts/Moonwing Serif.asset");
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            var panel=opening.narration.transform.Find("Caretaker Story Panel");
            var heading=panel.Find("Caretaker Speaker").GetComponent<TextMeshProUGUI>();
            heading.text="MOONWING FOREST";
            heading.fontStyle=FontStyles.Italic;
            heading.fontSize=heading.fontSizeMin=heading.fontSizeMax=40;
            heading.characterSpacing=2;
            // The original heading RectTransform height was designed for small capitals.
            heading.rectTransform.sizeDelta=new Vector2(heading.rectTransform.sizeDelta.x,64);
            if(!panel.Find("Narration Moon Haze"))
            {
                var haze=Image("Narration Moon Haze",panel,null,new Color(1,1,1,0.8f));
                Stretch(haze.rectTransform,new Vector2(-45,-38),new Vector2(45,38));
                haze.transform.SetAsFirstSibling();
                var effect=haze.gameObject.AddComponent<MoonwingNarrationHaze>(); effect.haze=haze;
                effect.stars=new Image[4];
                var star=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/UI/Moonlight Star.png");
                for(int i=0;i<4;i++)
                {
                    var mote=Image("Narration Pearl Mote "+i,haze.transform,star,Color.white);
                    Rect(mote.rectTransform,new Vector2(i%2==0?0.12f:0.88f,i<2?0.18f:0.82f),Vector2.zero,Vector2.one*(i%2==0?9:12));
                    effect.stars[i]=mote;
                }
            }
            var spawner=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>();
            if(spawner.totalAssassins!=6) throw new System.InvalidOperationException("Expected six assassins");
            var victory=spawner.victoryPanel;
            // Restore the already-authored confirmation/poem presentation binding only.
            // The spawner, damage, win trigger and six-assassin count are untouched.
            if(!victory.GetComponent<MoonwingVictorySequence>()) VictoryConfirmation();
            var sequence=victory.GetComponent<MoonwingVictorySequence>();
            AddRestart(GameObject.Find("Player").GetComponent<FairyMagic>().capturedPanel.transform,"TRY AGAIN",null);
            AddRestart(sequence.poem.transform,"PLAY AGAIN",sequence);
            var scenes=EditorBuildSettings.scenes.ToList();
            int stale=scenes.FindIndex(s=>s.path=="Assets/Scenes/SampleScene.unity"&&!File.Exists(s.path));
            if(stale>=0) scenes[stale]=new EditorBuildSettingsScene(ScenePath,true);
            else if(!scenes.Any(s=>s.path==ScenePath&&s.enabled)) scenes.Add(new EditorBuildSettingsScene(ScenePath,true));
            EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("MOONWING_FINAL_FINISHING_SAVED");
        }

        static void AddRestart(Transform parent,string title,MoonwingVictorySequence sequence)
        {
            if(parent.Find(title)) return;
            var image=Image(title,parent,null,new Color(0.035f,0.035f,0.08f,0.64f));
            Rect(image.rectTransform,new Vector2(0.5f,0.5f),new Vector2(0,-157),new Vector2(230,50));
            image.raycastTarget=true;
            var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            var colors=button.colors; colors.normalColor=Color.white;
            colors.highlightedColor=new Color(0.78f,0.81f,1); colors.pressedColor=new Color(0.57f,0.58f,0.80f); colors.fadeDuration=0.18f; button.colors=colors;
            Frame(image.rectTransform,Vector2.zero,Vector2.one,new Color(0.72f,0.72f,0.86f,0.46f));
            var label=Text(title+" Label",image.transform,title,24,new Color(0.96f,0.94f,0.90f));
            label.characterSpacing=3; Stretch(label.rectTransform,Vector2.zero,Vector2.zero);
            var visibility=image.gameObject.AddComponent<CanvasGroup>();
            var restart=image.gameObject.AddComponent<MoonwingRestartButton>();
            restart.button=button; restart.visibility=visibility; restart.victorySequence=sequence;
            UnityEventTools.AddPersistentListener(button.onClick,restart.Restart);
            if(sequence) { visibility.alpha=0; visibility.interactable=visibility.blocksRaycasts=false; button.interactable=false; }
        }
    }
}
