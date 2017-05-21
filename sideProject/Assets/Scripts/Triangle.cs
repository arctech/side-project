using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle {

    private Vector3 v1;
    private Vector3 v2;
    private Vector3 v3;
    private Vector3 centroid;

    public Triangle()
    {
        v1 = Vector3.zero;
        v2 = Vector3.zero;
        v3 = Vector3.zero;
    }

     public Triangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        v1 = vertex1;
        v2 = vertex2;
        v3 = vertex3;
        setCentroid();
    }


    private void setCentroid() {
         centroid = 1.0f / 3.0f *  (v1 + v2 +v3);
    }
    
    public Vector3 Vertex1{
        get {
            return v1;
        }

        set {
            v1 = value;
        }
    }

      public Vector3 Vertex2{
        get {
            return v2;
        }

        set {
            v2 = value;
        }
    }

      public Vector3 Vertex3{
        get {
            return v3;
        }

        set {
            v3 = value;
        }
    }

      public Vector3 Centroid{
        get {
            return centroid;
        }

        set {
            centroid = value;
        }
    }
}