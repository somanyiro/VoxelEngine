using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using Jitter2;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;

namespace VoxelEngine;

public class Player
{
	public Camera3D camera;

	RigidBody body;

	Vector2 screenCenter = new Vector2(640, 360);
	float sensitivity = 0.7f;

	bool captureCursor = false;
	Vector3 lookDirection = new Vector3(1, 0, 0);

	public Player(Jitter2.World physicsWorld)
	{
		camera.position = new Vector3(20.0f, 20.0f, 0.0f);
		camera.target = new Vector3(0.0f, 0.0f, 0.0f);
		camera.up = new Vector3(0.0f, 1.0f, 0.0f);
		camera.fovy = 75.0f;
		camera.projection = CameraProjection.CAMERA_PERSPECTIVE;

		body = physicsWorld.CreateRigidBody();
		body.AddShape(new CylinderShape(2, 1));
		body.Position = new JVector(20, 20, 0);
		body.EnableSpeculativeContacts = true;
		body.Friction = 0;
		//body.Velocity
	}

	public void Update(float deltaTime)
	{
		camera.position = new Vector3(body.Position.X, body.Position.Y+1, body.Position.Z);
		body.Orientation = JMatrix.Identity;
		camera.target = camera.position + lookDirection;

		if (IsKeyPressed(KeyboardKey.KEY_TAB))
		{
			if (!captureCursor)
			{
				captureCursor = true;
				HideCursor();
				SetMousePosition((int)screenCenter.X, (int)screenCenter.Y);
			}
			else
			{
				captureCursor = false;
				ShowCursor();
			}
		}

		if (captureCursor)
		{
			//camera movement
			Vector2 mouseMovement = GetMousePosition() - screenCenter;
			SetMousePosition((int)screenCenter.X, (int)screenCenter.Y);
			lookDirection = Utils.RotateVector(lookDirection, Vector3.UnitY, -mouseMovement.X / 200 * sensitivity);
			Vector3 upDownRotationAxis = Utils.RotateVector(Vector3.Normalize(new Vector3(lookDirection.X, 0, lookDirection.Z)), Vector3.UnitY, 90 * MathF.PI / 180);
			lookDirection = Utils.RotateVector(lookDirection, upDownRotationAxis, mouseMovement.Y / 200 * sensitivity);

			//lookDirection.Y = (float)Math.Clamp(lookDirection.Y, -0.9, 0.9);
			//lookDirection = Vector3.Normalize(lookDirection);

			Vector3 walkDirection = Vector3.Zero;
			Vector3 forwardDirection = Vector3.Normalize(new Vector3(lookDirection.X, 0, lookDirection.Z));

			if (IsKeyDown(KeyboardKey.KEY_W))
				walkDirection += forwardDirection;
			if (IsKeyDown(KeyboardKey.KEY_S))
				walkDirection += -forwardDirection;
			if (IsKeyDown(KeyboardKey.KEY_A))
				walkDirection += Utils.RotateVector(forwardDirection, Vector3.UnitY, 90 * MathF.PI / 180);
			if (IsKeyDown(KeyboardKey.KEY_D))
				walkDirection += Utils.RotateVector(forwardDirection, Vector3.UnitY, -90 * MathF.PI / 180);

				body.AddForce(Utils.ToJVector(walkDirection * 100));
		}

	}
}