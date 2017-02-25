using UnityEngine;

public class Map : MonoBehaviour {
  public float allowedOffset = 2f;
  public float size = 20;
  public int pixels = 20;

  public float moveSpeed = 6f;
  public Vector2 position = Vector2.zero;

  public WaveSystem waves;

  public Transform pixelPrefab;

  Transform gridObject;
  Transform[] grid;
  Transform this[int x, int y] {
    get {
      return grid[y * pixels + x];
    }
    set {
      grid[y * pixels + x] = value;
    }
  }

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
  Vector2 intPos = new Vector2(Mathf.Round(mapPos.x), Mathf.Round(mapPos.y));
    Tile tile = tileType(intPos);
    return tile == Tile.Water;
  }

	public bool CanEnter(int x, int y) {
    return CanEnter(new Vector2(x, y));
  }

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update() {
    Vector3 offset = -(new Vector3(position.x - Mathf.Floor(position.x), position.y - Mathf.Floor(position.y), 0));
    for (int y = 0; y < pixels; ++y) {
      for (int x = 0; x < pixels; ++x) {
        var pixel = this[x, y];
        Vector3 tileOffset = offset;
        Tile tile = tileType(gridToMap(new Vector2(x, y)));
        pixel.GetChild(0).gameObject.SetActive(tile == Tile.Water);
        pixel.GetChild(1).gameObject.SetActive(tile == Tile.Ground);

        if (tile == Tile.Water)
          tileOffset += waves.WaveOffset(gridToMap(new Vector2(x, y)));
        else if (tile == Tile.Ground)
          tileOffset.z += .25f;

        Position(pixel, new Vector3(x, y, 0), tileOffset);

        if (gridToMap(new Vector2(x, y)) == Vector2.zero)
          pixel.gameObject.SetActive(false);
        else
          pixel.gameObject.SetActive(true);
      }
    }

    {
      Vector2 pos = mapToGrid(boat.mapPos);
      var bt = boat.transform;
      var boatOffset = offset + waves.WaveOffset(boat.mapPos);
      Position(bt, pos, boatOffset);
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
    if (sample < .55)
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

	void Position(Transform obj, Vector3 gridCords, Vector3 offset) {
    gridCords += offset;
    // we extend the range on the right/top a bit, to compensate for the fact that
    // the "pixels" disappear sooner on that side as they scroll.
    float xr = (gridCords.x + 1) / (float)(pixels);
    float yr = (gridCords.y + 1) / (float)(pixels);
    float lon = circlePadding + (Mathf.PI - 2 * circlePadding) * xr;
    float lat = circlePadding + (Mathf.PI - 2 * circlePadding) * yr - (Mathf.PI / 2);
    float radius = gridCords.z + size / 2f;
    float xp = radius * Mathf.Cos(lat) * Mathf.Cos(lon + Mathf.PI);
    float yp = radius * Mathf.Cos(lat) * Mathf.Sin(lon);
    float zp = radius * Mathf.Sin(lat);
    obj.position = new Vector3(xp, yp, zp);
		// eesh
		// This is to rotate the sprite so that "up" is directly out of the circle (perpendicular to the surface),
		// but "right" is always aligned with the positive x axis (but still tangent to the circle's surface), so
		// that non-round sprites aren't pointing in random directions.
		// my trig-fu is weak, this took like 30 minutes.
    var norm = obj.position.normalized;
    var localRight = Quaternion.Euler(0, 0, Mathf.Rad2Deg * (Mathf.PI / 2 - lon)) * Vector3.right + obj.position;
    obj.LookAt(localRight, obj.position + norm);
    obj.Rotate(0, 270, 0);
  }

  void CreateGrid() {
    if (gridObject)
      Destroy(gridObject.gameObject);
    gridObject = new GameObject("Grid").transform;
    gridObject.SetParent(transform);

    grid = new Transform[pixels * pixels];

    for (int y = 0; y < pixels; ++y) {
      for (int x = 0; x < pixels; ++x) {
        var pixel = Instantiate(pixelPrefab, Vector3.zero, Quaternion.identity, gridObject);
        Position(pixel, new Vector3(x, y, 0), Vector3.zero);
        this[x, y] = pixel;
      }
    }
  }

  void SetupCamera() {
    var cam = Camera.main.transform;
    cam.position = new Vector3(0, size + 5, 0);
    cam.rotation = Quaternion.Euler(90, 0, 0);
  }
}