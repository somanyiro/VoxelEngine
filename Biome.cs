
public class Biome
{
	public float[,] heightNoise;
	public Voxel.Types voxelType;
	public float selectorPoint;

	public Biome(float selectorPoint, int seed, int worldWidth, int worldDepth, float noiseSize, float noisePower, Voxel.Types voxelType)
	{
		heightNoise = Utils.GetNoiseData(seed, worldWidth, worldDepth, noiseSize, noisePower);
		this.voxelType = voxelType;
		this.selectorPoint = selectorPoint;
	}

	public virtual void CustomPass(World world)
	{

	}
}

public class Plains : Biome
{
	public Plains(int seed, int worldWidth, int worldDepth) : base(0.5f, seed, worldWidth, worldDepth, 10, 5, Voxel.Types.Dirt)
	{

	}

	public override void CustomPass(World world)
	{
		Random random = new Random();

		for (int x = 0; x < world.Width; x++) {
			for (int z = 0; z < world.Depth; z++) {
				if (world.biomeIndex[x,z] != this.GetType().ToString()) continue;

				for (int y = 0; y < world.Height; y++) {
					if (world.GetVoxel(x,y,z).type == Voxel.Types.Air &&
						world.GetVoxel(x,y-1,z).type == Voxel.Types.Dirt &&
						random.Next(10) < 3)
					{
						world.SetVoxel(x,y,z, Voxel.Types.Grass);
					}
				}
			}
		}
	}
}

public class Mountains : Biome
{
	public Mountains(int seed, int worldWidth, int worldDepth) : base(0.8f, seed, worldWidth, worldDepth, 12, 20, Voxel.Types.Stone)
	{
		
	}

	public override void CustomPass(World world)
	{
		int snowHeight = world.Height/2 + 12;

		for (int x = 0; x < world.Width; x++) {
			for (int z = 0; z < world.Depth; z++) {
				if (world.biomeIndex[x,z] != this.GetType().ToString()) continue;

				for (int y = 0; y < world.Height; y++) {
					if (y > snowHeight &&
						world.GetVoxel(x,y,z).type == Voxel.Types.Stone &&
						world.GetVoxel(x,y+1,z).type == Voxel.Types.Air)
					{
						world.SetVoxel(x,y,z, Voxel.Types.Snow);
					}
				}
			}
		}

	}
}

public class Sea : Biome
{
	public Sea(int seed, int worldWidth, int worldDepth) : base(0.2f, seed, worldWidth, worldDepth, 5, -10, Voxel.Types.Sand)
	{
		
	}

	public override void CustomPass(World world)
	{
		
	}
}