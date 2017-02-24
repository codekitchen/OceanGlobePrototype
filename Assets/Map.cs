﻿using UnityEngine;

public class Map : MonoBehaviour {
  public float allowedOffset = 2f;
  public float size = 20;
  public int pixels = 20;

  public float moveSpeed = 6f;
  public Vector2 position = Vector2.zero;

  public WaveSystem waves;

  public Transform pixelPrefab;

  Transform gridObject;
  Transform[,] grid;

  public PlayerControls boat;

  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
  void Start() {
    CreateGrid();
    SetupCamera();
  }

  public bool CanEnter(Vector2 mapPos) {
    Vector2 intPos = new Vector2(Mathf.Floor(mapPos.x), Mathf.Floor(mapPos.y));
    Tile tile = tileType(mapPos);
    return tile == Tile.Water;
  }

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update() {
    Vector3 offset = -(new Vector3(position.x - Mathf.Floor(position.x), 0, position.y - Mathf.Floor(position.y)));
    for (int y = 0; y < pixels; ++y) {
      for (int x = 0; x < pixels; ++x) {
        var pixel = grid[y, x];
        Vector3 waveMotion = Vector3.zero;
        Tile tile = tileType(gridToMap(new Vector2(x, y)));
        pixel.GetChild(0).gameObject.SetActive(tile == Tile.Water);
        pixel.GetChild(1).gameObject.SetActive(tile == Tile.Ground);

        if (tile == Tile.Water)
          waveMotion = waves.WaveOffset(gridToMap(new Vector2(x, y)));

        pixel.position = BasePos(x + offset.x + waveMotion.x, y + offset.z + waveMotion.z, waveMotion.y);
        pixel.LookAt(pixel.position * 2, Vector3.right);
        pixel.Rotate(Vector3.right, 90);
      }
    }

    {
      Vector3 waveMotion = waves.WaveOffset(boat.mapPos);
      Vector2 pos = mapToGrid(boat.mapPos);
      var bt = boat.transform;
      bt.position = BasePos(pos.x + offset.x + waveMotion.x, pos.y + offset.z + waveMotion.z, waveMotion.y);
      bt.LookAt(bt.position * 2, Vector3.right);
      bt.Rotate(Vector3.right, 90);
      if (boat.mapPos.x - position.x > allowedOffset) {
        position.x = boat.mapPos.x - allowedOffset;
      }
      else if (boat.mapPos.x - position.x < -allowedOffset) {
        position.x = boat.mapPos.x + allowedOffset;
      }
      if (boat.mapPos.y - position.y > allowedOffset) {
        position.y = boat.mapPos.y - allowedOffset;
      }
      else if (boat.mapPos.y - position.y < -allowedOffset) {
        position.y = boat.mapPos.y + allowedOffset;
      }
    }
  }

  public enum Tile {
    Water, Ground,
  }

  float xOrg = 1024;
  float yOrg = 5412;
  int worldWidth = 1024;
  int worldHeight = 1024;
  float scale = 20f;
  int octaves = 5;
  float persistence = 2f;

  Tile tileType(Vector2 worldPos) {
    float xc = xOrg + worldPos.x / (float)worldWidth * scale;
    float yc = yOrg + worldPos.y / (float)worldHeight * scale;
    float sample = OctavePerlin(xc, yc);
    if (sample < .65)
      return Tile.Water;
    return Tile.Ground;
  }

  float OctavePerlin(float x, float y) {
    float total = 0f;
    float maxValue = 0f;
    float frequency = 1f;
    float amplitude = 1f;

    for (int i = 0; i < octaves; ++i) {
      total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
      maxValue += amplitude;
      amplitude *= persistence;
      frequency *= 2;
    }

    return total / maxValue;
  }

  Vector2 gridToMap(Vector2 gridPos) {
    int halfSize = pixels / 2;
    return new Vector2(
      gridPos.x + Mathf.Floor(position.x) - halfSize + 1,
      gridPos.y + Mathf.Floor(position.y) - halfSize + 1
    );
  }

  Vector2 mapToGrid(Vector2 mapPos) {
    int halfSize = pixels / 2;
    return new Vector2(
      mapPos.x - Mathf.Floor(position.x) + halfSize - 1,
      mapPos.y - Mathf.Floor(position.y) + halfSize - 1
    );
  }

  float Perlin(float x, float y) {
    return Mathf.PerlinNoise(x * 0.05f, y * 0.005f);
  }

  float stride { get { return size / (float)(pixels - 1); } }

  float circlePadding = Mathf.PI / 7;

  Vector3 BasePos(float x, float y, float z = 0f) {
    // we extend the range on the right/top a bit, to compensate for the fact that
    // the "pixels" disappear sooner on that side as they scroll.
    float xr = (x + 1) / (float)(pixels);
    float yr = (y + 1) / (float)(pixels);
    float lon = circlePadding + (Mathf.PI - 2 * circlePadding) * xr;
    float lat = circlePadding + (Mathf.PI - 2 * circlePadding) * yr - (Mathf.PI / 2);
    float radius = z + size / 2f;
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
        var pos = BasePos(x, y);
        var pixel = Instantiate(pixelPrefab, pos, Quaternion.identity, gridObject);
        grid[y, x] = pixel;
      }
    }
  }

  void SetupCamera() {
    var cam = Camera.main.transform;
    cam.position = new Vector3(0, size + 5, 0);
    cam.rotation = Quaternion.Euler(90, 0, 0);
  }
}