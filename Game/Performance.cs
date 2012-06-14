using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using OpenTkProject.Drawables;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace OpenTkProject.Game
{

	public struct FrameStatistics
	{

		public static float GetSecond(TimeSpan sp)
		{
			return (float)((double)sp.Ticks / (double)TimeSpan.TicksPerSecond);
		}

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
			sw.Reset();
			sw.Start();
		}

		public void EndUpdate()
		{
			sw.Stop();
			samples[curFrame].Update = FrameStatistics.GetSecond(sw.Elapsed);
		}

		public void BeginRender()
		{
			sw.Reset();
			sw.Start();
		}

		public void EndRender()
		{
			sw.Stop();
			samples[curFrame].Render = FrameStatistics.GetSecond(sw.Elapsed);

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
		int drawn;

		public void Initialize(OpenTkProjectWindow window)
		{
			cacheAmount = Performance.Instance.SampleCount * 8;
			points = new Vector3[ cacheAmount ];
			updateIndices = new int[cacheAmount];
			renderIndices = new int[cacheAmount];

			GL.GenBuffers(1, out positionVBO);
			GL.GenBuffers(1, out updateIndexVBO);
			GL.GenBuffers(1, out renderIndexVBO);

			Size = new Vector2(1280, 720);
			Position = new Vector2(0, 1280);
			step = Size.X / (float)Performance.Instance.SampleCount;

			win = window;
			shd = win.shaderLoader.getShader("perf.xsp");
			perfColorPos = GL.GetUniformLocation(shd.handle, "in_perfcolor");
		}

		private static void CheckError()
		{
			ErrorCode ec = GL.GetError();
			if (ec != 0)
			{
				throw new System.Exception(ec.ToString());
			}
		}

		public void Render()
		{
			Vector2 curPos = new Vector2(Position);
			curPos.X += Size.X;

			drawn = 0;
			foreach(FrameStatistics fs in Performance.Instance)
			{
				if (fs.Update == null)
				{
					break;
				}

				float updateSize = fs.Update * Size.Y;
				float renderSize = fs.Render * Size.Y;
				
				points[drawn++] = new Vector3(curPos);
				points[drawn++] = new Vector3(curPos.X, curPos.Y + updateSize,0);
				points[drawn++] = new Vector3(curPos.X, curPos.Y + updateSize + renderSize,0);

				curPos.X -= step;
			}


			///  5+--+2
			///   |  |
			///   |  |
			///  4+--+1
			///   |  |
			///   |  |
			///  3+--+0

			int totalBars = (drawn / 3)-1;
			int updIndex = 0;
			int rndIndex = 0;
			int curBarPoint = 0;
			for (int i=0;i<totalBars;i++)
			{
				// counter clock wise
				updateIndices[updIndex++] = curBarPoint + 0; // eg. 0
				updateIndices[updIndex++] = curBarPoint + 1; // eg. 1
				updateIndices[updIndex++] = curBarPoint + 4; // eg. 3
				updateIndices[updIndex++] = curBarPoint + 3; // eg. 4

				renderIndices[rndIndex++] = curBarPoint + 1;
				renderIndices[rndIndex++] = curBarPoint + 2;
				renderIndices[rndIndex++] = curBarPoint + 5;
				renderIndices[rndIndex++] = curBarPoint + 4;

				curBarPoint += 3;
			}


			GL.UseProgram(shd.handle);
			CheckError();

			GL.EnableClientState(ArrayCap.IndexArray);
			GL.EnableClientState(ArrayCap.VertexArray);

			GL.BindBuffer(BufferTarget.ArrayBuffer, positionVBO);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(cacheAmount * Vector3.SizeInBytes), points, BufferUsageHint.StaticDraw);
			CheckError();

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, updateIndexVBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(cacheAmount * sizeof(int)), updateIndices, BufferUsageHint.StaticDraw);
			CheckError();

			GL.Uniform4(perfColorPos, 1.0f, 0, 0, 1.0f);
			CheckError();
			GL.DrawElements(BeginMode.Quads,updIndex / 4, DrawElementsType.UnsignedInt,0);
			CheckError();

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderIndexVBO);
			GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(cacheAmount * sizeof(int)), renderIndices, BufferUsageHint.StaticDraw);
			
			GL.Uniform4(perfColorPos, 0.0f, 1.0f, 0, 1.0f);
			GL.DrawElements(BeginMode.Quads, rndIndex / 4, DrawElementsType.UnsignedInt, 0);

			GL.DisableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.IndexArray);

			
		}

		public int positionVBO;
		public int updateIndexVBO;
		public int renderIndexVBO;
		public int perfColorPos;

		Vector3[] points;
		int[] updateIndices;
		int[] renderIndices;
	}
}
