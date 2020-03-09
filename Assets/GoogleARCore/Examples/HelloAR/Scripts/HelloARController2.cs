//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.EventSystems;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloARController2 : MonoBehaviour
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera firstPersonCamera;
		public Camera uiCamera;
		[SerializeField] LineRenderer line;
		//public UILabel ddd;

		/// <summary>
		/// A prefab to place when a raycast from a user touch hits a vertical plane.
		/// </summary>
		public GameObject goVerticalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a horizontal plane.
        /// </summary>
        public GameObject goHorizontalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a feature point.
        /// </summary>
        public GameObject goPointPrefab;

        /// <summary>
        /// The rotation in degrees need to apply to prefab when it is placed.
        /// </summary>
        private const float k_PrefabRotation = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

		//찍힌 점들....
		List<Transform> list = new List<Transform>();
		bool bCalculate, bLine;
		GameObject goNewTile;
		[SerializeField]Material material;
		/// <summary>
		/// The Unity Awake() method.
		/// </summary>
		public void Awake()
        {
            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;
			if(line == null)
			{
				line = GetComponent<LineRenderer>();
			}
        }



        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
			//if (Input.GetMouseButtonDown(0))
			//{
			//	Debug.Log("s:" + Time.realtimeSinceStartup);
			//	float x = 0;
			//	for (int i = 0; i < 100000000; i++)
			//	{
			//		x += 1;
			//	}
			//	Debug.Log("e:" + Time.realtimeSinceStartup);
			//}

			DoCreateMesh();

			//Debug.Log(UICamera.currentCamera);
            _UpdateApplicationLifecycle();

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }


            // Should not handle input if the player is pointing on UI.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)
				|| uiCamera == UICamera.currentCamera)
            {
				//Debug.Log(1);
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(firstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Choose the prefab based on the Trackable that got hit.
                    GameObject prefab;
                    if (hit.Trackable is FeaturePoint)
                    {
                        prefab = goPointPrefab;
                    }
                    else if (hit.Trackable is DetectedPlane)
                    {
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                        if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                        {
                            prefab = goVerticalPlanePrefab;
                        }
                        else
                        {
                            prefab = goHorizontalPlanePrefab;
                        }
                    }
                    else
                    {
                        prefab = goHorizontalPlanePrefab;
                    }

                    // Instantiate prefab at the hit pose.
                    var _go = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    _go.transform.Rotate(0, k_PrefabRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    _go.transform.SetParent(anchor.transform);

					//리스트에 생성된 오브젝트 나타내기...
					list.Add(_go.transform);
					bCalculate	= true;
					bLine		= true;
				}
			}

			//line 그려주기...
			if (bLine && list.Count >= 2)
			{
				bLine = false;
				int _count = list.Count;
				line.SetVertexCount(_count);
				line.SetWidth(0.01f, 0.01f);
				line.SetColors(Color.red, Color.yellow);
				line.useWorldSpace = true;
				for (int i = 0; i < _count; i++)
				{
					line.SetPosition(i, list[i].position);
				}
			}

		}

		bool bCreateMesh;
		public void CreateMeshing(bool _bCreateMesh)
		{
			bCreateMesh = _bCreateMesh;
		}

		void CalculateListObject()
		{
			if (goNewTile != null)
			{
				Destroy(goNewTile);
			}
			goNewTile				= new GameObject();
			MeshFilter _meshFilter	= goNewTile.AddComponent<MeshFilter>();
			MeshRenderer _meshRenderer= goNewTile.AddComponent<MeshRenderer>();
			Mesh _mesh				= new Mesh();

			_meshRenderer.material	= material;
			_mesh.name				= "TileMesh";
			_meshFilter.mesh		= _mesh;

			//리스트의 위치를 재조정 할 필요가 있을떄 재조정한다...
			if(list.Count == 4)
				Triangulator.ReCalcuatePosition(list);

			//3D -> 2D position recalculate
			Vector2[] _vertices2D = new Vector2[list.Count];
			Vector3 _pos;
			float _y = 0f;

			for (int i = 0; i < list.Count; i++)
			{
				_pos			= list[i].position;
				_vertices2D[i]	= new Vector2(_pos.x, _pos.z);
				_y += _pos.y;
			}
			_y /= list.Count;


			//triangle ...
			Triangulator _tr = new Triangulator(_vertices2D);
			int[] _triangles = _tr.Triangulate();

			//vertices
			Vector3[] _vertices = new Vector3[_vertices2D.Length];
			for (int i = 0; i < _vertices.Length; i++)
			{
				_vertices[i] = new Vector3(_vertices2D[i].x, _y, _vertices2D[i].y);
			}

			// Create the mesh
			_mesh.vertices	= _vertices;
			_mesh.triangles	= _triangles;
			_mesh.uv		= _tr.CalculateUV();
			_mesh.RecalculateNormals();
			_mesh.RecalculateBounds();

			//material texture, 
			material.mainTextureScale = _tr.CalculateScale(.2f);

			for (int i = 0; i < list.Count; i++)
				Destroy(list[i].gameObject);
			list.Clear();
			line.SetVertexCount(0);
		}

		//----------------------------------------
		public void SetMaterial(Texture _texture)
		{
			material.mainTexture = _texture;
		}

		void DoCreateMesh()
		{
			//ddd.text = bCalculate + ":" + list.Count + ":" + bCreateMesh;
			if (bCalculate && list.Count >= 3 && bCreateMesh)
			{
				bCreateMesh		= false;
				bCalculate		= false;
				CalculateListObject();
			}
		}

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
