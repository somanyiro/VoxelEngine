using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace VoxelEngine;

class Program
{
    public static void Main()
    {
        VoxelWorld scene = new VoxelWorld();
        scene.Run();
    }
}