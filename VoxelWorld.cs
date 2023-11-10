using System;
using Raylib_cs;
using static Raylib_cs.Raylib;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace VoxelEngine;

public class VoxelWorld
{
	World world = new World(10, 5, 10);
	Jitter2.World physicsWorld = new ();

	Chunk[] distanceOrder;

	Texture2D atlas;

	public void Run()
	{
		InitWindow(1280, 720, "VoxelEngine");

		distanceOrder = new Chunk[world.chunks.GetLength(0)*world.chunks.GetLength(1)*world.chunks.GetLength(2)];

		{

		int fillCounter = 0;

		for (int x = 0; x < world.chunks.GetLength(0); x++) {
			for (int y = 0; y < world.chunks.GetLength(1); y++) {
				for (int z = 0; z < world.chunks.GetLength(2); z++) {
					distanceOrder[fillCounter] = world.chunks[x,y,z];
					fillCounter++;
				}
			}
		}

		}


		physicsWorld.UseFullEPASolver = true;
		physicsWorld.Gravity = JVector.UnitY * -9.8f * 2;
		//physicsWorld.NumberSubsteps = 10; //not sure if these help or not
		//physicsWorld.SolverIterations = 10;

		Player player = new Player(physicsWorld);

		Random random = new Random();

		atlas = LoadTexture("assets/texture_atlas.png");

		world.CreatePhysicsObjects(physicsWorld);

		while (!WindowShouldClose())
		{
			if (IsKeyPressed(KeyboardKey.KEY_G))
			{
				int newSeed = random.Next(1000, 10000);
				Console.WriteLine($"seed: {newSeed}");
				world.Generate(newSeed);
				
				List<RigidBody> toRemove = new List<RigidBody>();
				foreach (var body in physicsWorld.RigidBodies) 
				{
					if (body.IsStatic)
					{
						toRemove.Add(body);
					}
				}
				foreach (var body in toRemove) 
				{
					physicsWorld.Remove(body);
				}

				world.CreatePhysicsObjects(physicsWorld);

				Console.WriteLine($"active physics bodies: {physicsWorld.RigidBodies.Count()}");
			}

			//sort array of chunks by distance to player
			Utils.BubbleSortIteration<Chunk>(ref distanceOrder, (a,b) => Vector3.Distance(a.position, player.Position) > Vector3.Distance(b.position, player.Position));

			player.Update(GetFrameTime());

			physicsWorld.Step(GetFrameTime(), true);

			BeginDrawing();
			ClearBackground(Color.SKYBLUE);

			BeginMode3D(player.camera);

			Rlgl.rlBegin(DrawMode.QUADS);//I moved this here from DrawVoxel because it was slowing down rendering

			//draw non transparent voxels
			for (int x = 0; x < world.chunks.GetLength(0); x++) {
				for (int y = 0; y < world.chunks.GetLength(1); y++) {
					for (int z = 0; z < world.chunks.GetLength(2); z++) {
						DrawChunk(world.chunks[x,y,z], false);
					}
				}
			}
			//draw transparent voxels
			for (int x = 0; x < world.chunks.GetLength(0); x++) {
				for (int y = 0; y < world.chunks.GetLength(1); y++) {
					for (int z = 0; z < world.chunks.GetLength(2); z++) {
						DrawChunk(world.chunks[x,y,z], true);
					}
				}
			}

			if (IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
			{
				Ray mouseRay = GetMouseRay(new Vector2(1280/2, 720/2), player.camera);

				Vector3 closestHit;

				for (int i = 0; i < 27; i++)
				{
					for (int x = 0; x < 8; x++) {
						for (int y = 0; y < 8; y++) {
							for (int z = 0; z < 8; z++) {
								
								BoundingBox voxelBox = new BoundingBox(distanceOrder[i].position - new Vector3(0.5f,0.5f,0.5f), distanceOrder[i].position + Vector3.One);

								RayCollision collision = GetRayCollisionBox(mouseRay, voxelBox);

								if (collision.hit)
									world.SetVoxel();

							}
						}
					}

					
				}
			}

			for (int i = 0; i < 100; i++)
			{
				//DrawChunk(distanceOrder[i], false);
				//DrawCube(distanceOrder[i].position, 3, 3, 3, Color.WHITE);
			}

			/*
			foreach (var body in physicsWorld.RigidBodies) 
			{
				DrawCubeWires(new Vector3(body.Position.X, body.Position.Y, body.Position.Z), 1, 1, 1, Color.WHITE);
			}
			*/
			Rlgl.rlEnd();
				
			EndMode3D();

			DrawFPS(10,10);
			
			EndDrawing();
		}

		CloseWindow();
	}

	void DrawChunk(Chunk chunk, bool drawOnlyTransparent)
	{
		if (chunk.empty) return;

		for (int x = 0; x < 8; x++) {
			for (int y = 0; y < 8; y++) {
				for (int z = 0; z < 8; z++) {
					Voxel voxel = chunk.voxels[x,y,z];

					if (voxel.type == Voxel.Types.Air) continue;

					if (drawOnlyTransparent && !voxel.transparent) continue;
					if (!drawOnlyTransparent && voxel.transparent) continue;

					if(!voxel.visible) continue;

					VoxelData voxelData = Voxel.propertiesOf[voxel.type];

					switch (voxelData.renderType)
					{
						case VoxelData.RenderTypes.Cube:
							DrawVoxel(
								atlas,
								new Rectangle(voxelData.atlasX, voxelData.atlasY, voxelData.texWidth, voxelData.texHeight),
								new Vector3(chunk.position.X+x, chunk.position.Y+y, chunk.position.Z+z),
								new Color(voxel.light, voxel.light, voxel.light, voxelData.alpha),
								voxel.visibleFaces);
							break;

						case VoxelData.RenderTypes.XPlane:
							DrawXPlane(
								atlas,
								new Rectangle(voxelData.atlasX, voxelData.atlasY, voxelData.texWidth, voxelData.texHeight),
								new Vector3(chunk.position.X+x, chunk.position.Y+y, chunk.position.Z+z),
								new Color(voxel.light, voxel.light, voxel.light, voxelData.alpha));
							break;
					}
					
				}
			}
		}

	}

	// Draw cube with texture piece applied to all faces
	static void DrawVoxel(
		Texture2D texture,
		Rectangle source,
		Vector3 position,
		Color color,
		bool[] faces
	)
	{
		float x = position.X;
		float y = position.Y;
		float z = position.Z;
		float texWidth = (float)texture.width;
		float texHeight = (float)texture.height;

		// Set desired texture to be enabled while drawing following vertex data
		Rlgl.rlSetTexture(texture.id);

		// We calculate the normalized texture coordinates for the desired texture-source-rectangle
		// It means converting from (tex.Width, tex.Height) coordinates to [0.0f, 1.0f] equivalent
		//Rlgl.rlBegin(DrawMode.QUADS);
		Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

		if (faces[0])
		{
			// Front face
			Rlgl.rlNormal3f(0.0f, 0.0f, 1.0f);
			Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z + 0.5f);
		}

		if (faces[1])
		{
			// Back face
			Rlgl.rlNormal3f(0.0f, 0.0f, -1.0f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z - 0.5f);
		}

		if (faces[2])
		{
			// Top face
			Rlgl.rlNormal3f(0.0f, 1.0f, 0.0f);
			Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z - 0.5f);
		}

