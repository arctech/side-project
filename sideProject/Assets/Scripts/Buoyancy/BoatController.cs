﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
* Implementation follows: http://www.gamasutra.com/view/news/237528/Water_interaction_model_for_boats_in_video_games.php
 */
public class BoatController : MonoBehaviour {

	class LinePair {
		public Vector3 p1;
		public Vector3 p2;

		public bool marked = false;

		public LinePair(Vector3 v1, Vector3 v2) {
			p1 = v1;
			p2 = v2;
		}

		public LinePair(Vector3 v1, Vector3 v2, bool mark) {
			p1 = v1;
			p2 = v2;
			marked = mark;
		}
	}

	class MeshTriangle {
		public Triangle triangle;
		public Vector3 normal;

		public MeshTriangle(Triangle triangle, Vector3 normal) {
			this.triangle = triangle;
			this.normal = normal;
		}
	}

	[SerializeField]
	public BuoyantSimulationSettings SimSettings = new BuoyantSimulationSettings();

	[SerializeField]
	public BuoyantBodySettings BoatSet = new BuoyantBodySettings();

	[SerializeField]
	public BuoyantDebugSettings DebugSet = new BuoyantDebugSettings();


	public float WaterpatchScaleFactor = 1.5f;
	private List<MeshTriangle> _meshTriangleList = new List<MeshTriangle>();

	private Vector3 _boatMeshCenter = Vector3.zero; // boat mesh bounding box center

	private GameObject _sentinelSphere = null;
	private Mesh _boatMesh = null;

	private string _debugMsg = "";
	private string _debugMsgFixedUpdate = "";

	private WaterPatch _waterPatch = new WaterPatch();

	public OceanManager _oceanManager;

	private List<LinePair> _waterLinePoints = new List<LinePair>();

	private List<Vector3> _sortedWaterLinePoints = new List<Vector3>();

	private List<LinePair> _triangleCutLinePoints = new List<LinePair>();
	private List<SubmergedTriangle> _submergedTriangleList = new List<SubmergedTriangle>();

	private Rigidbody _rigidBody;

	private Vector3 _commonCenterOfApplication = Vector3.zero;

	private Vector3 _totalForceVector = Vector3.zero;

	private int[] _sortedTriangleArray = new int[3];

	private float _totalSubmergedArea = 0.0f;

	private float _forcePreFactor = 1.0f;	// - rho * gravity

	private List<Vector3> _waterGrid = new List<Vector3>();

	private float _initialArea = 0.0f;

	private Transform _motorLocator;

	private Vector3 _motorLocatorEndPoint = new Vector3();

	private float _boatAcceleration = 0.0f;

	private float _boatVelocity  = 0.0f;
	private float _motorLocatorEndPointAngle = 0.0f;

	private Vector3 _motorForceVector = new Vector3();

	private BVertex _H = new BVertex(new Vector3());
	private	BVertex _M = new BVertex(new Vector3());
	private	BVertex _L = new BVertex(new Vector3());

	// Use this for initialization
	void Start () {
		_rigidBody = this.GetComponent<Rigidbody>();
	
		Mesh boatMesh = this.GetComponent<MeshFilter>().mesh;
		_boatMesh = this.GetComponent<MeshFilter>().mesh;

		// length of diagonal of boat mesh bounding box
		float boatMeshDiagLength = (_boatMesh.bounds.max - _boatMesh.bounds.min).magnitude;

		_sentinelSphere =  GameObject.Find("SentinelSphere");

		for (int i = 0; i < boatMesh.triangles.Length; i += 3)
		{
    		Vector3 p1 = boatMesh.vertices[boatMesh.triangles[i + 0]];
    		Vector3 p2 = boatMesh.vertices[boatMesh.triangles[i + 1]];
    		Vector3 p3 = boatMesh.vertices[boatMesh.triangles[i + 2]];

			_initialArea += MathUtil.calcTriangleArea(p1, p2, p3);

			_meshTriangleList.Add(new MeshTriangle(new Triangle(p1, p2, p3), calculateNormal(p1, p2, p3)));	
		}
		_boatMeshCenter = _boatMesh.bounds.center;

		_waterPatch.init(getCenterWorldPosition(), WaterpatchScaleFactor * boatMeshDiagLength, SimSettings.WaterpatchDimRows);
		_waterPatch.build();
		
		_forcePreFactor = -(SimSettings.Rho * Physics.gravity.y);

		_motorLocator = transform.Find("MotorLocator");
		if(_motorLocator != null) {
			_motorLocatorEndPointAngle = Mathf.Deg2Rad * 270.0f;
			float radius = 3.0f;
			_motorLocatorEndPoint = new Vector3((radius)  *  Mathf.Cos(_motorLocatorEndPointAngle) + _motorLocator.position.x, 
				_motorLocator.transform.position.y, (radius) * Mathf.Sin(_motorLocatorEndPointAngle) + _motorLocator.transform.position.z );
		}
	}
	
