using System.Diagnostics;
using SharpDX;
using static System.Math;
using static MathExtensions;

public struct Swing {
	private readonly float y;
	private readonly float z;
	
	[Conditional("DEBUG")]
	private static void AssertValid(float y, float z) {
		float lengthSquared = Sqr(y) + Sqr(z);
		Debug.Assert(lengthSquared < 1 + 1e-2f);
	}

	public Swing(float y, float z) {
		AssertValid(y, z);

		this.y = y;
		this.z = z;
	}
	
	public float Y => y;
	public float Z => z;
	public float WSquared => 1 - Sqr(y) - Sqr(z);
		
	public float W {
		get {
			float wSquared = WSquared;
			return wSquared > 0 ? (float) Sqrt(wSquared) : 0;
		}
	}

	public Vector2 Axis => Vector2.Normalize(new Vector2(y, z));
	public float Angle => (float) Acos(2 * WSquared - 1);

	public Vector2 AxisAngleProduct {
		get {
			float length = (float) Sqrt(Sqr(Y) + Sqr(Z));
			float angle = 2 * (float) Asin(length);
			float m = length == 0 ? 0 : angle / length;
			return new Vector2(Y * m, Z * m);
		}
	}

	public static readonly Swing Zero = new Swing(0, 0);

	override public string ToString() {
		return string.Format("Swing[{0}, {1}]", Y, Z);
	}

	public Vector3 GetRotationAxis(CartesianAxis twistAxis) {
		Vector3 axis = default(Vector3);
		axis[((int) twistAxis + 1) % 3] = Y;
		axis[((int) twistAxis + 2) % 3] = Z;
		return Vector3.Normalize(axis);
	}

	public Quaternion AsQuaternion(CartesianAxis twistAxis) {
		Quaternion q = default(Quaternion);
		q[((int) twistAxis + 1) % 3] = Y;
		q[((int) twistAxis + 2) % 3] = Z;
		q.W = W;
		return q;
	}

	public static Swing MakeFromAxisAngleProduct(float axisAngleY, float axisAngleZ) {
		float angle = (float) IEEERemainder(Sqrt(Sqr(axisAngleY) + Sqr(axisAngleZ)), 2 * PI);
		float m = angle == 0 ? 0 : (float) Sin(angle / 2) / angle;
		return new Swing(m * axisAngleY, m * axisAngleZ);
	}

	public static Swing AxisAngle(float axisY, float axisZ, float angle) {
		angle = (float) IEEERemainder(angle, 2 * PI);
		if (angle == 0) {
			return new Swing(0, 0);
		}

		DebugUtilities.AssertIsUnit(axisY, axisZ);

		float halfSin = (float) Sin(angle / 2);
		return new Swing(halfSin * axisY, halfSin * axisZ);
	}

	public static Swing To(CartesianAxis twistAxis, Vector3 to) {
		DebugUtilities.AssertIsUnit(to);

		float toX = to[((int) twistAxis + 0) % 3];
		float toY = to[((int) twistAxis + 1) % 3];
		float toZ = to[((int) twistAxis + 2) % 3];

		/*
		 * To reconstruct the swing quaternion, we need to calculate:
		 *     <y, z> = Sin[angle / 2] * rotation-axis
		 *     w = Cos[angle / 2]
		 *   
		 * We know:
		 *     Cos[angle]
		 *       = Dot[twist-axis, to]
		 *       = toX
		 *  
		 *     rotation-axis
		 *       = Normalize[Cross[twist-axis, to]]
		 *       = Normalize[<-toZ, toX>]
		 *       = <-toZ, toX> / Sqrt[toX^2 + toZ^2]
		 *       = <-toZ, toX> / Sqrt[1 - toX^2]
		 * 
		 * So:
         *     w = Cos[angle / 2]
		 *       = Sqrt[(1 + Cos[angle]) / 2]    (half-angle trig identity)
		 *       = Sqrt[(1 + toX) / 2]
		 *      
		 *    <y,z>
		 *      = Sin[angle / 2] * rotation-axis
		 *      = Sqrt[(1 - Cos[angle]) / 2] * rotation-axis    (half-angle trig identity)
		 *      = Sqrt[(1 - toX) / 2] * rotation-axis
		 *      = Sqrt[(1 - toX) / 2] / Sqrt[1 - toX^2] * <-toZ, toY>
		 *      = Sqrt[(1 - toX) / (2 * (1 - toX^2))] * <-toZ, toY>
		 *      = Sqrt[(1 - toX) / (2 * (1 - toX) * (1 + toX))] * <-toZ, toY>
		 *      = Sqrt[1 / (2 * (1 + toX))] * <-toZ, toY>
		 *      = 1 / (2 * w) * <-toZ, toY> 
		 */
		float wSquared = (1 + toX) / 2;
		float w = (float) Sqrt(wSquared);
		float y = -toZ / (2 * w);
		float z = +toY / (2 * w);
		
		return new Swing(y, z);
	}

