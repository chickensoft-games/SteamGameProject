namespace SteamGameProject;

using Godot;
#if DEBUG
using System.Reflection;
using GoDotTest;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Runtime.CompilerServices;
#endif

public partial class Main : Node2D {
  [MethodImpl(MethodImplOptions.NoInlining)]
  public Main() {
    NativeLibrary.Load(
      Path.Join(AppContext.BaseDirectory, "libsteam_api.dylib")
    );
    AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
  }

  private Assembly? ResolveAssemblies(object? sender, ResolveEventArgs args) {
    // Never gets called :'(
    var assemblyName = new AssemblyName(args.Name);
    var assemblyPath = Path.Join(
      AppContext.BaseDirectory,
      assemblyName.Name + ".dll"
    );
    return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
  }

  public TestEnvironment Environment = default!;

  public override void _Ready() {
#if DEBUG
    // Let GoDotTest run tests if it thinks we should. It checks for the
    // correct command line arguments and constructs a test environment.
    Environment = TestEnvironment.From(OS.GetCmdlineArgs());
    if (Environment.ShouldRunTests) {
      CallDeferred("RunTests");
      return;
    }
#endif
    // If we don't need to run tests, we can just switch to the game scene.
    GetTree().ChangeSceneToFile("res://src/Game.tscn");
  }

  private void RunTests()
    => GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
}
