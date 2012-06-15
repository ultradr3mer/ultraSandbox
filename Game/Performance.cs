using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using OpenTkProject.Drawables;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Graphics;

namespace OpenTkProject.Game
{

	public struct FrameStatistics
	{
		/// <summary>
		/// How much seconds update took?
		/// </summary>
		public float Update;

		/// <summary>
		/// How much seconds rendering took?
		/// </summary>
		public float Render;
	}

	public class PerformanceSampleEnumerator : IEnumerator
	{
		Performance prf;
		int curFrame = 0;
		int remFrame = 0;
		int actFrame = 0;

		public PerformanceSampleEnumerator(Performance perfo)
		{
			prf = perfo;
			Reset();
		}

		public void Reset()
		{
			actFrame = prf.curFrame+1;
			remFrame = prf.SampleCount-1;
			curFrame = -1;
		}

		public bool MoveNext()
		{
			if (remFrame < 0)
			{
				return false;
			}

			actFrame--;
			if (actFrame < 0)
			{
				actFrame = prf.SampleCount - 1;
			}
			curFrame++;
			remFrame--;
			return true;
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public FrameStatistics Current
		{
			get
			{
				return prf.samples[actFrame];
			}
		}
	}

	/// <summary>
	/// Acts like a circular buffer of time samples to see performance impact real time
	/// TODO: I will implement multiple samples, so there can be more samples other than Render and Update
	/// </summary>
	public class Performance: IEnumerable
	{ 

		public int SampleCount
		{
			get
			{
				return samples.Length;
			}
			set
			{
				Initialize(value);
			}
		}

		public FrameStatistics[] samples;
		public int curFrame = 0;

		Stopwatch sw = new Stopwatch();

		/// <summary>
		/// Singleton
		/// </summary>
		public static Performance Instance = new Performance();


		public void Initialize(int sampleCount)
		{
			samples = new FrameStatistics[sampleCount];
			curFrame = 0;
		}

		public void BeginUpdate()
		{
			sw.Restart();
		}

		public static float GetSecond(TimeSpan sp)
		{
			return (float)((double)sp.Ticks / (double)TimeSpan.TicksPerSecond);
		}

		public void EndUpdate()
		{
			sw.Stop();
			samples[curFrame].Update = GetSecond(sw.Elapsed);
		}

		public void BeginRender()
		{
			sw.Restart();
		}

		public void EndRender()
		{
			sw.Stop();
			samples[curFrame].Render = GetSecond(sw.Elapsed);

			AdvanceFrame();
		}

		public void AdvanceFrame()
		{
			curFrame++;
			if (curFrame >= samples.Length)
			{
				curFrame = 0;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new PerformanceSampleEnumerator(this);
		}
	}

	/// <summary>
	/// Visualizes performance data
	/// TODO: implement a dynamic Mesh class and use it instead doing these stuff in here
	/// </summary>
	public class PerformanceVisualizer
	{
		Shader shd;
		OpenTkProjectWindow win;

		/// <summary>
		/// This can be singleton too
		/// </summary>
		public static PerformanceVisualizer Instance = new PerformanceVisualizer();

		public Vector2 Size;
		public Vector2 Position;

		float step; // horizontal pixel step for each sample
		int cacheAmount;
		int pointCount;

		int stripeCount;

