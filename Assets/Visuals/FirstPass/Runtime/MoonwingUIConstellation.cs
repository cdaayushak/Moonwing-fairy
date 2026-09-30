using UnityEngine;
using UnityEngine.UI;

namespace Moonwing.Visuals
{
    public sealed class MoonwingUIConstellation : MonoBehaviour
    {
        public Graphic[] stars;
        void Update()
        {
            for(int i=0;i<stars.Length;i++)
            {
                if(!stars[i]) continue;
                Color c=stars[i].color;
                c.a=0.2f+0.38f*(0.5f+0.5f*Mathf.Sin(Time.unscaledTime*(0.5f+(i%5)*0.13f)+i*2.399f));
                stars[i].color=c;
            }
        }
    }
}
