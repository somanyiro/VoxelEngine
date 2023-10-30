using Raylib_cs;
using static Raylib_cs.Raylib;

public class World 
{
	public Chunk[,,] chunks;
	public string[,] biomeIndex;

	public int Width
	{
		get {return chunks.GetLength(0)*8;}
	}

	public int Height
	{
		get {return chunks.GetLength(1)*8;}
	}

	public int Depth
	{
		get {return chunks.GetLength(2)*8;}
	}

	public World(int width = 1, int height = 1, int depth = 1)
	{
		chunks = new Chunk[width, height, depth];
		biomeIndex = new string[width*8, depth*8];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				for (int z = 0; z < depth; z++) {
					chunks[x,y,z] = new Chunk();
				}
			}
		}

		Generate(1234);
	}

	public Voxel GetVoxel(int x, int y, int z)
	{
		if (x < 0 || x >= Width)
			return new Voxel();
		if (y < 0 || y >= Height)
			return new Voxel();
		if (z < 0 || z >= Depth)
			return new Voxel();

		return chunks[x/8,y/8,z/8].voxels[x%8,y%8,z%8];
	}

	public ref Voxel GetVoxelRef(int x, int y, int z)
	{
		if (x < 0 || x >= Width)
			return ref Voxel.Air;
		if (y < 0 || y >= Height)
			return ref Voxel.Air;
		if (z < 0 || z >= Depth)
			return ref Voxel.Air;

		return ref chunks[x/8,y/8,z/8].voxels[x%8,y%8,z%8];
	}

	public void SetVoxel(int x, int y, int z, Voxel.Types type)
	{
		if (x < 0 || x >= Width)
			return;
		if (y < 0 || y >= Height)
			return;
		if (z < 0 || z >= Depth)
			return;

		int chunkX = x / 8;
		int chunkY = y / 8;
		int chunkZ = z / 8;

		chunks[chunkX, chunkY, chunkZ].voxels[x%8, y%8, z%8].SetType(type);

		if (chunks[chunkX, chunkY, chunkZ].empty && type != Voxel.Types.Air)
			chunks[chunkX, chunkY, chunkZ].empty = false;
	}

	public void Generate(int seed = 1)
	{
		float[,] biomeNoise = Utils.GetNoiseData(seed, Width, Depth, 2, 1);

		Biome[] biomes = {
			new Plains(seed+100, Width, Depth),
			new Mountains(seed+200, Width, Depth),
			new Sea(seed+300, Width, Depth)
		};

		//height generaiton
		for (int x = 0; x < Width; x++) {
			for (int z = 0; z < Depth; z++) {
				
				float biomeSelector = biomeNoise[x,z];

				biomes = biomes.OrderBy(a => MathF.Abs(biomeSelector - a.selectorPoint)).ToArray();

				biomeIndex[x,z] = biomes[0].GetType().ToString();

				float contribution = Utils.Map(
					MathF.Abs(biomeSelector - biomes[0].selectorPoint),
					0, MathF.Abs(biomeSelector - biomes[1].selectorPoint)*4,//I wish I knew why this is here, lerp and cubic lerp need it
					0, 1);

				float heightValue = Utils.CubicLerp(biomes[0].heightNoise[x,z], biomes[1].heightNoise[x,z], contribution);
				float treshold = Height/2 + heightValue;

				for (int y = 0; y < Height; y++) {

					if (y > treshold)
					{
						SetVoxel(x,y,z, Voxel.Types.Air);
						continue;
					}

					if (y < treshold - 2)
					{
						SetVoxel(x,y,z, Voxel.Types.Stone);
						continue;
					}

					SetVoxel(x,y,z, biomes[0].voxelType);
				}
			}
		}

		//water
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				for (int z = 0; z < Depth; z++) {
					if (GetVoxel(x, y, z).type == Voxel.Types.Air && y < Height/2)
						SetVoxel(x, y, z, Voxel.Types.Water);
				}
			}
		}

		//biome custom passes
		foreach (Biome biome in biomes) 
		{
			biome.CustomPass(this);
		}

		//updating visible faces and light values
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				for (int z = 0; z < Depth; z++) {

					ref Voxel voxelRef = ref GetVoxelRef(x,y,z);

					if (voxelRef.type == Voxel.Types.Air) continue;

					voxelRef.visibleFaces = new bool[] {
						Voxel.propertiesOf[GetVoxel(x,y,z+1).type].transparent && GetVoxel(x,y,z+1).type != voxelRef.type,
						Voxel.propertiesOf[GetVoxel(x,y,z-1).type].transparent && GetVoxel(x,y,z-1).type != voxelRef.type,
						Voxel.propertiesOf[GetVoxel(x,y+1,z).type].transparent && GetVoxel(x,y+1,z).type != voxelRef.type,
						Voxel.propertiesOf[GetVoxel(x,y-1,z).type].transparent && GetVoxel(x,y-1,z).type != voxelRef.type,
						Voxel.propertiesOf[GetVoxel(x+1,y,z).type].transparent && GetVoxel(x+1,y,z).type != voxelRef.type,
						Voxel.propertiesOf[GetVoxel(x-1,y,z).type].transparent && GetVoxel(x-1,y,z).type != voxelRef.type
					};

					voxelRef.visible = voxelRef.AnyFaceVisible();

					voxelRef.light = 255/Height*y;
					
				}
			}
		}

		//WIP
		//merge chunks into meshes
		/*
		for (int x = 0; x < chunks.GetLength(0); x++) {
			for (int y = 0; y < chunks.GetLength(1); y++) {
				for (int z = 0; z < chunks.GetLength(2); z++) {
					
					int visibleVoxels = 0;

					for (int i = 0; i < 8; i++) {
						for (int j = 0; j < 8; j++) {
							for (int k = 0; k < 8; k++) {
								if (chunks[x,y,z].voxels[i,j,k].visible) visibleVoxels++;
							}
						}
					}


				}
			}
		}
		*/

	}
}