		if (faces[3])
		{
			// Bottom face
			Rlgl.rlNormal3f(0.0f, -1.0f, 0.0f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z + 0.5f);
		}

		if (faces[4])
		{
			// Right face
			Rlgl.rlNormal3f(1.0f, 0.0f, 0.0f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z + 0.5f);
		}

		if (faces[5])
		{
			// Left face
			Rlgl.rlNormal3f(-1.0f, 0.0f, 0.0f);
			Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z - 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z + 0.5f);
			Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
			Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z - 0.5f);
		}

		//Rlgl.rlEnd();

		Rlgl.rlSetTexture(0);
	}

	static void DrawXPlane(
		Texture2D texture,
		Rectangle source,
		Vector3 position,
		Color color)
	{
		float x = position.X;
		float y = position.Y;
		float z = position.Z;
		float texWidth = (float)texture.width;
		float texHeight = (float)texture.height;

		Rlgl.rlSetTexture(texture.id);

		Rlgl.rlColor4ub(color.r, color.g, color.b, color.a);

		//front face
		Rlgl.rlNormal3f(0.0f, 0.0f, 1.0f);
		Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z);
		Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z);

		//back face
		Rlgl.rlNormal3f(0.0f, 0.0f, -1.0f);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x - 0.5f, y - 0.5f, z);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x - 0.5f, y + 0.5f, z);
		Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x + 0.5f, y + 0.5f, z);
		Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x + 0.5f, y - 0.5f, z);

		//right face
		Rlgl.rlNormal3f(1.0f, 0.0f, 0.0f);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x, y - 0.5f, z - 0.5f);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x, y + 0.5f, z - 0.5f);
		Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x, y + 0.5f, z + 0.5f);
		Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x, y - 0.5f, z + 0.5f);

		//left face
		Rlgl.rlNormal3f(-1.0f, 0.0f, 0.0f);
		Rlgl.rlTexCoord2f(source.x / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x, y - 0.5f, z - 0.5f);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, (source.y + source.height) / texHeight);
		Rlgl.rlVertex3f(x, y - 0.5f, z + 0.5f);
		Rlgl.rlTexCoord2f((source.x + source.width) / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x, y + 0.5f, z + 0.5f);
		Rlgl.rlTexCoord2f(source.x / texWidth, source.y / texHeight);
		Rlgl.rlVertex3f(x, y + 0.5f, z - 0.5f);

		Rlgl.rlSetTexture(0);
	}
	
}