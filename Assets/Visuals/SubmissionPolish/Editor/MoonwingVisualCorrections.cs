using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Moonwing.Visuals.Editor
{
    public static partial class MoonwingFirstPassBuilder
    {
        public static void PolishVisualCorrections()
        {
            EditorSceneManager.OpenScene(ScenePath);
            var state=Random.state;
            try
            {
                Random.InitState(814);
                SoftenSubmissionPanels();
                if(!GameObject.Find("Cloud Belt - Verified Game View")) DenseCloudBelt();
                ClearNearCameraFoliage();
                AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                ValidateSubmission(); SubmissionPreviews();
                Debug.Log("MOONWING_VISUAL_CORRECTIONS_BUILT");
            }
            finally { Random.state=state; }
        }
        static void ClearNearCameraFoliage()
        {
            foreach(string name in new[]{"Lavender Canopy","Silver Indigo Bark"})
            {
                var material=AssetDatabase.LoadAssetAtPath<Material>(Clearing+"/Materials/"+name+".mat");
                if(name=="Silver Indigo Bark") material.shader=Shader.Find("Moonwing/Veiled Bark");
                material.SetFloat("_CameraFade",1); EditorUtility.SetDirty(material);
            }
            string path=Polish+"/Materials/Distant Leaf Clearance.mat";
            var leaves=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!leaves) { leaves=new Material(LoadMat("Moonfairy Silks")); AssetDatabase.CreateAsset(leaves,path); }
            leaves.SetFloat("_CameraFade",1); EditorUtility.SetDirty(leaves);
            for(int i=0;i<3;i++)
            {
                string prefabPath=Root+"/Prefabs/Submission Mist Tree "+i+".prefab";
                var tree=PrefabUtility.LoadPrefabContents(prefabPath);
                try { tree.transform.Find("Submission Distant Canopy "+i).GetComponent<MeshRenderer>().sharedMaterial=leaves; PrefabUtility.SaveAsPrefabAsset(tree,prefabPath); }
                finally { PrefabUtility.UnloadPrefabContents(tree); }
            }
        }
        static void SoftenSubmissionPanels()
        {
            var feather=SpriteAsset("Feathered Moonwing Veil",256,128,(x,y)=>
            {
                float shape=Mathf.Pow(Mathf.Abs(x*2-1),4)+Mathf.Pow(Mathf.Abs(y*2-1),4);
                return new Color(1,1,1,1-Smooth(0.15f,1.12f,shape));
            });
            var accent=SpriteAsset("Quiet Moon Flourish",128,64,(x,y)=>
            {
                float line=(1-Smooth(0.007f,0.025f,Mathf.Abs(y-0.12f)))*(1-Smooth(0.08f,0.26f,Mathf.Abs(x-0.5f)));
                return new Color(1,1,1,line*0.65f);
            });
            var vignette=SpriteAsset("Moonwing Ending Vignette",128,128,(x,y)=>
            {
                float radius=new Vector2((x-0.5f)*1.5f,(y-0.5f)*1.8f).magnitude;
                return new Color(1,1,1,Mathf.Lerp(0.50f,0.98f,Smooth(0.10f,0.9f,radius)));
            });
            var style=AssetDatabase.LoadAssetAtPath<MoonwingDialogueStyle>(Polish+"/Resources/MoonwingDialogueStyle.asset");
            style.panel=feather; style.border=accent; EditorUtility.SetDirty(style);
            var opening=Object.FindAnyObjectByType<MoonwingOpening>(FindObjectsInactive.Include);
            var panel=opening.narration.transform.Find("Caretaker Story Panel");
            var image=panel.GetComponent<Image>(); image.sprite=feather; image.type=UnityEngine.UI.Image.Type.Simple; image.color=new Color(0.018f,0.025f,0.055f,0.76f);
            var frame=panel.Find("Silver Filigree Frame"); if(frame) frame.gameObject.SetActive(false);
            foreach(var ending in opening.endingPanels)
            {
                ending.GetComponent<Image>().sprite=vignette; ending.GetComponent<Image>().type=UnityEngine.UI.Image.Type.Simple;
                foreach(var child in ending.GetComponentsInChildren<RectTransform>(true)) if(child.name=="Silver Filigree Frame") child.gameObject.SetActive(false);
            }
        }
        static void DenseCloudBelt()
        {
            var root=Group("Cloud Belt - Verified Game View",GameObject.Find("Moonwing - Submission Atmosphere").transform);
            var ground=GameObject.Find("Ground").transform; ground.localScale=new Vector3(14,ground.localScale.y,14);
            // All orbit positions (player radius 38 + camera distance) stay inside these walls.
            // The outer layer is effectively opaque below its irregular cloud crest.
            float[] radii={56,65,77}; float[] opacities={0.46f,0.72f,1f};
            for(int layer=0;layer<3;layer++)
            {
                var material=new Material(Shader.Find("Moonwing/Enchanted Cloud Belt"));
                material.SetColor("_BaseColor",new Color(0.83f,0.86f,0.94f,opacities[layer])); material.SetFloat("_Phase",layer*2.1f);
                AssetDatabase.CreateAsset(material,Polish+"/Materials/Cloud Belt "+layer+".mat");
                var mesh=new MoonwingMesh();
                for(int i=0;i<96;i++)
                {
                    float a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96;
                    float ra=radii[layer]+Mathf.Sin(a*7+layer)*1.1f, rb=radii[layer]+Mathf.Sin(b*7+layer)*1.1f;
                    Vector3 p=new Vector3(Mathf.Cos(a)*ra,-5,Mathf.Sin(a)*ra), q=new Vector3(Mathf.Cos(b)*rb,-5,Mathf.Sin(b)*rb);
                    mesh.Quad(p,q,q+Vector3.up*34,p+Vector3.up*34,Color.white);
                }
                Mesh("Submission Dense Cloud Belt "+layer,mesh,material,root);
                // A feathered horizontal skirt blends each bank into the ground without a hard base line.
                if(layer==0)
                {
                    var skirt=new MoonwingMesh();
                    for(int ring=0;ring<6;ring++) for(int i=0;i<96;i++)
                    {
                        float r0=30+ring*5,r1=r0+5,a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96;
                        var inner=new Color(1,1,1,Mathf.SmoothStep(0,0.7f,ring/6f));
                        var outer=new Color(1,1,1,Mathf.SmoothStep(0,0.7f,(ring+1)/6f));
                        skirt.Quad(new Vector3(Mathf.Cos(a)*r0,0.25f,Mathf.Sin(a)*r0),new Vector3(Mathf.Cos(b)*r0,0.25f,Mathf.Sin(b)*r0),new Vector3(Mathf.Cos(b)*r1,0.25f,Mathf.Sin(b)*r1),new Vector3(Mathf.Cos(a)*r1,0.25f,Mathf.Sin(a)*r1),inner,inner,outer,outer);
                    }
                    Mesh("Submission Cloud Ground Skirt",skirt,material,root);
                }
            }
            string[] trees={"Silverbranch Arch","Lavender Willow","Midnight Alder","Moonbloom Crown"};
            for(int i=0;i<12;i++)
            {
                float a=i*2.399963f, radius=R(34,48);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Clearing+"/Prefabs/"+trees[i%4]+".prefab");
                var tree=Place(prefab,root,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),R(0,360),R(0.65f,1.0f));
                foreach(var renderer in tree.GetComponentsInChildren<MeshRenderer>()) renderer.shadowCastingMode=ShadowCastingMode.Off;
            }
            for(int i=0;i<24;i++)
            {
                float a=i*2.399963f+0.7f,radius=R(43,60);
                Place(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Submission Mist Tree "+(i%3)+".prefab"),root,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius),R(0,360),R(0.8f,1.3f));
            }
        }
    }
}
