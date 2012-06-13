using System;
using System.Drawing;
using System.Collections.Generic;

using Jitter.LinearMath;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace OpenTkProject
{
    public sealed class GenericMethods
    {
        static string splitter = "|";

        public static XmlWriter CoolXMLWriter(Stream output)
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.IndentChars = "\t";

            return XmlWriter.Create(output, xws);
        }

        public static Matrix4 Matrix4Zero = new Matrix4(
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0);

        public static NumberFormatInfo getNfi()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ",";
            nfi.NumberDecimalSeparator = ".";
            return nfi;
        }

        public static Vector3 ToOpenTKVector(JVector vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static JVector FromOpenTKVector(Vector3 vector)
        {
            return new JVector(vector.X, vector.Y, vector.Z);
        }

        public static List<JVector> FromOpenTKVecArToJVecList(Vector3[] vectors)
        {
            List<JVector> mList = new List<JVector> { };
            foreach (Vector3 vector in vectors)
            {
                mList.Add(FromOpenTKVector(vector));
            }
            return mList;
        }

        public static string StringFromStringList(List<string> stringAryList)
        {
            if (stringAryList.Count > 0)
            {
                StringWriter sw = new StringWriter();
                for (int i = 0; i < stringAryList.Count - 1; i++)
                {
                    sw.Write(stringAryList[i]);
                    sw.Write(splitter);
                }
                sw.Write(stringAryList[stringAryList.Count - 1]);

                return sw.ToString();
            }
            else
            {
                return "";
            }
        }

        public static List<string> StringListFromString(string mString)
        {
            string[] splitString = mString.Split(splitter.ToCharArray());
            List<string> stringList = new List<string> { };
            foreach (var curString in splitString)
            {
                stringList.Add(curString);
            }
            return stringList;
            {

            }
        }

        public static string StringFromJMatrix(JMatrix mMat)
        {
            NumberFormatInfo nfi = getNfi();
            string mString =
                mMat.M11.ToString(nfi) + splitter +
                mMat.M12.ToString(nfi) + splitter +
                mMat.M13.ToString(nfi) + splitter +
                mMat.M21.ToString(nfi) + splitter +
                mMat.M22.ToString(nfi) + splitter +
                mMat.M23.ToString(nfi) + splitter +
                mMat.M31.ToString(nfi) + splitter +
                mMat.M32.ToString(nfi) + splitter +
                mMat.M33.ToString(nfi);

            return mString;

        }

        public static JMatrix JMatrixFromString(string mString)
        {
            NumberFormatInfo nfi = GenericMethods.getNfi();
            string[] fields = mString.Split(splitter.ToCharArray());
            JMatrix mMat = new JMatrix(
                float.Parse(fields[0], nfi),
                float.Parse(fields[1], nfi),
                float.Parse(fields[2], nfi),
                float.Parse(fields[3], nfi),
                float.Parse(fields[4], nfi),
                float.Parse(fields[5], nfi),
                float.Parse(fields[6], nfi),
                float.Parse(fields[7], nfi),
                float.Parse(fields[8], nfi));

            return mMat;
        }

        public static Matrix4 Matrix4FromArray(float[] ary)
        {
            Matrix4 tmpMat = new Matrix4();
            tmpMat.M11 = ary[0];
            tmpMat.M12 = ary[1];
            tmpMat.M13 = ary[2];
            tmpMat.M14 = ary[3];

            tmpMat.M21 = ary[4];
            tmpMat.M22 = ary[5];
            tmpMat.M23 = ary[6];
            tmpMat.M24 = ary[7];

            tmpMat.M31 = ary[8];
            tmpMat.M32 = ary[9];
            tmpMat.M33 = ary[10];
            tmpMat.M34 = ary[11];

            tmpMat.M41 = ary[12];
            tmpMat.M42 = ary[13];
            tmpMat.M43 = ary[14];
            tmpMat.M44 = ary[15];

            return tmpMat;
        }


        public static string StringFromVector3(Vector3 mVec)
        {
            NumberFormatInfo nfi = getNfi();
            string mString =
                mVec.X.ToString(nfi) + splitter +
                mVec.Y.ToString(nfi) + splitter +
                mVec.Z.ToString(nfi);

            return mString;

        }

        public static Vector3 VectorFromString(string mString)
        {
            NumberFormatInfo nfi = getNfi();
            string[] fields = mString.Split(splitter.ToCharArray());
            return new Vector3(
                float.Parse(fields[0], nfi),
                float.Parse(fields[1], nfi),
                float.Parse(fields[2], nfi));
        }

        public static float FloatFromString(string mString)
        {
            NumberFormatInfo nfi = getNfi();
            return float.Parse(mString, nfi);
        }

        internal static int IntFromString(string mString)
        {
            NumberFormatInfo nfi = getNfi();
            return int.Parse(mString, nfi);
        }

        public static string StringFromFloat(float mFloat)
        {
            NumberFormatInfo nfi = getNfi();
            return mFloat.ToString(nfi);
        }

        private static byte[] StringToByteArray(string str)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetBytes(str);
        }

        private static string ByteArrayToString(byte[] arr)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(arr);
        }

        // Convert an object to a string
        public static string ObjectToString(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return Convert.ToBase64String(ms.ToArray());
        }
        // Convert a string to an Object
        public static Object StringToObject(string mString)
        {
            byte[] arrBytes = Convert.FromBase64String(mString);
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        public static Matrix4 ToOpenTKMatrix(JMatrix matrix)
        {
            return new Matrix4(matrix.M11,
                               matrix.M12,
                               matrix.M13,
                               0.0f,
                            matrix.M21,
                            matrix.M22,
                            matrix.M23,
                            0.0f,
                            matrix.M31,
                            matrix.M32,
                            matrix.M33,
                            0.0f, 0.0f, 0.0f, 0.0f, 1.0f);
        }

        public static JMatrix FromOpenTKMatrix(Matrix4 matrix)
        {
            return new JMatrix(matrix.M11,
                                matrix.M12,
                                matrix.M13,
                                matrix.M21,
                                matrix.M22,
                                matrix.M23,
                                matrix.M31,
                                matrix.M32,
                                matrix.M33);
        }
        /// <summary>
        /// Values takes 0 to 1
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(Color c)
        {
            Vector3 v = new Vector3();
            v.X = c.R / 255;
            v.X = c.G / 255;
            v.X = c.B / 255;

            return v;
        }

        public static Matrix4 BlendMatrix(Matrix4 matA, Matrix4 matB, float weight)
        {
            Matrix4 matR = new Matrix4();

            matR.M11 = matA.M11 * weight + matB.M11 * (1 - weight);
            matR.M12 = matA.M12 * weight + matB.M12 * (1 - weight);
            matR.M13 = matA.M13 * weight + matB.M13 * (1 - weight);
            matR.M14 = matA.M14 * weight + matB.M14 * (1 - weight);

            matR.M21 = matA.M21 * weight + matB.M21 * (1 - weight);
            matR.M22 = matA.M22 * weight + matB.M22 * (1 - weight);
            matR.M23 = matA.M23 * weight + matB.M23 * (1 - weight);
            matR.M24 = matA.M24 * weight + matB.M24 * (1 - weight);

            matR.M31 = matA.M31 * weight + matB.M31 * (1 - weight);
            matR.M32 = matA.M32 * weight + matB.M32 * (1 - weight);
            matR.M33 = matA.M33 * weight + matB.M33 * (1 - weight);
            matR.M34 = matA.M34 * weight + matB.M34 * (1 - weight);

            matR.M41 = matA.M41 * weight + matB.M41 * (1 - weight);
            matR.M42 = matA.M42 * weight + matB.M42 * (1 - weight);
            matR.M43 = matA.M43 * weight + matB.M43 * (1 - weight);
            matR.M44 = matA.M44 * weight + matB.M44 * (1 - weight);

            return matR;
        }

        public static Vector4 Mult(Matrix4 m, Vector4 v)
        {
            Vector4 result = new Vector4();

            result.X = m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W;
            result.Y = m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W;
            result.Z = m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W;
            result.W = m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W;

            return result;
        }


        public static Vector4 Mult(Vector4 v, Matrix4 m)
        {
            Vector4 result = new Vector4();

            result.X = m.M11 * v.X + m.M21 * v.Y + m.M31 * v.Z + m.M41 * v.W;
            result.Y = m.M12 * v.X + m.M22 * v.Y + m.M32 * v.Z + m.M42 * v.W;
            result.Z = m.M13 * v.X + m.M23 * v.Y + m.M33 * v.Z + m.M43 * v.W;
            result.W = m.M14 * v.X + m.M24 * v.Y + m.M34 * v.Z + m.M44 * v.W;

            return result;
        }

        internal static Matrix4 MatrixFromVector(JVector normal)
        {
            float rotationY = (float)Math.Atan2(normal.X, normal.Z);

            Matrix4 tmpMatA = Matrix4.Identity;
            //Console.WriteLine(rotationZ);
            if (normal.Y < 0.99 && normal.Y > -0.99)
                tmpMatA = Matrix4.CreateRotationY(rotationY);

            Vector4 secondaryVec = Mult(tmpMatA, new Vector4(ToOpenTKVector(normal), 1));
            float rotationX = (float)Math.Atan2(secondaryVec.Z, secondaryVec.Y);
            Matrix4 tmpMatB = Matrix4.CreateRotationX(rotationX);

            return Matrix4.Mult(tmpMatB, tmpMatA);
        }

        internal static Matrix4 MatrixFromVector(Vector3 normal)
        {
            float rotationY = (float)Math.Atan2(normal.X, normal.Z);

            Matrix4 tmpMatA = Matrix4.Identity;
            //Console.WriteLine(rotationZ);
            if (normal.Y < 0.99 && normal.Y > -0.99)
                tmpMatA = Matrix4.CreateRotationY(rotationY);

            Vector4 secondaryVec = Mult(tmpMatA, new Vector4(normal, 1));
            float rotationX = (float)Math.Atan2(secondaryVec.Z, secondaryVec.Y);
            Matrix4 tmpMatB = Matrix4.CreateRotationX(rotationX);

            return Matrix4.Mult(tmpMatB, tmpMatA);
        }

        public static string tabify(int amnt)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < amnt; i++)
            {
                sb.Append("\t");
            }
            return sb.ToString();
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value <= min)
                return min;

            if (value >= max)
                return max;

            return value;
        }

        internal static float[] FloatAryFromStringAry(string[] tmpAry)
        {
            int tmpLenth = tmpAry.Length;
            float[] floatAry = new float[tmpLenth];

            for (int i = 0; i < tmpLenth; i++)
            {
                floatAry[i] = GenericMethods.FloatFromString(tmpAry[i]);
            }
            return floatAry;
        }

        internal static List<Vector2> FlipY(List<Vector2> list)
        {
            List<Vector2> outVec = new List<Vector2> { };
            foreach (var vec in list)
            {
                outVec.Add(new Vector2(vec.X, 1.0f - vec.Y));
            }
            return outVec;
        }
    }

    

    /// <summary>
    /// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
    /// 
    /// Provides a method for performing a deep copy of an object.
    /// Binary Serialization is used to perform the copy.
    /// </summary>

    public static class ObjectCopier
    {
        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }

    public class Plane
    {
        public Vector3 normal;
        float d;
        public Vector3 center;

        public Plane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 inside)
        {
            Vector3 dir1 = v2 - v1;
            Vector3 dir2 = v3 - v1;
            normal = Vector3.Normalize(Vector3.Cross(dir1, dir2));

            //center = (v3 + v2) * 0.5f;

            d = Vector3.Dot(v1, normal);


            if (!check(inside))
            {
                normal *= -1.0f;
                d *= -1.0f;
            }
        }

        public bool check(Vector3 point)
        {
            return (Vector3.Dot(point, normal) > d);
        }

        public bool check(Vector3 point, float range)
        {
            return (Vector3.Dot(point, normal) + range > d);
        }
    }
}
