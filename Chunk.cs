using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace VoxelEngine;

public class Chunk 
{
	public Vector3 position = Vector3.Zero;

	public Voxel[,,] voxels = new Voxel[8,8,8];
	public bool empty = true;

	public Model model = new Model();

	public Chunk()
	{
		for (int x = 0; x < 8; x++) {
			for (int y = 0; y < 8; y++) {
				for (int z = 0; z < 8; z++) {
					voxels[x,y,z] = new Voxel();
				}
			}
		}
	}
}