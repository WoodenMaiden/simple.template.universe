using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonGenerator
{
    CoordinateConverter converter;
    float offsetYBackgroundGeom;

    public PolygonGenerator(CoordinateConverter c, float Yoffset)
    {
        converter = c;
        offsetYBackgroundGeom = Yoffset;
    }
    public void GeneratePolygons(GAMAGeometry geom)
    {
        GameObject generated = new GameObject();
        generated.name = "generated";


        List<Vector2> pts = new List<Vector2>();
        int cpt = 0;
        for (int i = 0; i < geom.points.Count; i++)
        {
            GAMAPoint pt = geom.points[i];
            if (pt.c.Count < 2)
            {
                if (pts.Count > 2)
                {
                    GameObject p = GeneratePolygon(pts.ToArray(), geom.names.Count > 0 ?  geom.names[cpt] : "", geom.heights[cpt], geom.hasColliders[cpt]);
                    p.transform.SetParent(generated.transform);
                }
                pts = new List<Vector2>();
                cpt++;
            }
            else
            {
                pts.Add(converter.fromGAMACRS2D(pt.c[0], pt.c[1]));
            }


        }
    }

    // Start is called before the first frame update
    GameObject GeneratePolygon(Vector2[] MeshDataPoints, string name, float extrusionHeight, bool isUsingCollider)
    {
        bool is3D = true;
        bool isUsingBottomMeshIn3D = false;
        bool isOutlineRendered = false;

        // create new GameObject (as a child)
        GameObject polyExtruderGO = new GameObject();
        if (name != "")
            polyExtruderGO.name = name;

        // reference to setup example poly extruder 
        PolyExtruder polyExtruder;

        // add PolyExtruder script to newly created GameObject and keep track of its reference
        polyExtruder = polyExtruderGO.AddComponent<PolyExtruder>();

        // global PolyExtruder configurations
        polyExtruder.isOutlineRendered = isOutlineRendered;
        Vector3 pos = polyExtruderGO.transform.position;
        pos.y += offsetYBackgroundGeom;
        polyExtruderGO.transform.position = pos;
        polyExtruder.createPrism(polyExtruderGO.name, extrusionHeight, MeshDataPoints, Color.grey, is3D, isUsingBottomMeshIn3D, isUsingCollider);
        if (isUsingCollider)
        {
            MeshCollider mc = polyExtruderGO.AddComponent<MeshCollider>();
            mc.sharedMesh = polyExtruder.surroundMesh;


        }
        return polyExtruderGO;


    }
}


