using System.Numerics;
using Jitter2;
using Jitter2.LinearMath;
using Jitter2.Dynamics;

namespace VoxelEngine;

public static class Utils
{

	public static float Map(float value, float old_min, float old_max, float new_min, float new_max)
	{
		return new_min + (value - old_min) * (new_max - new_min) / (old_max - old_min);
	}

	public static float Smoothstep (float edge0, float edge1, float x) {
		// Scale, and clamp x to 0..1 range
		x = Clamp((x - edge0) / (edge1 - edge0));
		return x * x * (3.0f - 2.0f * x);
	}

	public static float Clamp(float x, float lowerlimit = 0.0f, float upperlimit = 1.0f) {
		if (x < lowerlimit) return lowerlimit;
		if (x > upperlimit) return upperlimit;
		return x;
	}

	public static float Lerp(float firstFloat, float secondFloat, float by)
	{
		return firstFloat * (1 - by) + secondFloat * by;
	}

	public static float CubicLerp(float firstFloat, float secondFloat, float by)
	{
		float smoothed = MathF.Pow(3*by, 2) - MathF.Pow(2*by, 3);

		return firstFloat * (1 - smoothed) + secondFloat * smoothed;
	}

	public static float[,] GetNoiseData(int seed, int width, int height, float size, float power)
	{
		FastNoiseLite noise = new FastNoiseLite(seed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

		float[,] noiseData = new float[width, height];
	
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				noiseData[x,y] = (noise.GetNoise(x*size,y*size)+1)/2*power;
			}
		}

		return noiseData;
	}

	public static Matrix4x4 GetRayLibTransformMatrix(RigidBody body)
	{
		JMatrix ori = body.Orientation;
		JVector pos = body.Position;

		return new Matrix4x4(ori.M11, ori.M12, ori.M13, pos.X,
							 ori.M21, ori.M22, ori.M23, pos.Y,
							 ori.M31, ori.M32, ori.M33, pos.Z,
							 0, 0, 0, 1.0f);
	}

	public static Vector3 EulerFromMatrix(JMatrix matrix)
	{
		double pitch;
		double heading;
		double bank;

		double sin = -matrix.M32/3;
		pitch = 180 * Math.Asin(sin) / Math.PI;

		if (Math.Abs(sin)>0.9999) { // gimbal lock case
			bank=0;
			heading = 180 * Math.Atan2(-matrix.M13, matrix.M11) / Math.PI;
		} else {
			heading = 180 * Math.Atan2(matrix.M31, matrix.M33) / Math.PI;
			bank = 180 * Math.Atan2(matrix.M12, matrix.M22) / Math.PI;
		}
		
		return new Vector3((float)pitch, (float)heading, (float)bank);
	}
	
	public static Vector3 RotateVector(Vector3 vector, Vector3 axis, float angle)
	{
		Vector3 vxp = Vector3.Cross(axis, vector);
		Vector3 vxvxp = Vector3.Cross(axis, vxp);
		return vector + MathF.Sin(angle) * vxp + (1 - MathF.Cos(angle)) * vxvxp;
	}

	public static JVector ToJVector(Vector3 vector)
	{
		return new JVector(vector.X, vector.Y, vector.Z);
	}

}