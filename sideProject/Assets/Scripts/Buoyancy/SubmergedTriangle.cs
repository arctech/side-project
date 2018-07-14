using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public	class BVertex {
		public Vector3 Position = Vector3.zero;
		public float Depth = 0.0f;
		
		public BVertex(Vector3 pos) {
			Position = pos;
		}

		public BVertex(Vector3 pos, float depth) {
			Position = pos;
			Depth = depth;
		}
	}
	
public class SubmergedTriangle  {
		private BVertex _A;
		private BVertex _B;
		private BVertex _C;
		private BVertex _center;
		public Vector3 ForceCenter;
		public Vector3 Force;
		private Vector3 _normal;
		public Vector3 Velocity;

		private float _area;

		public SubmergedTriangle(BVertex A, BVertex B, BVertex C, Vector3 normal) {
			this._A = A;
			this._B = B;
			this._C = C;

			_center = new BVertex(1.0f / 3.0f *  (_A.Position + _B.Position + _C.Position));
			
			_normal = normal;
		
			_area = MathUtil.calcTriangleArea(_A.Position, _B.Position, _C.Position);
		}

		public Vector3 Normal  {
			get {
				return _normal;
			}
		}

		public BVertex Center {
			get {
				return _center;
			}
		}

		public float Area {
			get {
				return _area;
			}
		}

		public BVertex A
		{
			get {
				return _A;
			}
		}
		public BVertex B
		{
			get {
				return _B;
			}
		}
		public BVertex C
		{
			get {
				return _C;
			}
		}


}