	public Vector3 TransformTwistAxis(CartesianAxis twistAxis) {
		float x = 2 * WSquared - 1;
		float y = 2 * W * Z;
		float z = 2 * W * -Y;

		Vector3 v = default(Vector3);
		v[((int) twistAxis + 0) % 3] = x;
		v[((int) twistAxis + 1) % 3] = y;
		v[((int) twistAxis + 2) % 3] = z;
		return v;
	}

	public Vector3 Transform(CartesianAxis twistAxis, Vector3 v) {
		float vx = v[((int) twistAxis + 0) % 3];
		float vy = v[((int) twistAxis + 1) % 3];
		float vz = v[((int) twistAxis + 2) % 3];

		float w = W;
		
		float c = 2 * (w*vx - z*vy + y*vz);
		float rx = -vx + w*c;
		float ry = +vy + z*c;
		float rz = +vz - y*c;

		Vector3 result = default(Vector3);
		result[((int) twistAxis + 0) % 3] = rx;
		result[((int) twistAxis + 1) % 3] = ry;
		result[((int) twistAxis + 2) % 3] = rz;
		return result;
	}

	public static Swing FromTo(CartesianAxis twistAxis, Vector3 from, Vector3 to) {
		/*
		 * This function is not optimized.
		 * I should write an optimized version if I need to use FromTo in production.
		 */
		
		DebugUtilities.AssertIsUnit(from);
		DebugUtilities.AssertIsUnit(to);
		
		float fromX = from[((int) twistAxis + 0) % 3];
		float fromY = from[((int) twistAxis + 1) % 3];
		float fromZ = from[((int) twistAxis + 2) % 3];
		
		float toX = to[((int) twistAxis + 0) % 3];
		float toY = to[((int) twistAxis + 1) % 3];
		float toZ = to[((int) twistAxis + 2) % 3];

		Vector2 axis = Vector2.Normalize(new Vector2(fromZ - toZ, toY - fromY));
		
		float projectionLength = axis.X * fromY + axis.Y * fromZ; 
		Vector2 projection = axis * projectionLength; //by construction, projection onto axis is same for from and to
		Vector3 fromRejection = new Vector3(fromY - projection.X, fromZ - projection.Y, fromX);
		Vector3 toRejection = new Vector3(toY - projection.X, toZ - projection.Y, toX);
		Vector3 rejectionCross = Vector3.Cross(fromRejection, toRejection);
		float rejectionDot = Vector3.Dot(fromRejection, toRejection);
		
		float angle = (float) Atan2(axis.X * rejectionCross.X + axis.Y * rejectionCross.Y, rejectionDot);
		
		return Swing.AxisAngle(axis.X, axis.Y, angle);
	}

	/**
	 *  Returns a Swing delta such that:
	 *		delta.Transform(twistAxis, initial.TransformTwistAxis(twistAxis)) == final.TransformTwistAxis(twistAxis)
	 */
	public static Swing CalculateDelta(Swing initial, Swing final) {
		float iw = initial.W;
		float iy = initial.Y;
		float iz = initial.Z;

		float fw = final.W;
		float fy = final.Y;
		float fz = final.Z;

		float pw = fw * fw - 1;
		float py = fy * fw;
		float pz = fz * fw;

		float dw = pw + iw * iw;
		float dy = py - iw * iy;
		float dz = pz - iw * iz;
		float m = 1 / (float) Sqrt(dw * dw + dy * dy + dz * dz);
		dw *= m;
		dy *= m;
		dz *= m;

		if (dw < 0) {
			dw *= -1;
			dy *= -1;
			dz *= -1;
		}

		return new Swing(dy, dz);
	}

	/**
	 *  Returns a Swing final such that:
	 *		delta.Transform(twistAxis, initial.TransformTwistAxis(twistAxis)) == final.TransformTwistAxis(twistAxis)
	 */
	public static Swing ApplyDelta(Swing initial, Swing delta) {
		float iw = initial.W;
		float iy = initial.Y;
		float iz = initial.Z;

		float dw = delta.W;
		float dy = delta.Y;
		float dz = delta.Z;
		
		float c = 2 * iw * (dw*iw - dz*iz - dy*iy) - dw;
		float pw = dw*c - iw*iw;
		float py = dy*c + iw*iy;
		float pz = dz*c + iw*iz;
		
		float fw = (float) Sqrt(pw + 1);
		float fy = +py / fw;
		float fz = +pz / fw;

		return new Swing(fy, fz);
	}
}
