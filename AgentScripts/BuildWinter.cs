using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildWinter
{
    static string root = "Assets/Winter";
    static Dictionary<string, Material> materials;
    static Mesh quad, circle, triangle;
    static int meshId;
    static System.Random random;
    static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
    static Material Mat(string color)
    {
        if (materials.TryGetValue(color, out var m)) return m;
        m = new Material(Shader.Find("Sprites/Default")) { color = C(color), name = color };
        AssetDatabase.CreateAsset(m, root + "/Art/" + color + ".mat");
        materials[color] = m;
        return m;
    }
    static Mesh MeshOf(string name, Vector2[] points, Color[] colors = null)
    {
        var mesh = new Mesh { name = name };
        var vertices = new Vector3[points.Length];
        for (int i=0;i<points.Length;i++) vertices[i] = points[i];
        var indices = new int[(points.Length-2)*3];
        for(int i=0;i<points.Length-2;i++) { indices[i*3]=0; indices[i*3+1]=i+1; indices[i*3+2]=i+2; }
        mesh.vertices=vertices; mesh.triangles=indices;
        mesh.uv=new Vector2[points.Length];
        if(colors != null) mesh.colors=colors;
        mesh.RecalculateBounds(); mesh.RecalculateNormals();
        AssetDatabase.CreateAsset(mesh, root + "/Art/" + name + "_" + meshId++ + ".asset");
        return mesh;
    }
    static GameObject Shape(string name, Transform parent, Mesh mesh, float x,float y,float sx,float sy,string color,int order,float angle=0)
    {
        var g = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
        g.transform.SetParent(parent,false); g.transform.localPosition=new Vector3(x,y,0);
        g.transform.localScale=new Vector3(sx,sy,1); g.transform.localRotation=Quaternion.Euler(0,0,angle);
        g.GetComponent<MeshFilter>().sharedMesh=mesh;
        var r=g.GetComponent<MeshRenderer>(); r.sharedMaterial=Mat(color); r.sortingOrder=order;
        return g;
    }
    static Transform Group(string name,Transform parent=null,float x=0,float y=0)
    {
        var g=new GameObject(name).transform; g.SetParent(parent,false);g.localPosition=new Vector3(x,y,0);return g;
    }
    static void Ellipse(string n,Transform p,float x,float y,float w,float h,string c,int z) { Shape(n,p,circle,x,y,w,h,c,z); }
    static GameObject Poly(string n,Transform p,Vector2[] pts,string c,int z) { return Shape(n,p,MeshOf(n,pts),0,0,1,1,c,z); }
    static void Tree(Transform p,float x,float y,float size,int layer,bool snowy)
    {
        var t=Group("Snow pine",p,x,y);
        Shape("Trunk",t,quad,0,size*.28f,size*.08f,size*.56f,"203D50",layer);
        for(int i=0;i<4;i++)
        {
            float width=size*(.63f-i*.12f), height=size*.43f, cy=size*(.27f+i*.19f);
            Shape("Pine tier",t,triangle,0,cy,width,height,snowy ? "20576A" : "28485F",layer+1);
            Shape("Pine shadow",t,triangle,width*.095f,cy-height*.075f,width*.6f,height*.76f,snowy ? "193E55" : "234157",layer+2);
            if(snowy) Shape("Snow tier",t,triangle,-width*.015f,cy+height*.155f,width*.71f,height*.64f,"B6DBE2",layer+3);
        }
    }
    static void Mountain(Transform p,float x,float baseline,float w,float h,string color,int layer)
    {
        var t=Group("Faceted mountain",p,x,baseline);
        Poly("Mountain",t,new[]{new Vector2(-w/2,0),new Vector2(w/2,0),new Vector2(w*.02f,h)},color,layer);
        Poly("Shadow face",t,new[]{new Vector2(w*.02f,h),new Vector2(w/2,0),new Vector2(-w*.05f,0)},"243D5B",layer+1);
        Poly("Snow peak",t,new[]{new Vector2(-w*.17f,h*.63f),new Vector2(-w*.05f,h*.70f),new Vector2(w*.03f,h*.61f),new Vector2(w*.1f,h*.74f),new Vector2(w*.2f,h*.63f),new Vector2(w*.02f,h)},"91BDCE",layer+2);
        Poly("Peak shade",t,new[]{new Vector2(w*.02f,h),new Vector2(w*.03f,h*.61f),new Vector2(w*.1f,h*.74f),new Vector2(w*.2f,h*.63f)},"608EA9",layer+3);
    }
    static Transform Platform(Transform parent,int index,float x,float y,float w)
    {
        var p=Group("Snow shelf " + index.ToString("00"),parent,x,y);
        Poly("Ice body",p,new[]{new Vector2(-w/2,0),new Vector2(-w*.45f,-.5f),new Vector2(-w*.2f,-.7f),new Vector2(w*.28f,-.57f),new Vector2(w*.47f,-.32f),new Vector2(w/2,0)},"39677F",10);
        Poly("Ice facet",p,new[]{new Vector2(-w/2,0),new Vector2(-w*.2f,-.7f),new Vector2(-w*.07f,-.12f),new Vector2(w/2,0)},"5895A9",11);
        Poly("Blue edge",p,new[]{new Vector2(-w/2,0),new Vector2(-w*.44f,-.2f),new Vector2(w*.47f,-.16f),new Vector2(w/2,.03f)},"86CDD6",12);
        Shape("Snow cap",p,quad,0,.055f,w,.17f,"E3F3EE",14);
        Ellipse("Soft left edge",p,-w/2+.13f,.06f,.4f,.20f,"E3F3EE",14);
        Ellipse("Soft right edge",p,w/2-.13f,.055f,.4f,.20f,"E3F3EE",14);
        for(int j=0;j<5;j++)
        {
            float xx=-w*.38f+j*w*.18f;
            Ellipse("Snow pillow",p,xx,.14f,.38f+(j%2)*.22f,.16f,"F2F9F1",15);
            if(j%2==0) Shape("Icicle",p,triangle,xx,-.45f,.10f,.52f,"A3D8DE",13,180);
        }
        var collider=p.gameObject.AddComponent<BoxCollider2D>(); collider.size=new Vector2(w,.24f);collider.offset=new Vector2(0,.03f);collider.usedByEffector=true;
        var effector=p.gameObject.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.surfaceArc=165;effector.useSideFriction=false;effector.useSideBounce=false;
        return p;
    }
    static Transform Lamp(Transform parent,float x,float y,float size)
    {
        var p=Group("Amber lantern",parent,x,y);p.localScale=Vector3.one*size;
        Ellipse("Wide halo",p,0,.42f,1.6f,1.6f,"FFC5740A",19);
        Ellipse("Warm halo",p,0,.42f,.95f,.95f,"FFC57418",20);
        Shape("Glass",p,quad,0,.4f,.27f,.4f,"FFC67A",24);
        Shape("Flame",p,triangle,0,.4f,.10f,.22f,"FFF3C1",25);
        Shape("Frame L",p,quad,-.16f,.4f,.055f,.48f,"344552",26);
        Shape("Frame R",p,quad,.16f,.4f,.055f,.48f,"344552",26);
        Shape("Foot",p,quad,0,.14f,.42f,.08f,"344552",26);
        Shape("Roof",p,triangle,0,.7f,.5f,.22f,"344552",26);
        return p;
    }
    static AudioClip Audio(string name,int kind,float seconds)
    {
        const int rate=22050;int n=(int)(rate*seconds);var rng=new System.Random(432+kind);float filtered=0;
        string path=root+"/Audio/"+name+".wav";
        using(var writer=new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+n*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(n*2);
            for(int i=0;i<n;i++)
            {
                double t=(double)i/rate,u=t/seconds,noise=rng.NextDouble()*2-1,s=0;
                filtered=filtered*.985f+(float)noise*.015f;
                if(kind==0) s=(filtered*.85+Math.Sin(t*2*Math.PI*110)*.022+Math.Sin(t*2*Math.PI*165)*.012)*(.7+.3*Math.Sin(t*2*Math.PI/seconds))*Math.Min(1,Math.Min(t/.3,(seconds-t)/.3));
                if(kind==1) s=(Math.Sin(2*Math.PI*(300*t+650*t*t))*.2+filtered*.3)*Math.Sin(Math.PI*u)*Math.Exp(-u*2);
                if(kind==2 || kind==3) s=(noise*.18+Math.Sin(2*Math.PI*85*t)*.13)*Math.Exp(-u*9)*Math.Min(1,t/.006);
                if(kind==4) s=(Math.Sin(t*2*Math.PI*880)*.18+Math.Sin(t*2*Math.PI*1320)*.09+Math.Sin(t*2*Math.PI*1760)*.035)*Math.Exp(-u*5)*Math.Min(1,t/.008);
                if(kind==5)
                {
                    double[] notes={523.25,659.25,783.99,1046.5};
                    for(int k=0;k<4;k++){double tt=t-k*.22;if(tt>0)s+=Math.Sin(tt*2*Math.PI*notes[k])*.14*Math.Exp(-tt*4)*Math.Min(1,tt/.01);}
                }
                s*=Math.Min(1,(seconds-t)/.015);
                writer.Write((short)(Math.Max(-.95,Math.Min(.95,s))*32767));
            }
        }
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    public static string Main()
    {
        if(EditorApplication.isPlaying) throw new Exception("Stop Play Mode first.");
        if(File.Exists(root+"/WinterSummit.unity")) throw new Exception("WinterSummit already exists; edit the existing scene instead of rebuilding.");
        var current=EditorSceneManager.GetActiveScene();
        if(current.isDirty) throw new Exception("Save current scene before building.");
        Directory.CreateDirectory(root+"/Art");Directory.CreateDirectory(root+"/Audio");AssetDatabase.Refresh();
        materials=new Dictionary<string,Material>();meshId=0;random=new System.Random(27);
        quad=MeshOf("Quad",new[]{new Vector2(-.5f,-.5f),new Vector2(.5f,-.5f),new Vector2(.5f,.5f),new Vector2(-.5f,.5f)});
        triangle=MeshOf("Triangle",new[]{new Vector2(-.5f,-.5f),new Vector2(.5f,-.5f),new Vector2(0,.5f)});
        var pts=new Vector2[40];for(int i=0;i<pts.Length;i++)pts[i]=new Vector2(Mathf.Cos(i*Mathf.PI*2/pts.Length)*.5f,Mathf.Sin(i*Mathf.PI*2/pts.Length)*.5f);
        circle=MeshOf("Circle",pts);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));cameraObject.tag="MainCamera";
        var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.1f;cam.transform.position=new Vector3(0,1.8f,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=C("10243D");
        var back=Group("01 • Sky, aurora and distant mountains");
        var sky=MeshOf("Sky gradient",new[]{new Vector2(-30,-15),new Vector2(30,-15),new Vector2(30,20),new Vector2(-30,20)},new[]{C("29465F").linear,C("29465F").linear,C("08152E").linear,C("08152E").linear});
        Shape("Twilight sky",back,sky,0,0,1,1,"FFFFFF",-100);
        // Translucent ribbons, built as strips so the aurora has a soft lower edge.
        for(int band=0;band<3;band++)
        {
            int count=55;var verts=new Vector3[count*2];var colors=new Color[count*2];var tris=new int[(count-1)*6];
            for(int i=0;i<count;i++)
            {
                float x=-16+i*32f/(count-1), y=5.6f+Mathf.Sin(x*.26f+band*.8f)*1.25f+band*.4f;
                verts[i*2]=new Vector3(x,y);verts[i*2+1]=new Vector3(x,y+1.6f+Mathf.Sin(x*.5f)*.5f);
                Color c=C(band==0?"64E9B9":"7BBEE5").linear;c.a=0;colors[i*2]=c;c.a=.14f;colors[i*2+1]=c;
                if(i<count-1){int j=i*6;tris[j]=i*2;tris[j+1]=i*2+1;tris[j+2]=i*2+2;tris[j+3]=i*2+1;tris[j+4]=i*2+3;tris[j+5]=i*2+2;}
            }
            var m=new Mesh{name="Aurora"};m.vertices=verts;m.colors=colors;m.triangles=tris;m.uv=new Vector2[verts.Length];m.RecalculateBounds();AssetDatabase.CreateAsset(m,root+"/Art/Aurora"+band+".asset");
            Shape("Aurora ribbon",back,m,0,0,1,1,"FFFFFF",-95);
        }
        for(int i=0;i<65;i++)
        {
            float x=(float)random.NextDouble()*27-13.5f,y=(float)random.NextDouble()*10+1;
            float s=i%8==0?.055f:.025f;Ellipse("Star",back,x,y,s,s,i%3==0?"CDEAE7":"7FA5BC",-92);
        }
        Ellipse("Moon halo",back,6.7f,6.0f,2.7f,2.7f,"C8EFE70A",-91);
        Ellipse("Moon halo inner",back,6.7f,6,1.9f,1.9f,"C8EFE710",-90);
        Ellipse("Winter moon",back,6.7f,6,1.12f,1.12f,"D9EFE3",-89);
        Ellipse("Moon crater",back,6.85f,6.15f,.2f,.2f,"C3DFD8",-88);
        Ellipse("Moon crater small",back,6.48f,5.8f,.12f,.12f,"C3DFD8",-88);
        Mountain(back,-8,-4,11,9,"405777",-80);Mountain(back,-1.8f,-4,10,8,"425E7D",-76);Mountain(back,5,-4,10,9.2f,"3B5874",-72);Mountain(back,12,-4,10,7,"3B5874",-70);
        Ellipse("Valley mist",back,0,-2.9f,35,3.0f,"8FC7CD16",-60);
        for(int i=0;i<30;i++)Tree(back,-16+i*1.1f,-3.7f,1.4f+(float)random.NextDouble()*2,-55,false);
        var forest=Group("02 • Snow forest");
        Tree(forest,-7.6f,-3.2f,5.4f,-30,true);Tree(forest,-9.6f,-3.6f,4.2f,-26,true);Tree(forest,8.0f,-3.0f,5.8f,-30,true);Tree(forest,10.2f,-3.5f,4.5f,-25,true);
        var terrain=Group("03 • Climbing trail");
        float[] xs={0,-3.8f,-.3f,3.3f,.9f,-2.6f,-4.2f,-.8f,2.8f,4.1f,.8f,-2.7f,-.3f,3.1f,.2f,-3.2f,0};
        var lights=new List<Transform>();
        for(int i=0;i<xs.Length;i++)
        {
            float y=-2.5f+i*1.65f,w=i==0?13:i==16?4.6f:2.55f+(i%3)*.22f;
            var p=Platform(terrain,i,xs[i],y,w);
            if(i>0 && i%2==1)
            {
                var orb=Group("Trail light " + lights.Count,terrain,xs[i],y+.92f);
                Ellipse("Glow outer",orb,0,0,.9f,.9f,"FFD68710",28);Ellipse("Glow inner",orb,0,0,.48f,.48f,"FFD68728",29);
                Shape("Golden crystal",orb,quad,0,0,.13f,.13f,"FFE8A8",30,45);
                lights.Add(orb);
            }
            if(i%4==0 && i>0 && i<16)
            {
                Tree(p,-w*.3f,.13f,1.3f,-5,true);
                Lamp(p,w*.3f,.1f,.7f);
            }
        }
        Ellipse("Snowbank rear",forest,0,-3.4f,27,1.8f,"B9DADE",-15);
        Ellipse("Snowbank front",forest,-5,-4.1f,19,1.6f,"D7EAE4",16);
        Ellipse("Snowbank front right",forest,7,-4.1f,16,1.7f,"C5E0DE",17);
        Lamp(terrain,-5.7f,-2.35f,1.1f);
        var sign=Group("Trail sign",terrain,5.2f,-2.35f);
        Shape("Sign post",sign,quad,0,.36f,.1f,.72f,"72545A",20);
        Shape("Arrow board",sign,quad,0,.77f,.85f,.3f,"956C68",21);
        Shape("Arrow",sign,triangle,.13f,.77f,.19f,.22f,"F2D9BD",22,-90);
        var player=new GameObject("Traveller • red parka",typeof(BoxCollider2D),typeof(PlayerMovement),typeof(WinterAdventure));
        player.transform.position=new Vector3(-4.5f,-1.71f,0);
        var box=player.GetComponent<BoxCollider2D>();box.size=new Vector2(.52f,1.02f);box.offset=new Vector2(0,-.03f);
        var move=new SerializedObject(player.GetComponent<PlayerMovement>());move.FindProperty("horizontalWrapLimit").floatValue=6.3f;move.ApplyModifiedPropertiesWithoutUndo();
        var art=Group("Character artwork",player.transform);
        Ellipse("Backpack",art,-.25f,-.02f,.36f,.5f,"364C61",39);
        Shape("Boot L",art,quad,-.15f,-.46f,.21f,.19f,"233A4F",45);
        Shape("Boot R",art,quad,.15f,-.46f,.21f,.19f,"233A4F",45);
        Ellipse("Parka",art,0,-.1f,.63f,.61f,"C85153",43);
        Shape("Jacket highlight",art,quad,-.09f,-.14f,.12f,.38f,"DF7063",44);
        Shape("Zip",art,quad,.035f,-.14f,.027f,.38f,"F5D9B1",45);
        Ellipse("Left mitten",art,-.33f,-.16f,.19f,.23f,"EBA566",46);
        Ellipse("Right mitten",art,.33f,-.16f,.19f,.23f,"EBA566",46);
        Ellipse("Hood",art,0,.26f,.76f,.76f,"D56259",46);
        Ellipse("Fur rim",art,.025f,.26f,.62f,.62f,"F5E6CC",47);
        Ellipse("Face shadow",art,.055f,.26f,.47f,.47f,"D89679",48);
        Ellipse("Face",art,.075f,.29f,.43f,.43f,"F3C49B",49);
        Ellipse("Eye L",art,-.035f,.3f,.044f,.071f,"2B394B",50);
        Ellipse("Eye R",art,.16f,.3f,.044f,.071f,"2B394B",50);
        Ellipse("Cheek",art,.21f,.20f,.09f,.055f,"E89881",50);
        Shape("Fringe",art,quad,.02f,.47f,.30f,.085f,"5E4546",50,-6);
        Shape("Scarf collar",art,quad,.03f,.00f,.58f,.13f,"EAB361",51);
        var scarf=Group("Fluttering scarf",art,-.23f,.02f);
        Shape("Scarf tail",scarf,quad,-.21f,.04f,.46f,.13f,"F4C875",42,-12);
        Shape("Scarf tip",scarf,triangle,-.46f,.09f,.16f,.19f,"F4C875",42,95);
        var goal=Group("04 • Summit lantern",terrain,0,-2.5f+16*1.65f+.15f);
        Shape("Lookout post",goal,quad,0,1.15f,.14f,2.3f,"6A555D",20);
        Shape("Beam",goal,quad,.32f,2.3f,.8f,.13f,"956F6B",21);
        Shape("Hanging cord",goal,quad,.6f,2.1f,.035f,.45f,"364C61",22);
        Lamp(goal,.6f,1.05f,1.2f);
        Shape("Summit pennant",goal,triangle,-.37f,1.9f,.42f,.75f,"D86860",22,90);
        var snowRoot=Group("05 • Falling snow",cam.transform);
        snowRoot.localPosition=new Vector3(0,0,10);
        var flakes=new Transform[105];
        for(int i=0;i<flakes.Length;i++)
        {
            float s=.019f+(i%5)*.009f;
            flakes[i]=Shape("Snowflake",snowRoot,circle,(float)random.NextDouble()*26-13,(float)random.NextDouble()*13-6.5f,s,s,i%3==0?"E7F6EDC0":"BADDE780",55).transform;
        }
        var game=player.GetComponent<WinterAdventure>();game.sceneCamera=cam;game.backdrop=back;game.artwork=art;game.scarf=scarf;game.snowflakes=flakes;game.lights=lights.ToArray();game.summit=goal;
        game.wind=Audio("Quiet winter wind",0,16);game.jump=Audio("Snow jump",1,.28f);game.landing=Audio("Soft landing",2,.26f);game.footstep=Audio("Snow step",3,.15f);game.collect=Audio("Warm light",4,.85f);game.finish=Audio("Summit chime",5,1.8f);
        EditorSceneManager.SaveScene(scene,root+"/WinterSummit.unity");AssetDatabase.SaveAssets();
        Selection.activeGameObject=player;
        if(SceneView.lastActiveSceneView!=null){SceneView.lastActiveSceneView.in2DMode=true;SceneView.lastActiveSceneView.LookAt(new Vector3(0,1.8f,0),Quaternion.identity,6.1f,true,true);}
        return "Created WinterSummit: 17 platforms, 8 lights, traveller, layered winter scenery, and 6 original synthesized audio clips.";
    }
}
