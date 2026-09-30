using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonwing.Visuals.Editor
{
    internal sealed class MoonwingMesh
    {
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<Color> c = new List<Color>();
        readonly List<int> t = new List<int>();
        int V(Vector3 p, Color color) { v.Add(p); c.Add(color); return v.Count - 1; }
        void T(int a, int b, int d) { t.Add(a); t.Add(b); t.Add(d); }
        public void Quad(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Color color)
        { int n=V(a,color); V(b,color); V(d,color); V(e,color); T(n,n+1,n+2); T(n,n+2,n+3); }
        public void Quad(Vector3 a, Vector3 b, Vector3 d, Vector3 e, Color ca, Color cb, Color cd, Color ce)
        { int n=V(a,ca); V(b,cb); V(d,cd); V(e,ce); T(n,n+1,n+2); T(n,n+2,n+3); }
        public void Ellipsoid(Vector3 p, Vector3 scale, Color color, int sides=16, int rings=10)
        {
            int n=v.Count;
            for(int i=0;i<=rings;i++) for(int j=0;j<sides;j++)
            {
                float theta=i*Mathf.PI/rings, a=j*Mathf.PI*2/sides;
                V(p+Vector3.Scale(new Vector3(Mathf.Sin(theta)*Mathf.Cos(a),Mathf.Cos(theta),Mathf.Sin(theta)*Mathf.Sin(a)),scale),color);
            }
            for(int i=0;i<rings;i++) for(int j=0;j<sides;j++)
            { int a=n+i*sides+j,b=n+i*sides+(j+1)%sides; T(a,b,b+sides); T(a,b+sides,a+sides); }
        }
        public void Lathe(Vector3 p, Vector2[] profile, Color color, int sides=24, float zScale=1, float pleats=0)
        {
            int n=v.Count;
            for(int i=0;i<profile.Length;i++) for(int j=0;j<sides;j++)
            {
                float a=j*Mathf.PI*2/sides;
                float r=profile[i].x*(1+pleats*Mathf.Cos(a*8));
                V(p+new Vector3(r*Mathf.Cos(a),profile[i].y,r*Mathf.Sin(a)*zScale),color);
            }
            for(int i=0;i<profile.Length-1;i++) for(int j=0;j<sides;j++)
            { int a=n+i*sides+j,b=n+i*sides+(j+1)%sides; T(a,b,b+sides); T(a,b+sides,a+sides); }
        }
        public void Tube(Vector3[] points, float start, float end, Color color, int sides=8)
        {
            int n=v.Count;
            for(int i=0;i<points.Length;i++)
            {
                Vector3 tangent=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)]).normalized;
                Vector3 x=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>0.9f?Vector3.right:Vector3.up).normalized;
                Vector3 y=Vector3.Cross(tangent,x);
                float radius=Mathf.Lerp(start,end,i/(float)(points.Length-1));
                for(int j=0;j<sides;j++) { float a=j*Mathf.PI*2/sides; V(points[i]+(x*Mathf.Cos(a)+y*Mathf.Sin(a))*radius,color); }
            }
            for(int i=0;i<points.Length-1;i++) for(int j=0;j<sides;j++)
            { int a=n+i*sides+j,b=n+i*sides+(j+1)%sides; T(a,b,b+sides); T(a,b+sides,a+sides); }
        }
        public void Line(Vector3 a,Vector3 b,float radius,Color color) { Tube(new[]{a,b},radius,radius,color); }
        public void Box(Vector3 center,Vector3 size,Color color)
        {
            Vector3 h=size*0.5f;
            Vector3[] p={center+new Vector3(-h.x,-h.y,-h.z),center+new Vector3(h.x,-h.y,-h.z),center+new Vector3(h.x,h.y,-h.z),center+new Vector3(-h.x,h.y,-h.z),center+new Vector3(-h.x,-h.y,h.z),center+new Vector3(h.x,-h.y,h.z),center+new Vector3(h.x,h.y,h.z),center+new Vector3(-h.x,h.y,h.z)};
            Quad(p[0],p[3],p[2],p[1],color); Quad(p[4],p[5],p[6],p[7],color);
            Quad(p[0],p[4],p[7],p[3],color); Quad(p[1],p[2],p[6],p[5],color);
            Quad(p[3],p[7],p[6],p[2],color); Quad(p[0],p[1],p[5],p[4],color);
        }
        public void Diamond(Vector3 p,Vector3 size,Color color)
        {
            int n=V(p+Vector3.up*size.y,color); V(p+Vector3.down*size.y,color);
            V(p+Vector3.right*size.x,color); V(p+Vector3.forward*size.z,color); V(p+Vector3.left*size.x,color); V(p+Vector3.back*size.z,color);
            for(int i=0;i<4;i++) { int a=n+2+i,b=n+2+(i+1)%4; T(n,a,b); T(n+1,b,a); }
        }
        public void Ribbon(Vector3[] points,float width,Color color)
        {
            for(int i=0;i<points.Length-1;i++)
            {
                float w=width*(1-i/(float)points.Length*0.7f),next=width*(1-(i+1)/(float)points.Length*0.7f);
                Quad(points[i]-Vector3.right*w,points[i+1]-Vector3.right*next,points[i+1]+Vector3.right*next,points[i]+Vector3.right*w,color);
            }
        }
        public void Wing(float side,bool upper,Color color, MoonwingMesh veins)
        {
            Vector3[] outline=upper?new[]{new Vector3(0,0,0),new Vector3(0.18f,0.48f,0),new Vector3(0.70f,1.12f,0.04f),new Vector3(1.13f,1.0f,0.07f),new Vector3(0.88f,0.43f,0.02f),new Vector3(0.38f,0.02f,0)}:
                new[]{new Vector3(0,0,0),new Vector3(0.45f,0.12f,0),new Vector3(0.87f,-0.30f,0.05f),new Vector3(0.75f,-0.63f,0.08f),new Vector3(0.27f,-0.35f,0.02f)};
            List<Vector3> curve=new List<Vector3>();
            for(int i=0;i<outline.Length;i++) for(int s=0;s<6;s++)
            {
                Vector3 a=outline[(i+outline.Length-1)%outline.Length],b=outline[i],d=outline[(i+1)%outline.Length],e=outline[(i+2)%outline.Length];
                float k=s/6f; Vector3 p=0.5f*((2*b)+(-a+d)*k+(2*a-5*b+4*d-e)*k*k+(-a+3*b-3*d+e)*k*k*k); p.x*=side; curve.Add(p);
            }
            Vector3 center=new Vector3(side*0.43f,upper?0.45f:-0.2f,0);
            int n=V(center,new Color(color.r,color.g,color.b,0.38f));
            foreach(var p in curve) V(p,new Color(color.r,color.g,color.b,0));
            for(int i=0;i<curve.Count;i++) T(n,n+1+i,n+1+(i+1)%curve.Count);
            for(int i=1;i<outline.Length-1;i++)
            {
                Vector3 p=outline[i]; p.x*=side;
                veins.Tube(Curve(Vector3.zero,center*0.6f,p*0.92f,8),0.003f,0.0008f,new Color(0.7f,0.75f,0.95f,0.20f),5);
            }
        }
        public Mesh Save(string name)
        {
            var mesh=new Mesh { name=name,indexFormat=v.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16 };
            mesh.SetVertices(v); mesh.SetColors(c); mesh.SetTriangles(t,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,"Assets/Visuals/FirstPass/Meshes/"+name+".asset"); return mesh;
        }
        public static Vector3[] Curve(Vector3 a,Vector3 b,Vector3 c,int count=10)
        {
            Vector3[] p=new Vector3[count];
            for(int i=0;i<count;i++) { float t=i/(float)(count-1); p[i]=(1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c; } return p;
        }
    }
}
