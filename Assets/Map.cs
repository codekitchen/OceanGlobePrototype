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

    Vector3 offset = new Vector3(position.x - Mathf.Floor(position.x), 0, position.y - Mathf.Floor(position.y)) * -stride;
    for (int y = 0; y < pixels; ++y) {
      for (int x = 0; x < pixels; ++x) {
        var pixel = grid[y, x];
        var waveMotion = waves.WaveOffset(gridToWorld(new Vector2(x, y)));
        waveMotion = Vector3.zero;
        pixel.position = BasePos(x, y) + offset + waveMotion;
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

  Vector3 BasePos(int x, int y) {
    float yp = -(size / 2) + y * stride;
    float xp = -(size / 2) + x * stride;
    return new Vector3(xp, 0, yp);
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
    cam.position = new Vector3(0, size, 0);
    cam.LookAt(Vector3.zero);
  }
}