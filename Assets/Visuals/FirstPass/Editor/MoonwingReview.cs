using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        public static void Validate()
        {
            var player=GameObject.Find("Player"); var magic=player.GetComponent<FairyMagic>(); var spawner=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>();
            if(spawner.totalAssassins!=1 || spawner.player!=player.transform || !spawner.victoryPanel || !magic.capturedPanel || !magic.moonlightBar || Camera.main.GetComponent<CameraFollow>().target!=player.transform) throw new InvalidOperationException("Gameplay binding changed.");
            if(spawner.assassinPrefab.GetComponent<EnemyHealth>().health!=3 || player.GetComponent<PlayerShoot>().arrowPrefab.GetComponent<ArrowDamage>().damage!=1) throw new InvalidOperationException("Three-hit damage baseline changed.");
            var opening=UnityEngine.Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            if(!opening || opening.gameplay.Length<8 || !opening.playButton || opening.playButton.onClick.GetPersistentEventCount()!=1) throw new InvalidOperationException("Opening is not wired.");
            foreach(string path in AssetDatabase.FindAssets("t:Material",new[]{Root,Clearing}).Select(AssetDatabase.GUIDToAssetPath))
            { var material=AssetDatabase.LoadAssetAtPath<Material>(path); if(!material.shader || ShaderUtil.ShaderHasError(material.shader)) throw new InvalidOperationException("Shader failed: "+path); }
            foreach(string name in new[]{"Moonfairy - Moon Goddess Silhouette","Caretaker - Lantern Keeper","Forest Spirit - Gentle Moon Wisp",PassName})
                if(GameObject.Find(name).GetComponentsInChildren<Collider>(true).Length!=0) throw new InvalidOperationException("Unexpected presentation collider: "+name);
            Directory.CreateDirectory("Logs/FirstPassReview");
            var renderers=UnityEngine.Object.FindObjectsByType<MeshRenderer>();
            int triangles=UnityEngine.Object.FindObjectsByType<MeshFilter>().Sum(x=>x.sharedMesh?x.sharedMesh.triangles.Length/3:0);
            File.WriteAllText("Logs/FirstPassReview/Validation.txt","Bindings and shaders passed.\nAssassins = 1; enemy health = 3; arrow damage = 1.\nScene mesh renderers: "+renderers.Length+"\nTriangles across all instances: "+triangles+"\nNo added character/environment colliders.\n");
            Debug.Log("MOONWING_FULL_PASS_VALIDATED");
        }

        public static void CapturePreviews()
        {
            Camera camera=Camera.main;
            Vector3 pos=camera.transform.position; Quaternion rotation=camera.transform.rotation; float fov=camera.fieldOfView;
            var canvas=GameObject.Find("Canvas").GetComponent<Canvas>(); var originalMode=canvas.renderMode; var originalCamera=canvas.worldCamera; float distance=canvas.planeDistance;
            var opening=UnityEngine.Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            var victory=GameObject.Find("AssassinSpawner").GetComponent<AssassinSpawner>().victoryPanel; var captured=GameObject.Find("Player").GetComponent<FairyMagic>().capturedPanel;
            bool victoryActive=victory.activeSelf,capturedActive=captured.activeSelf,openingActive=opening.gameObject.activeSelf;
            float meterAlpha=opening.meter.alpha;
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1;
                victory.SetActive(false); captured.SetActive(false); opening.gameObject.SetActive(false); opening.meter.alpha=1;
                foreach(var ps in UnityEngine.Object.FindObjectsByType<ParticleSystem>()) ps.Simulate(2,true,true);
                Canvas.ForceUpdateCanvases(); Render("01 - Gameplay Foundation");
                int quality=QualitySettings.GetQualityLevel();
                try { QualitySettings.SetQualityLevel(0,true); Render("18 - Mobile Quality Foundation"); }
                finally { QualitySettings.SetQualityLevel(quality,true); }
                opening.gameObject.SetActive(true); opening.meter.alpha=0; opening.narration.alpha=1; opening.title.alpha=0; Render("02 - Caretaker Opening");
                opening.narration.alpha=0; opening.title.alpha=1; Render("03 - Moonfairy Title");
                opening.gameObject.SetActive(false); victory.SetActive(true); Render("04 - Victory"); victory.SetActive(false); captured.SetActive(true); Render("05 - Captured"); captured.SetActive(false);
                canvas.enabled=false;
                Portrait("Player","06 - Moonfairy",new Vector3(1.4f,0.45f,3.7f));
                var spirit=GameObject.Find("ForestSpirit"); spirit.SetActive(false); Portrait("Caretaker","07 - Caretaker",new Vector3(1.3f,0.4f,3.8f)); spirit.SetActive(true);
                var caretaker=GameObject.Find("Caretaker"); caretaker.SetActive(false); Portrait("ForestSpirit","08 - Forest Spirit",new Vector3(1.2f,0.4f,3.7f)); caretaker.SetActive(true);
                var assassin=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Assassin_1.prefab")); assassin.name="Assassin Review Only"; assassin.transform.position=new Vector3(0,1,7);
                try { Portrait(assassin.name,"09 - Assassin",new Vector3(1.4f,0.35f,3.8f)); } finally { UnityEngine.Object.DestroyImmediate(assassin); }
                camera.transform.position=new Vector3(-13,3,-7); camera.transform.LookAt(new Vector3(-15,2,-14)); camera.fieldOfView=58; Render("10 - Lantern Garden Home");
                camera.transform.position=new Vector3(28,32,-32); camera.transform.LookAt(new Vector3(0,0,0)); camera.fieldOfView=58; Render("11 - Forest Coverage");
            }
            finally
            {
                camera.transform.SetPositionAndRotation(pos,rotation); camera.fieldOfView=fov;
                canvas.enabled=true; canvas.renderMode=originalMode; canvas.worldCamera=originalCamera; canvas.planeDistance=distance;
                victory.SetActive(victoryActive); captured.SetActive(capturedActive); opening.gameObject.SetActive(openingActive); opening.meter.alpha=meterAlpha;
                opening.narration.alpha=1; opening.title.alpha=0;
            }
            Debug.Log("MOONWING_FULL_PASS_PREVIEWS_CAPTURED");
        }
        static void Portrait(string name,string file,Vector3 offset)
        {
            var target=GameObject.Find(name).transform; Camera.main.transform.position=target.position+offset; Camera.main.transform.LookAt(target.position+Vector3.up*0.05f); Camera.main.fieldOfView=40; Render(file);
        }
        public static void Render(string name)
        {
            Directory.CreateDirectory("Logs/FirstPassReview"); Camera camera=Camera.main;
            var rt=new RenderTexture(1440,900,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB); rt.Create();
            var active=RenderTexture.active; var previousTarget=camera.targetTexture; float previousAspect=camera.aspect;
            try
            {
                camera.targetTexture=rt; camera.aspect=1440f/900f;
                Canvas.ForceUpdateCanvases(); RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest { destination=rt });
                RenderTexture.active=rt; var texture=new Texture2D(1440,900,TextureFormat.RGB24,false);
                texture.ReadPixels(new Rect(0,0,1440,900),0,0); texture.Apply(); File.WriteAllBytes("Logs/FirstPassReview/"+name+".png",texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
            }
            finally { camera.targetTexture=previousTarget; camera.aspect=previousAspect; RenderTexture.active=active; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
        }
    }
}
