using System.Collections.Generic;

namespace VoxelEngine;

public struct Voxel
{
	public enum Types
	{
		Air,
		Dirt,
		Stone,
		Snow,
		Sand,
		Water,
		Grass
	}

	public static Dictionary<Types, VoxelData> propertiesOf = new Dictionary<Types, VoxelData>()
	{
		{Types.Air, new VoxelData(0, 0, 0, 0, 0, true, VoxelData.RenderTypes.Cube, VoxelData.CollisionTypes.None)},
		{Types.Dirt, new VoxelData(0, 0, 8, 8, 255, false, VoxelData.RenderTypes.Cube, VoxelData.CollisionTypes.Cube)},
		{Types.Stone, new VoxelData(8, 0, 8, 8, 255, false, VoxelData.RenderTypes.Cube, VoxelData.CollisionTypes.Cube)},
		{Types.Snow, new VoxelData(16, 0, 8, 8, 255, false, VoxelData.RenderTypes.Cube, VoxelData.CollisionTypes.Cube)},
		{Types.Sand, new VoxelData(24, 0, 8, 8, 255, false, VoxelData.RenderTypes.Cube, VoxelData.CollisionTypes.Cube)},
		{Types.Water, new VoxelData(32, 0, 8, 8, 100, true, VoxelData.RenderTypes.Cube, VoxelData.CollisionTypes.None)},
		{Types.Grass, new VoxelData(0, 8, 8, 8, 255, true, VoxelData.RenderTypes.XPlane, VoxelData.CollisionTypes.None)}
	};

	public Types type = Types.Air;

	public bool transparent = true;

	public int light = 255;

	public bool visible = false;

	public bool[] visibleFaces = new bool[6];

	public static Voxel Air = new Voxel();

	public Voxel()
	{
		type = Types.Air;
		transparent = true;
	}

	public Voxel(Types type)
	{
		SetType(type);
	}

	public void SetType(Types type)
	{
		this.type = type;
		
		if (propertiesOf[type].transparent)
			transparent = true;
		else
			transparent = false;
	}

	public bool AnyFaceVisible()
	{
		foreach (bool face in visibleFaces)
		{
			if (face) return true;
		}

		return false;
	}

}

public struct VoxelData
{
	public enum RenderTypes
	{
		Cube,
		XPlane
	}

	public enum CollisionTypes
	{
		None,
		Cube
	}

	public int atlasX;
	public int atlasY;
	public int texWidth;
	public int texHeight;
	public int alpha;
	public bool transparent;
	public RenderTypes renderType;
	public CollisionTypes collisionType;

	public VoxelData(int atlasX, int atlasY, int texWidth, int texHeight, int alpha, bool transparent, RenderTypes renderType, CollisionTypes collisionType)
	{
		this.atlasX = atlasX;
		this.atlasY = atlasY;
		this.texWidth = texWidth;
		this.texHeight = texHeight;
		this.alpha = alpha;
		this.transparent = transparent;
		this.renderType = renderType;
		this.collisionType = collisionType;
	}
}