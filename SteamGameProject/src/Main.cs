namespace SteamGameProject;

using Godot;
#if DEBUG
using GoDotTest;
#endif
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Runtime.CompilerServices;

public partial class Main : Node2D {
  [MethodImpl(MethodImplOptions.NoInlining)]
  public Main() {
    NativeLibrary.Load(
      Path.Join(AppContext.BaseDirectory, "libsteam_api.dylib")
    );
    var me = Assembly.GetExecutingAssembly();
    var references = me.GetReferencedAssemblies();
    var steamworksDotNet = Array.Find(
      references, a => a.Name == "Steamworks.NET"
    )!;
    AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
    var steamworksDotNetAssembly = Assembly.Load(steamworksDotNet);
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