	// Update is called once per frame
	void Update () {
		float start = Time.realtimeSinceStartup;
		//_debugMsg = "BoatController - update: " + string.Format(".3f%", Time.realtimeSinceStartup - start);
		_debugMsg = "BoatController - update: " + (Time.realtimeSinceStartup - start);

		if( Input.GetKeyDown(KeyCode.D))
		{
	//		_waterPatch.incrementNumTiles();
		} else if (Input.GetKeyDown(KeyCode.A)){
	//		_waterPatch.decrementNumTiles();
		}
	
		/*if( Input.GetKeyDown(KeyCode.W))
		{
			_waterPatchCellWidth = Mathf.Min(_waterPatchCellWidth + waterPatchIncrement, 20.0f);	
			buildWaterpatch();
		} else if(Input.GetKeyDown(KeyCode.S)) {
			buildWaterpatch();
			_waterPatchCellWidth = Mathf.Max(_waterPatchCellWidth - waterPatchIncrement, 1.0f);	
		}*/
	}


	public void FixedUpdate() {
		float start = Time.realtimeSinceStartup;

		_waterPatch.sampleOcean(getCenterWorldPosition(), _oceanManager);
		_waterLinePoints.Clear();
		_sortedWaterLinePoints.Clear();
		_submergedTriangleList.Clear();
		_triangleCutLinePoints.Clear();
		
		calcBuoyancy();
		buildWaterLineMesh();

		_totalSubmergedArea = 0.0f;
		_totalForceVector = Vector3.zero;
		_commonCenterOfApplication = Vector3.zero;

		//Vector3 pointVel = _rigidBody.GetPointVelocity(pos);

		//foreach(SubmergedTriangle tri in _submergedTriangleList) {
		for(int i = 0; i < _submergedTriangleList.Count; i++) {	
			SubmergedTriangle tri = _submergedTriangleList[i];
			_totalSubmergedArea += tri.Area;
			//Vector3 force = checkForceIsValid(_forceFactor * (tri._depth) * tri._area * tri._normal.normalized, "buoyancy force"); 
			
			if(tri.Area < 0.00001f || float.IsNaN(tri.Area) ) {
			//	Debug.Log("Calculating Force: Area invalid: " + tri.Area);
				continue;
			}
		
		
			// calc hydrostatic force
			if( SimSettings.ApplyHydrostaticForce) {
					
				Vector3 force = _forcePreFactor * (tri.Center.Depth) * tri.Area * tri.Normal  * SimSettings.DensityCorrectionModifier; 
				tri.Force = force.normalized;				

				if( Mathf.Approximately(force.magnitude, 0.0f) && isForceValid(force, "buoyancy force")) {
					debugLog("Hydrostatic force is invalid: " + force);
				}
				if(SimSettings.UseVerticalForceOnly) { 
					force.x = 0.0f;
					force.z = 0.0f;
				}
				if( !Mathf.Approximately(force.magnitude, 0.0f) && isForceValid(force, "buoyancy force")) {
					_rigidBody.AddForceAtPosition(force, tri.ForceCenter);	
				//	Debug.Log("Hydrostatic force: " + force);
				}
				_totalForceVector += force;
			}

			
			// calc velocity
			//Vector3 centerOfMassVel = _rigidBody.velocity;
			//Vector3 centerOfMassAngularVel = _rigidBody.angularVelocity;
			//Vector3 centerToCog = tri.Center.Position - _rigidBody.centerOfMass;
			//Vector3 velocityDir = (centerOfMassVel + Vector3.Cross(centerOfMassAngularVel, centerToCog)).normalized;
			Vector3 triCenterVelocity = _rigidBody.GetPointVelocity(tri.Center.Position);
			Vector3 tangentialVelocity = triCenterVelocity - (Vector3.Dot( triCenterVelocity , tri.Normal)) * tri.Normal;
			tri.Velocity = tangentialVelocity;

			Vector3 tempForce = Vector3.zero;
			
			if(SimSettings.ApplyViscousForce) 
			{
				Vector3 viscousForce = -tangentialVelocity *  tri.Area * SimSettings.ViscousDragCoefficient * Mathf.Max(51.4444f - tangentialVelocity.magnitude, 0.0f);
				//Debug.Log("viscousForce: " + velocityDir + " " + tri.Area + "  " +  SimSettings.ViscousDragCoefficient + " -> " + viscousForce);
				tempForce += viscousForce;
			}

			if(SimSettings.ApplyDragForce) {
				float refV = 500.0f;
				Vector3 dragForce = Vector3.zero;
				
				float projectedVel = Vector3.Dot(triCenterVelocity.normalized, tri.Normal);
				float speedFactor = triCenterVelocity.magnitude / refV;
				
				
				if(projectedVel >= 0) {
					// push drag
					dragForce = -tri.Area * Mathf.Pow(Mathf.Abs(projectedVel), SimSettings.DragCoefficient.y) * tri.Normal
					  * (SimSettings.DragCoefficient.x * speedFactor + SimSettings.DragCoefficient.z * Mathf.Pow(speedFactor, 2.0f));
				}
				else {
					// pull drag
					dragForce = tri.Area * Mathf.Pow(Mathf.Abs(projectedVel), SimSettings.SuctionCoefficient.y) * tri.Normal
						* (SimSettings.SuctionCoefficient.x * speedFactor + SimSettings.SuctionCoefficient.z * Mathf.Pow(speedFactor, 2.0f));
				}
				
				tempForce += dragForce;
			}

			if(SimSettings.ApplySlamForce) {
				Vector3 slamForce = Vector3.zero;

				tempForce += slamForce;
			}

			if( !Mathf.Approximately(tempForce.magnitude, 0.0f) && isForceValid(tempForce, "Incr. force")) {
				//	Debug.Log("tempForce: " + tempForce);
					_rigidBody.AddForceAtPosition(tempForce, tri.Center.Position);			
			}

			_commonCenterOfApplication += tri.ForceCenter;
		}

		_commonCenterOfApplication /= _submergedTriangleList.Count;

		//	float rotSpeed = 3f;
		//_motorLocator.transform.Rotate(0, -Input.GetAxis("Horizontal") * rotSpeed * Time.deltaTime, 0);
		//	Debug.Log(transform.name + " loc: " + _motorLocator);
		if(transform.name.Equals("cube1")   && _motorLocator != null) {
				//_motorLocatorEndPoint
				float radius = 3.0f;
				_motorLocatorEndPointAngle += Input.GetAxis("Horizontal") * BoatSet.RotationSpeed * Time.deltaTime *  Mathf.Deg2Rad;
				_motorLocatorEndPoint = new Vector3((radius)  *  Mathf.Cos(_motorLocatorEndPointAngle) + _motorLocator.position.x, 
					_motorLocator.transform.position.y, (radius) * Mathf.Sin(_motorLocatorEndPointAngle) + _motorLocator.transform.position.z );
				Vector3 forceDir = (_motorLocator.transform.position - _motorLocatorEndPoint).normalized;
	
				float accelerationInput = Input.GetAxis ("Accelerate");
				if(accelerationInput > 0) {
				//	Debug.Log("acc:  " + accelerationInput);
					_rigidBody.AddForceAtPosition(-forceDir * BoatSet.Velocity, _motorLocator.transform.position);
					BoatSet.Velocity = Mathf.Min(BoatSet.Velocity + BoatSet.Acceleration, BoatSet.TopSpeed);
				}
				else 
				{
					BoatSet.Velocity = 0.0f;
				}
			}

		_debugMsgFixedUpdate = "FixedUpdate: " + (Time.realtimeSinceStartup - start);
				
		if( _totalSubmergedArea > _initialArea) {
			float areaDiff = _totalSubmergedArea - _initialArea;
			if(areaDiff > 1e-3) {
				debugLog("Submerged area must be smaller than total area: submerged: " +  _totalSubmergedArea + " initial: " + _initialArea);
			}
		}
	}

