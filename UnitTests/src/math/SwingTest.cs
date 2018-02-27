﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;

[TestClass]
public class SwingTest {
	private const float Acc = 1e-4f;

	[TestMethod]
	public void TestAxisAngle() {
		Vector2 axis = Vector2.Normalize(new Vector2(-1, 3));
		float angle = 0.8f;

		var swing = Swing.AxisAngle(axis.X, axis.Y, angle);

		MathAssert.AreEqual(axis, swing.Axis, Acc);
		Assert.AreEqual(angle, swing.Angle, Acc);

		var expectedQ = Quaternion.RotationAxis(new Vector3(axis.X, axis.Y, 0), angle);
		var q = swing.AsQuaternion(CartesianAxis.Z);
		MathAssert.AreEqual(expectedQ, q, Acc);
	}

	[TestMethod]
	public void TestAxisAngleProduct() {
		Vector2 axis = Vector2.Normalize(new Vector2(-1, 3));
		float angle = 0.8f;
		Vector2 axisAngleProduct = axis * angle;

		var swing = Swing.MakeFromAxisAngleProduct(axisAngleProduct.X, axisAngleProduct.Y);

		MathAssert.AreEqual(axisAngleProduct, swing.AxisAngleProduct, Acc);

		var expectedQ = Quaternion.RotationAxis(new Vector3(axis.X, axis.Y, 0), angle);
		var q = swing.AsQuaternion(CartesianAxis.Z);
		MathAssert.AreEqual(expectedQ, q, Acc);
	}

	[TestMethod]
	public void TestTo() {
		Vector3 to = Vector3.Normalize(new Vector3(1, -2, 3));

		var swing = Swing.To(CartesianAxis.Z, to);

		var swungTwistAxisV = swing.TransformTwistAxis(CartesianAxis.Z);
		MathAssert.AreEqual(to, swungTwistAxisV, Acc);
		
		var q = swing.AsQuaternion(CartesianAxis.Z);
		var twistAxisV = new Vector3(0, 0, 1);
		var swungTwistAxisUsingQV = Vector3.Transform(twistAxisV, q);
		MathAssert.AreEqual(to, swungTwistAxisUsingQV, Acc);
	}
	
	private static Vector3 MakeRandomUnitVector(Random rnd) {
		return Vector3.Normalize(new Vector3(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1)));
	}

	[TestMethod]
	public void TestFromTo() {
		var rnd = new Random(0);
		Vector3 a = MakeRandomUnitVector(rnd);
		Vector3 b = MakeRandomUnitVector(rnd);
		
		foreach (CartesianAxis twistAxis in CartesianAxes.Values) {
			Swing fromAToB = Swing.FromTo(twistAxis, a, b);
			MathAssert.AreEqual(b, Vector3.Transform(a, fromAToB.AsQuaternion(twistAxis)), Acc);

			Swing fromBToA = Swing.FromTo(twistAxis, b, a);
			MathAssert.AreEqual(a, Vector3.Transform(b, fromBToA.AsQuaternion(twistAxis)), Acc);
		}
	}

	[TestMethod]
    public void TestAsQuaternion() {
		MathAssert.AreEqual(Quaternion.Identity, new Swing(0, 0).AsQuaternion(CartesianAxis.X), Acc);
		MathAssert.AreEqual(Quaternion.Identity, new Swing(0, 0).AsQuaternion(CartesianAxis.Y), Acc);
		MathAssert.AreEqual(Quaternion.Identity, new Swing(0, 0).AsQuaternion(CartesianAxis.Z), Acc);

		float sinHalfOne = (float) Math.Sin(0.5);
		MathAssert.AreEqual(Quaternion.RotationAxis(Vector3.UnitY, 1), new Swing(sinHalfOne, 0).AsQuaternion(CartesianAxis.X), Acc);
		MathAssert.AreEqual(Quaternion.RotationAxis(Vector3.UnitZ, 1), new Swing(sinHalfOne, 0).AsQuaternion(CartesianAxis.Y), Acc);
		MathAssert.AreEqual(Quaternion.RotationAxis(Vector3.UnitX, 1), new Swing(sinHalfOne, 0).AsQuaternion(CartesianAxis.Z), Acc);

		MathAssert.AreEqual(Quaternion.RotationAxis(Vector3.UnitZ, 1), new Swing(0, sinHalfOne).AsQuaternion(CartesianAxis.X), Acc);
		MathAssert.AreEqual(Quaternion.RotationAxis(Vector3.UnitX, 1), new Swing(0, sinHalfOne).AsQuaternion(CartesianAxis.Y), Acc);
		MathAssert.AreEqual(Quaternion.RotationAxis(Vector3.UnitY, 1), new Swing(0, sinHalfOne).AsQuaternion(CartesianAxis.Z), Acc);
    }
	
	[TestMethod]
	public void TestAdd() {
		MathAssert.AreEqual(
			Swing.MakeFromAxisAngleProduct(0.8f, 0),
			Swing.MakeFromAxisAngleProduct(0.5f, 0) + Swing.MakeFromAxisAngleProduct(0.3f, 0),
			Acc);

		MathAssert.AreEqual(
			Swing.MakeFromAxisAngleProduct(0, 0.8f),
			Swing.MakeFromAxisAngleProduct(0, 0.5f) + Swing.MakeFromAxisAngleProduct(0, 0.3f),
			Acc);
	}

	[TestMethod]
	public void TestSubtract() {
		MathAssert.AreEqual(
			Swing.MakeFromAxisAngleProduct(0.5f, 0),
			Swing.MakeFromAxisAngleProduct(0.8f, 0) - Swing.MakeFromAxisAngleProduct(0.3f, 0),
			Acc);

		MathAssert.AreEqual(
			Swing.MakeFromAxisAngleProduct(0, 0.5f),
			Swing.MakeFromAxisAngleProduct(0, 0.8f) - Swing.MakeFromAxisAngleProduct(0, 0.3f),
			Acc);
	}

	[TestMethod]
	public void TestAddSubtractSymmetry() {
		var swing1 = new Swing(0.2f, 0.1f);
		var swing2 = new Swing(-0.3f, 0.4f);

		var diff = swing2 - swing1;
		var sum = swing1 + diff;

		MathAssert.AreEqual(swing2, sum, Acc);
	}
}