		public void Initialize(OpenTkProjectWindow window)
		{
			cacheAmount = Performance.Instance.SampleCount * 8; // this is totally random amount
			points = new Vector3[ cacheAmount ];
			updateIndices = new int[cacheAmount];
			renderIndices = new int[cacheAmount];

			GL.GenBuffers(1, out positionVBO);
			GL.GenBuffers(1, out updateIndexVBO);
			GL.GenBuffers(1, out renderIndexVBO);

			Size = window.screenSize;
			Position = new Vector2(0, 0);
			step = Size.X / (float)Performance.Instance.SampleCount;

			win = window;
			shd = win.shaderLoader.getShader("perf.xsp");
			perfColorPos = GL.GetUniformLocation(shd.handle, "in_perfcolor");
			inposPos = GL.GetAttribLocation(shd.handle, "in_position");
			projPos = GL.GetUniformLocation(shd.handle, "projection_matrix");

			/// create VAO for Update graph
			GL.GenVertexArrays(1, out vaoUpdate);
			GL.BindVertexArray(vaoUpdate);

			GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
			GL.VertexAttribPointer(inposPos, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
			GL.EnableVertexAttribArray(inposPos);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, updateIndexVBO);


			/// create VAO for Render graph
			GL.GenVertexArrays(1, out vaoRender);
			GL.BindVertexArray(vaoRender);

			GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
			GL.VertexAttribPointer(inposPos, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
			GL.EnableVertexAttribArray(inposPos);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderIndexVBO);

			GL.BindVertexArray(0);

			CheckError();

			ProjMatrix = Matrix4.CreateOrthographicOffCenter(0, win.screenSize.X, win.screenSize.Y, 0, 0, 1);
		}

		private static void CheckError()
		{
			ErrorCode ec = GL.GetError();
			if (ec != 0)
			{
				throw new System.Exception(ec.ToString());
			}
		}

		public void Update()
		{
			Vector2 curPos = Position;
			curPos.Y = Size.Y;
			curPos.X += Size.X;

			pointCount = 0;
			foreach (FrameStatistics fs in Performance.Instance)
			{
				float updateSize = fs.Update * Size.Y;
				float renderSize = fs.Render * Size.Y;

				points[pointCount++] = new Vector3(curPos);
				points[pointCount++] = new Vector3(curPos.X, curPos.Y - updateSize, 0);
				points[pointCount++] = new Vector3(curPos.X, (curPos.Y - updateSize) - renderSize, 0);

				curPos.X -= step;
			}

			int totalBars = (pointCount / 3);
			int updIndex = 0;
			int rndIndex = 0;
			int curBarPoint = 0;

			for (int i = 0; i < totalBars; i++)
			{
				updateIndices[updIndex++] = curBarPoint + 0; // eg. 3
				updateIndices[updIndex++] = curBarPoint + 1; // eg. 4

				renderIndices[rndIndex++] = curBarPoint + 1;
				renderIndices[rndIndex++] = curBarPoint + 2;

				curBarPoint += 3;
			}

			stripeCount = rndIndex;

			/// UPDATE VBO DATA

			GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
			GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(cacheAmount * Vector3.SizeInBytes), points, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, updateIndexVBO);
			GL.BufferData<int>(BufferTarget.ElementArrayBuffer, (IntPtr)(cacheAmount * sizeof(int)), updateIndices, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderIndexVBO);
			GL.BufferData<int>(BufferTarget.ElementArrayBuffer, (IntPtr)(cacheAmount * sizeof(int)), renderIndices, BufferUsageHint.StaticDraw);
		}

		public void Render()
		{
			/// Update data
			Update();

			GL.UseProgram(shd.handle);
			GL.UniformMatrix4(projPos, false, ref ProjMatrix);

			GL.BindVertexArray(vaoUpdate);
			GL.Uniform4(perfColorPos, new Vector4(1f, 0f, 0f, 1f));
			GL.DrawElements(BeginMode.TriangleStrip, stripeCount, DrawElementsType.UnsignedInt, 0);

			GL.BindVertexArray(vaoRender);
			GL.Uniform4(perfColorPos, new Vector4(0f, 1f, 0f, 1f));
			GL.DrawElements(BeginMode.TriangleStrip, stripeCount, DrawElementsType.UnsignedInt, 0);

			GL.BindVertexArray(0);
		}

		public int positionVBO;
		public int updateIndexVBO;
		public int renderIndexVBO;
		public int perfColorPos;
		public int inposPos;
		public int projPos;

		public int vaoUpdate;
		public int vaoRender;

		Matrix4 ProjMatrix;
		Vector3[] points;
		int[] updateIndices;
		int[] renderIndices;
	}
}
