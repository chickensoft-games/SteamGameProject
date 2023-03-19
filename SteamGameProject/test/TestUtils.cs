namespace SteamGameProject;

using System;
using System.IO;
using System.Reflection;
using Godot;

public static class TestUtils {
  /// <summary>
  /// Loads and instantiates the scene that corresponds to the given script
  /// type.
  /// </summary>
  /// <typeparam name="T">Script type attached to the scene.</typeparam>
  /// <returns>Instantiated scene.</returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static T LoadScene<T>() where T : Node {
    var attr = typeof(T).GetCustomAttribute<ScriptPathAttribute>()
      ?? throw new InvalidOperationException(
        $"Type '{typeof(T)}' does not have a ScriptPathAttribute"
      );
    var path = Path.ChangeExtension(attr.Path, ".tscn");
    return ResourceLoader.Load<PackedScene>(path).Instantiate<T>();
  }
}