	private void calcBuoyancy() {
			
		Transform t = this.transform;
				
		for(int i = 0; i < _meshTriangleList.Count; i++)
	   	{
			MeshTriangle mt = _meshTriangleList[i];
			Vector3 v1_transformed = t.TransformPoint( mt.triangle.Vertex1);
			Vector3 v2_transformed = t.TransformPoint( mt.triangle.Vertex2);
			Vector3 v3_transformed = t.TransformPoint( mt.triangle.Vertex3);
			
			float distanceToWater1 = getDistanceToWaterpatch(v1_transformed);
			float distanceToWater2 = getDistanceToWaterpatch(v2_transformed);
			float distanceToWater3 = getDistanceToWaterpatch(v3_transformed);

			int count = 0;
			if( distanceToWater1 <= 0 )
			{
				count += 1;
			}

			if( distanceToWater2 <= 0 )
			{
				count += 1;
			}

			if( distanceToWater3 <= 0 )
			{
				count += 1;
			}
		
			Vector3 normal = (t.TransformPoint((mt.triangle.Centroid + 0.1f * mt.normal))  - t.TransformPoint( mt.triangle.Centroid )).normalized;
			switch( count)
			{
				// totally submerged triangles
				case 3:
					cutTriangle(v1_transformed, v2_transformed, v3_transformed, normal);
					break;
				// partially submerged triangles - need to be cut
				case 2:
				case 1:
					calcWaterLine(v1_transformed, v2_transformed, v3_transformed, normal);
					break;
				case 0:
				default:
					break;
			}
		}
	}

