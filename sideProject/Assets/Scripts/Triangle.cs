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

    public static float calculateArea(Vector3 A, Vector3 B, Vector3 C) {
        float a =  (B - C).magnitude;
        float b =  (C - A).magnitude;
        float c =  (B - A).magnitude;
        float s = (a + b + c )/ 2;
        return Mathf.Sqrt(s * (s - a)* (s - b) * (s - c));
    }

    public float calculateArea() {
        float a =  (v2 - v3).magnitude;
        float b =  (v3 - v1).magnitude;
        float c =  (v2 - v1).magnitude;
        float s = (a + b + c )/ 2;
        return Mathf.Sqrt(s * (s - a)* (s - b) * (s - c));
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