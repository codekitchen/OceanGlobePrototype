using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {
  public float size = 20;
  public int pixels = 20;

  public float moveSpeed = 6f;
  public Vector2 position = Vector2.zero;

  public WaveSystem waves;

  public Transform pixelPrefab;

  Transform gridObject;
  Transform[,] grid;

  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
  void Start() {
    CreateGrid();
    SetupCamera();
  }

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update() {
    position.x += Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
    position.y += Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;

    Vector3 offset = -(new Vector3(position.x - Mathf.Floor(position.x), 0, position.y - Mathf.Floor(position.y)));
    for (int y = 0; y < pixels; ++y) {
      for (int x = 0; x < pixels; ++x) {
        var pixel = grid[y, x];
        var waveMotion = waves.WaveOffset(gridToWorld(new Vector2(x, y)));
        waveMotion = Vector3.zero;
        pixel.position = BasePos(x + offset.x, y + offset.z) + waveMotion;
				if (gridToWorld(new Vector2(x, y)) == new Vector2(2, 1))
          pixel.gameObject.SetActive(false);
					else
          pixel.gameObject.SetActive(true);
      }
    }
  }

  Vector2 gridToWorld(Vector2 gridPos) {
    return new Vector2(
      gridPos.x + Mathf.Floor(position.x),
      gridPos.y + Mathf.Floor(position.y)
    );
  }

  float Perlin(float x, float y) {
    return Mathf.PerlinNoise(x * 0.05f, y * 0.005f);
  }

  float stride { get { return size / (float)(pixels - 1); } }

  float circlePadding = Mathf.PI / 6;

  Vector3 BasePos(float x, float y) {
    float xr = x / (float)(pixels - 2);
    float yr = y / (float)(pixels - 2);
    float lon = circlePadding + (Mathf.PI - 2 * circlePadding) * xr;
    float lat = circlePadding + (Mathf.PI - 2 * circlePadding) * yr - (Mathf.PI / 2);
    float radius = size / 2f;
    float xp = radius * Mathf.Cos(lat) * Mathf.Cos(lon + Mathf.PI);
    float yp = radius * Mathf.Cos(lat) * Mathf.Sin(lon);
    float zp = radius * Mathf.Sin(lat);
    return new Vector3(xp, yp, zp);
  }

  void CreateGrid() {
    if (gridObject)
      Destroy(gridObject.gameObject);
    gridObject = new GameObject("Grid").transform;
    gridObject.SetParent(transform);

    grid = new Transform[pixels, pixels];

    for (int y = 0; y < pixels; ++y) {
      for (int x = 0; x < pixels; ++x) {
        grid[y, x] = Instantiate(pixelPrefab, BasePos(x, y), Quaternion.identity, gridObject);
      }
    }
  }

  void SetupCamera() {
    var cam = Camera.main.transform;
    cam.position = new Vector3(0, size + 5, 0);
    cam.rotation = Quaternion.Euler(90, 0, 0);
  }
}