	private void calcWaterLine(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 origNormal) {
	//	BVertex H = new BVertex(v1);
	//	BVertex M = new BVertex(v2);
	//	BVertex L = new BVertex(v3);

		_H.Position = v1;
		_M.Position = v2;
		_L.Position = v3;

		sortVertices(ref _H, ref _M, ref _L);

		if(_L.Depth < _M.Depth) {
 			debugLog("calcWaterLine: hL > hM!");
		}
		if(_M.Depth < _H.Depth) {
			debugLog("calcWaterLine: hM > hH!");
		}
	
		// no vertices under water
		if(_L.Depth > 0) return;

		// case 1:
		//if( L.Depth <= 0 && M.Depth <= 0 && H.Depth >= 0) 
		if( _M.Depth < 0 && _H.Depth > 0) 
		{
			float tm = -_M.Depth / (_H.Depth - _M.Depth);
			float tl = -_L.Depth / (_H.Depth - _L.Depth);

			Vector3 MH = _H.Position - _M.Position;
			Vector3 LH = _H.Position - _L.Position;

			Vector3 IM = _M.Position + tm * MH;
			Vector3 IL = _L.Position + tl * LH;

			Vector3 normal1 = calculateNormal(IM, _M.Position, _L.Position);
			if(! (origNormal == normal1))
			{
				normal1 *= -1;
			}

			cutTriangle(IM, _M.Position, _L.Position, normal1);
			
			Vector3 normal2 = calculateNormal(IL, IM, _L.Position);
			if(! (origNormal == normal2))
			{
				normal2 *= -1;
			}
			
			cutTriangle(IL, IM, _L.Position, normal2);

			_waterLinePoints.Add(new LinePair(IM, IL));
		}
		// case 2:
		//if( L.Depth < 0 &&  M.Depth > 0 && H.Depth >= 0) {
		if( _L.Depth < 0 &&  _M.Depth > 0) {
			float tm = -_L.Depth / (_M.Depth - _L.Depth);
			float tl = -_L.Depth / (_H.Depth - _L.Depth);

			Vector3 LM = _M.Position - _L.Position;
			Vector3 LH = _H.Position - _L.Position;

			Vector3 IM = _L.Position + tm * LM;
			Vector3 IH = _L.Position + tl * LH;

			Vector3 normal3 = calculateNormal(IM, IH, _L.Position);
			if(! (origNormal == normal3))
			{
				normal3 *= -1;
			}

			cutTriangle(IH, IM, _L.Position, normal3);
		
			_waterLinePoints.Add(new LinePair(IM, IH));
		}
	}

	private void cutTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal) {
		BVertex H = new BVertex(v1);
		BVertex M = new BVertex(v2);
		BVertex L = new BVertex(v3);
	
		if(DebugSet.RefineTriangles) {
			BVertex temp = new BVertex(new Vector3());
			if(M.Position.y > H.Position.y)
			{
				temp = H;
				H = M;
				M = temp;
			}
			if (L.Position.y > H.Position.y ){
				temp = H;
				H = L;
				L = temp;
			}
			if(L.Position.y > M.Position.y) 
			{
				temp = M;
				M = L;
				L = temp;
			}

			float factor = H.Position.y - L.Position.y;
			float factor2 = H.Position.y - M.Position.y;
			if(Mathf.Approximately(factor, 0)) {
				debugLog(" factor1 == 0: " + H.Position.y + " - " + L.Position.y + " -> " + factor/ factor2);
			}

			if(Mathf.Approximately(factor2, 0)) {
				debugLog(" factor2 == 0: " + H.Position.y + " - " + M.Position.y + " -> " + factor/ factor2);
			}
			Vector3 pos = H.Position + (L.Position - H.Position) * ((H.Position.y - M.Position.y) / (H.Position.y - L.Position.y));
		
			float depth = getDistanceToWaterpatch(pos);
			BVertex cutVertex = new BVertex(pos, depth);

			LinePair lp = new LinePair(M.Position, cutVertex.Position);

			//if(Mathf.Approximately(H.Position.y - M.Position.y, 0 )) {
			float parametrization = (H.Position.y - M.Position.y) / (H.Position.y - L.Position.y);
			if(parametrization > 1 || parametrization < 0)
			{
				debugLog(" parametrization in triangle cutting out of raange " + parametrization);
				lp.marked = true;
			}
			
			_triangleCutLinePoints.Add(lp);
			// horizontal pointing up 
			SubmergedTriangle tri1 = new SubmergedTriangle(L, cutVertex, M, normal);
			Vector3 medianPoint = (cutVertex.Position + M.Position) / 2;
			Vector3 medianLine = L.Position - medianPoint;
			float height = Mathf.Abs(L.Position.y - medianPoint.y);
			tri1.ForceCenter = medianPoint + medianLine * (2 * Mathf.Abs(medianPoint.y) + height) / (6 * Mathf.Abs(medianPoint.y) + 2 * height);
			tri1.Center.Depth = getDistanceToWaterpatch(tri1.Center.Position);
		
			_submergedTriangleList.Add(tri1);

			// horizontal pointing down
			SubmergedTriangle tri2 = new SubmergedTriangle(H, cutVertex, M, normal);
			medianPoint = (cutVertex.Position + M.Position) / 2;
			medianLine = medianPoint - H.Position;
			height = Mathf.Abs(H.Position.y - medianPoint.y);
			tri2.ForceCenter = H.Position + medianLine * (4 * Mathf.Abs(H.Depth) + 3 * height) / (6 * Mathf.Abs(H.Depth) + 4 * height);
			tri2.Center.Depth = getDistanceToWaterpatch(tri2.Center.Position);
	
			_submergedTriangleList.Add(tri2);
		}
		else 
		{
			//_submergedTriangleList.Add(new SubmergedTriangle(new BVertex(v1,getDistanceToWaterpatch(v1) ), 
			//	new BVertex(v2,getDistanceToWaterpatch(v2) ), new BVertex(v3,getDistanceToWaterpatch(v3) ), normal));
			SubmergedTriangle tri = new SubmergedTriangle(H, M, L, normal);
			tri.Center.Depth = getDistanceToWaterpatch(tri.Center.Position);
			tri.ForceCenter = tri.Center.Position;

			_submergedTriangleList.Add(tri);
		}
	}

	private void sortVertices(ref BVertex H, ref BVertex M, ref BVertex L) {
		BVertex temp = new BVertex(new Vector3());

		float hH = getDistanceToWaterpatch(H.Position);
		float hM = getDistanceToWaterpatch(M.Position);
		float hL = getDistanceToWaterpatch(L.Position);

		H.Depth = hH;
		M.Depth = hM;
		L.Depth = hL;

		if(M.Depth > H.Depth)
		{
			temp = H;
			H = M;
			M = temp;
		}
		if (L.Depth > H.Depth ){
			temp = H;
			H = L;
			L = temp;
		}
		if(L.Depth > M.Depth) 
		{
			temp = M;
			M = L;
			L = temp;
		}
	}

	private float getDistanceToWaterpatch(Vector3 point) {
		float distance = Mathf.Infinity;
		Vector2 cellIndices = _waterPatch.getCellIndicesForPoint(point);
		Vector2 cellCoordinates = _waterPatch.getCellCoordinatesForPoint(point);
		if(( cellIndices.x >= 0 && cellIndices.x < _waterPatch.NumTiles - 1) && cellIndices.y >= 0 && cellIndices.y < _waterPatch.NumTiles - 1)
		{
			int i = (int)cellIndices.x;
			int j = (int)cellIndices.y;
			float xpc = cellCoordinates.x;
			float zpc = cellCoordinates.y;
		
			Vector3 A = _waterPatch.get(i, j);
			Vector3 B = _waterPatch.get(i, j + 1);
			Vector3 C = _waterPatch.get(i + 1, j + 1); 	
			Vector3 D = _waterPatch.get(i + 1, j);

			Vector3 B1 = B;
			Gizmos.color = Color.green;
			if( xpc > zpc) {
				B1 = D;
			}		

			// calculate height
			Vector3 AB = B1 - A;
			Vector3 AC = C - A;
			Vector3 cross = Vector3.Cross(AB, AC);
			float d = Vector3.Dot(cross, A);
			distance = point.y - ((d - cross.x * point.x - cross.z * point.z) / cross.y);
		}
		return distance;
	}

	private void buildWaterLineMesh() {



		foreach(LinePair pair in _waterLinePoints) {
			Vector3 p = pair.p1;

		}
	}

	void OnDrawGizmos() {
		if(! DebugSet.ShowDebug)
		{
			return;
		}

		Transform t = this.transform;

		if(_motorLocator != null) {
			Gizmos.color  = Color.yellow;
			Gizmos.DrawLine(_motorLocator.transform.position,  _motorLocatorEndPoint);
			
			Gizmos.color  = ColorUtil.Cyan;
			Gizmos.DrawLine(_motorLocator.transform.position, _motorForceVector);
		}
	

		if(DebugSet.ShowNormalsOriginalMesh) {
			foreach(MeshTriangle mt in _meshTriangleList) 
			{
				Gizmos.color = Color.green;
				Vector3 A = t.TransformPoint(mt.triangle.Centroid + 0.1f * mt.normal);
				Vector3 E = t.TransformPoint(mt.triangle.Centroid + 0.2f * mt.normal);
				Gizmos.DrawLine(A,E);
			}
		}

		foreach(SubmergedTriangle tri in _submergedTriangleList) {
			
			if(DebugSet.ShowSubmergedVolume) {
				float factor = tri.Area / _totalSubmergedArea * 100;
				Color col = new Color(0.0f, factor, 0.0f, 1.0f);
			//	Debug.Log( "Tri.Area / TotalSubmergedArea" + tri.Area + " / " + _totalSubmergedArea + " " +  factor);
				//GLUtil.RenderTriangle(tri._triangle, DrawingUtil.LimeGreen * tri._area / _totalSubmergedArea);
				GLUtil.RenderTriangle(tri.A.Position, tri.B.Position, tri.C.Position, col);
			}

			if(DebugSet.ShowTriangleDetails) {
				Gizmos.color = Color.cyan;
				//Gizmos.DrawLine(tri.ForceCenter, tri.ForceCenter + (-1.0f *  tri.Force.magnitude / maxForce ) * tri.Force.normalized);
				Gizmos.DrawWireSphere(tri.ForceCenter, 0.01f);
				Gizmos.DrawLine(tri.ForceCenter, tri.ForceCenter + (-0.2f *  tri.Force.magnitude ) * tri.Force.normalized);
		//		Gizmos.DrawLine(tri.ForceCenter, tri.ForceCenter + (-1.0f *  tri._forceVector.magnitude / maxForce ) * tri._forceVector.normalized);
			//	maxForce = Mathf.Max(maxForce, tri._forceVector.magnitude);
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(tri.Center.Position, 0.005f);

				Gizmos.color = ColorUtil.Orange;
				Gizmos.DrawLine(tri.Center.Position, tri.Center.Position + 0.2f * tri.Velocity);

				Gizmos.color = Color.red;
				//Vector3 A = t.TransformPoint(tri._pointOfApplication);
				//Vector3 E = t.TransformPoint(tri._pointOfApplication + 0.1f * tri._normal);
				//Vector3 A = tri._pointOfApplication;
				//Vector3 E = tri._pointOfApplication + 0.1f * tri._normal;
				Gizmos.DrawLine(tri.Center.Position, tri.Center.Position + 0.1f * tri.Normal);

			}

			if(DebugSet.ShowTriangles) {
				DrawingUtil.DrawTriangle(tri.A.Position, tri.B.Position, tri.C.Position, Color.white);

				float offset = 0.01f;
				Gizmos.color = Color.blue;
				Gizmos.DrawSphere(tri.A.Position  - (tri.A.Position - tri.Center.Position).normalized * 3 * offset, offset);
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(tri.B.Position  - (tri.B.Position - tri.Center.Position).normalized * 3 * offset, offset);
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(tri.C.Position  - (tri.C.Position - tri.Center.Position).normalized * 3 * offset, offset);
			//	Gizmos.DrawSphere(tri.Center.Position, 0.005f);
			}

			if(DebugSet.ShowVertexHeight) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(tri.ForceCenter, new Vector3(tri.ForceCenter.x, tri.ForceCenter.y - getDistanceToWaterpatch(tri.ForceCenter), tri.ForceCenter.z ));

				Gizmos.color = Color.red;
				Gizmos.DrawLine(tri.Center.Position, new Vector3(tri.Center.Position.x, tri.Center.Position.y - tri.Center.Depth, tri.Center.Position.z ));

			//	Gizmos.DrawLine(tri.A.Position, new Vector3(tri.A.Position.x, tri.A.Position.y - tri.A.Depth, tri.A.Position.z ));
			//	Gizmos.DrawLine(tri.B.Position, new Vector3(tri.B.Position.x, tri.B.Position.y - tri.B.Depth, tri.B.Position.z ));
			//	Gizmos.DrawLine(tri.C.Position, new Vector3(tri.C.Position.x, tri.C.Position.y - tri.C.Depth, tri.C.Position.z ));
			}
		}
		
		if(DebugSet.ShowTotalForce) {
			Gizmos.color = ColorUtil.Cyan;
			Gizmos.DrawSphere(_commonCenterOfApplication, 0.25f);
			Gizmos.DrawLine(_commonCenterOfApplication, _commonCenterOfApplication + 1.0f * _totalForceVector);
		
		
			Gizmos.color = ColorUtil.Orange;
			Gizmos.DrawSphere(transform.position + transform.rotation * _rigidBody.centerOfMass, 0.25f);
		}

		if(DebugSet.ShowWaterLine) {
			foreach(LinePair pair in _waterLinePoints) {
				Gizmos.DrawLine(pair.p1, pair.p2);
			}
			Gizmos.color = ColorUtil.Orange;
			foreach(Vector3 p in _sortedWaterLinePoints) {
				Gizmos.DrawWireSphere(p, 0.025f);
			}
		}

		if(DebugSet.ShowTriangleCuts) {
			foreach(LinePair pair in _triangleCutLinePoints) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(pair.p1, pair.p2);
			
				if(pair.marked)  {
					Gizmos.color = pair.marked ?  Color.red : Color.green;
					Gizmos.DrawSphere(pair.p2, 0.025f);
				}
			}
		}
			
		if(DebugSet.ShowWaterPatch) {
			// draw waterPatch wire
			Gizmos.color = Color.blue;
			for( int i = 0; i < _waterPatch.NumTiles - 1; i++)
			{
				for( int j = 0; j < _waterPatch.NumTiles - 1; j++)
				{
					Vector3 A = _waterPatch.get(i, j);
					Vector3 B = _waterPatch.get(i + 1, j);
					Vector3 D = _waterPatch.get(i, j + 1);
					Vector3 C = _waterPatch.get(i + 1, j + 1);

					Gizmos.DrawLine(A,B);
					Gizmos.DrawLine(B,C);
					Gizmos.DrawLine(C,D);
					Gizmos.DrawLine(A,D);
					Gizmos.DrawLine(A,C);
				}
			}

			// test coordinates
			if( _sentinelSphere == null)
				return;	

			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(_waterPatch.zG + new Vector3(0,0,0.0f), 0.01f);
			Gizmos.DrawWireSphere(_sentinelSphere.transform.position + new Vector3(0,0.4f,0.0f), 0.01f);
	
			float distance = getDistanceToWaterpatch(_sentinelSphere.transform.position);	
			if(distance <= 0.0f)
			{
				Gizmos.DrawLine(_sentinelSphere.transform.position, _sentinelSphere.transform.position + new Vector3(0, (-1) * distance, 0));
			}
		}
	}

	private Vector3 getCenterWorldPosition() {
		return this.transform.TransformPoint(_boatMeshCenter);
	}

	private Vector3 calculateNormal(Triangle t) {
		return calculateNormal(t.Vertex1, t.Vertex2, t.Vertex3);
	}

	private Vector3 calculateNormal(Vector3 v1, Vector3 v2, Vector3 v3) {
		//Vector3 AB = 
		Vector3 AB = v2 - v1;
		Vector3 CB = v3 - v1;
		return Vector3.Cross(AB, CB).normalized;
	}

    private bool checkForce(Vector3 force)
    {
	    if (float.IsNaN(force.x + force.y + force.z) || force == Vector3.zero)
        {
            return false;
        }
       return true;
    }

	private bool isForceValid(Vector3 force, string forceName) {
		 if (float.IsNaN(force.x + force.y + force.z))
        {
            return false;
        }
		return true;
	}

	void OnGUI() {
 	//	GUI.Label (new Rect (0,0,100,50), _debugMsg);
		GUI.Label (new Rect (0,100,100,50), _debugMsgFixedUpdate);
	}

	private void debugLog(string msg) {
	  	if(DebugSet.PrintWarnings) Debug.Log(this.name + " -> " + msg);
	}
}
