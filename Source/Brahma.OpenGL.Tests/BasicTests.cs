#region License and Copyright Notice

//Brahma 2.0: Framework for streaming/parallel computing with an emphasis on GPGPU

//Copyright (c) 2007 Ananth B.
//All rights reserved.

//The contents of this file are made available under the terms of the
//Eclipse Public License v1.0 (the "License") which accompanies this
//distribution, and is available at the following URL:
//http://www.opensource.org/licenses/eclipse-1.0.php

//Software distributed under the License is distributed on an "AS IS" basis,
//WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
//the specific language governing rights and limitations under the License.

//By using this software in any fashion, you are agreeing to be bound by the
//terms of the License.

#endregion

using System;
using System.Linq;

using Brahma.OpenGL.Tests.Helper;

using NUnit.Framework;

namespace Brahma.OpenGL.Tests
{
    [TestFixture]
    public sealed class BasicTests
    {
        private ComputationProvider _provider;

        [TestFixtureSetUp]
        public void Setup()
        {
            _provider = new ComputationProvider();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        [Test]
        public void IdentityTransform()
        {
            var random = new Random();

            #region 1D tests

            using (CompiledQuery floats = _provider.Compile<DataParallelArray<float>>(d => from value in d
                                                                                           select value))
            using (var data = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble()))
            {
                IQueryable result = _provider.Run(floats, data);

                int index = 0;
                foreach (float value in result)
                {
                    value.AssertIfClose(data[index]);
                    index++;
                }
            }

            using (CompiledQuery vector2s = _provider.Compile<DataParallelArray<Vector2>>(d => from value in d
                                                                                               select value))
            using (var data = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(),
                                                                                              (float)random.NextDouble())))
            {
                IQueryable result = _provider.Run(vector2s, data);

                int index = 0;
                foreach (Vector2 value in result)
                {
                    value.AssertIfClose(data[index]);
                    index++;
                }
            }

            using (CompiledQuery vector3s = _provider.Compile<DataParallelArray<Vector3>>(d => from value in d
                                                                                               select value))
            using (var data = new DataParallelArray<Vector3>(_provider, 256, x => new Vector3((float)random.NextDouble(),
                                                                                              (float)random.NextDouble(),
                                                                                              (float)random.NextDouble())))
            {
                IQueryable result = _provider.Run(vector3s, data);

                int index = 0;
                foreach (Vector3 value in result)
                {
                    value.AssertIfClose(data[index]);
                    index++;
                }
            }

            using (CompiledQuery vector4s = _provider.Compile<DataParallelArray<Vector4>>(d => from value in d
                                                                                               select value))
            using (var data = new DataParallelArray<Vector4>(_provider, 256, x => new Vector4((float)random.NextDouble(),
                                                                                              (float)random.NextDouble(),
                                                                                              (float)random.NextDouble(),
                                                                                              (float)random.NextDouble())))
            {
                IQueryable result = _provider.Run(vector4s, data);

                int index = 0;
                foreach (Vector4 value in result)
                {
                    value.AssertIfClose(data[index]);
                    index++;
                }
            }

            #endregion 1D tests

            #region 2D tests

            using (CompiledQuery floats = _provider.Compile<DataParallelArray2D<float>>(d => from value in d
                                                                                             select value))
            using (var data = new DataParallelArray2D<float>(_provider, 256, 256, (x, y) => (float)random.NextDouble()))
            {
                var result = _provider.Run(floats, data) as DataParallelArray2D<float>;

                Assert.IsNotNull(result);

                for (int x = 0; x < data.Width; x++)
                    for (int y = 0; y < data.Height; y++)
                        result[x, y].AssertIfClose(data[x, y]);
            }

            using (CompiledQuery vector2s = _provider.Compile<DataParallelArray2D<Vector2>>(d => from value in d
                                                                                                 select value))
            using (var data = new DataParallelArray2D<Vector2>(_provider, 256, 256, (x, y) => new Vector2((float)random.NextDouble(),
                                                                                                          (float)random.NextDouble())))
            {
                var result = _provider.Run(vector2s, data) as DataParallelArray2D<Vector2>;

                Assert.IsNotNull(result);

                for (int x = 0; x < data.Width; x++)
                    for (int y = 0; y < data.Height; y++)
                        result[x, y].AssertIfClose(data[x, y]);
            }

            using (CompiledQuery vector3s = _provider.Compile<DataParallelArray2D<Vector3>>(d => from value in d
                                                                                                 select value))
            using (var data = new DataParallelArray2D<Vector3>(_provider, 256, 256, (x, y) => new Vector3((float)random.NextDouble(),
                                                                                                          (float)random.NextDouble(),
                                                                                                          (float)random.NextDouble())))
            {
                var result = _provider.Run(vector3s, data) as DataParallelArray2D<Vector3>;

                Assert.IsNotNull(result);

                for (int x = 0; x < data.Width; x++)
                    for (int y = 0; y < data.Height; y++)
                        result[x, y].AssertIfClose(data[x, y]);
            }

            using (CompiledQuery vector4s = _provider.Compile<DataParallelArray2D<Vector4>>(d => from value in d
                                                                                                 select value))
            using (var data = new DataParallelArray2D<Vector4>(_provider, 256, 256, (x, y) => new Vector4((float)random.NextDouble(),
                                                                                                          (float)random.NextDouble(),
                                                                                                          (float)random.NextDouble(),
                                                                                                          (float)random.NextDouble())))
            {
                var result = _provider.Run(vector4s, data) as DataParallelArray2D<Vector4>;

                Assert.IsNotNull(result);

                for (int x = 0; x < data.Width; x++)
                    for (int y = 0; y < data.Height; y++)
                        result[x, y].AssertIfClose(data[x, y]);
            }

            #endregion
        }

        [Test]
        public void Indexing()
        {
            var random = new Random();

            CompiledQuery query = _provider.Compile<DataParallelArray<float>>(d => from float value in d
                                                                                   select (d[d.Current - 1] + d[d.Current] + d[d.Current + 1]) / 3f);

            var data = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());

            IQueryable result = _provider.Run(query, data);

            int i = 0;
            foreach (float value in result)
            {
                value.AssertIfClose((data[i - 1] + data[i] + data[i + 1]) / 3f);
                i++;
            }
        }

        [Test]
        public void IndexingMultiSelect()
        {
            var random = new Random();

            CompiledQuery query = _provider.Compile<DataParallelArray<float>,
                DataParallelArray<float>>((d1, d2) => from value1 in d1
                                                      from value2 in d2
                                                      let num = 2f
                                                      select (d1[d1.Current - 1] + d2[d2.Current - 1]) / num);

            var data1 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());
            var data2 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());

            IQueryable result = _provider.Run(query, data1, data2);

            int index = 0;
            foreach (float value in result)
            {
                value.AssertIfClose((data1[index - 1] + data2[index - 1]) / 2);
                index++;
            }
        }

        [Test]
        public void ResultSizeTest()
        {
            var random = new Random();

            CompiledQuery query = _provider.Compile<DataParallelArray2D<float>,
                DataParallelArray<float>>((d1, d2) => from value1 in d1
                                                      from value2 in d2
                                                      select value1 + value2);

            var data1 = new DataParallelArray2D<float>(_provider, 1, 256, (x, y) => (float)random.NextDouble());
            var data2 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());

            var result = _provider.Run(query, data1, data2) as DataParallelArray<float>;

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Length == 1);
            result[0].AssertIfClose(data1[0, 0] + data2[0]);
        }

        [Test]
        public void SelectMany()
        {
            var random = new Random();

            CompiledQuery q =
                _provider.Compile<DataParallelArray<float>,
                    DataParallelArray<float>,
                    DataParallelArray<float>>((d1, d2, d3) => from value1 in d1
                                                              from value2 in d2
                                                              from value3 in d3
                                                              select value1 + value2 + value3);

            var data1 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());
            var data2 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());
            var data3 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());

            IQueryable result = _provider.Run(q, data1, data2, data3);
            q.Dispose();

            int index = 0;
            foreach (float value in result)
            {
                value.AssertIfClose(data1[index] + data2[index] + data3[index]);
                index++;
            }
        }

        [Test]
        public void SelectManyWithLet()
        {
            var random = new Random();

            {
                CompiledQuery q =
                    _provider.Compile<DataParallelArray<float>,
                        DataParallelArray<float>,
                        DataParallelArray<float>>((d1, d2, d3) => from value1 in d1
                                                                  from value2 in d2
                                                                  from value3 in d3
                                                                  let sum = value1 + value2 + value3
                                                                  select sum / 3f);

                var data1 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());
                var data2 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());
                var data3 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble());

                IQueryable result = _provider.Run(q, data1, data2, data3);
                q.Dispose();

                int index = 0;
                foreach (float value in result)
                {
                    value.AssertIfClose((data1[index] + data2[index] + data3[index]) / 3);
                    index++;
                }
            }

            // This query doesn't "new" in the select
            {
                CompiledQuery q =
                    _provider.Compile<DataParallelArray<Vector2>,
                        DataParallelArray<Vector2>,
                        DataParallelArray<Vector2>>((d1, d2, d3) => from vec1 in d1
                                                                    from vec2 in d2
                                                                    from vec3 in d3
                                                                    let average = (vec1 + vec2 + vec3) / 3
                                                                    let avgLength = Math.Sqrt((average.x * average.x) + (average.y * average.y))
                                                                    select average / (float)avgLength);

                var data1 = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(), (float)random.NextDouble()));
                var data2 = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(), (float)random.NextDouble()));
                var data3 = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(), (float)random.NextDouble()));

                IQueryable result = _provider.Run(q, data1, data2, data3);
                q.Dispose();

                int index = 0;
                foreach (Vector2 value in result)
                {
                    Vector2 average = (data1[index] + data2[index] + data3[index]) / 3;
                    var avgLength = (float)Math.Sqrt((average.x * average.x) + (average.y * average.y));
                    value.AssertIfClose(average / avgLength);
                    index++;
                }
            }

            // This query has a NewExpression tucked into it, with a default constructor and member-inits
            {
                CompiledQuery q =
                    _provider.Compile<DataParallelArray<Vector2>,
                        DataParallelArray<Vector2>,
                        DataParallelArray<Vector2>>((d1, d2, d3) => from vec1 in d1
                                                                    from vec2 in d2
                                                                    from vec3 in d3
                                                                    let average = (vec1 + vec2 + vec3) / 3
                                                                    let avgLength = Math.Sqrt((average.x * average.x) + (average.y * average.y))
                                                                    select new Vector2
                                                                               {
                                                                                   x = (float)(average.x / avgLength),
                                                                                   y = (float)(average.y / avgLength)
                                                                               });

                var data1 = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(), (float)random.NextDouble()));
                var data2 = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(), (float)random.NextDouble()));
                var data3 = new DataParallelArray<Vector2>(_provider, 256, x => new Vector2((float)random.NextDouble(), (float)random.NextDouble()));

                IQueryable result = _provider.Run(q, data1, data2, data3);
                q.Dispose();

                int index = 0;
                foreach (Vector2 value in result)
                {
                    Vector2 average = (data1[index] + data2[index] + data3[index]) / 3;
                    var avgLength = (float)Math.Sqrt((average.x * average.x) + (average.y * average.y));
                    value.AssertIfClose(average / avgLength);
                    index++;
                }
            }
        }

        [Test]
        public void SimpleSelectMany()
        {
            var random = new Random();

            using (CompiledQuery simpleSelectMany = _provider.Compile<DataParallelArray<float>, DataParallelArray<float>>(
                (d1, d2) => from value1 in d1
                            from value2 in d2
                            select value1 + value2)
                )
            using (var data1 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble()))
            using (var data2 = new DataParallelArray<float>(_provider, 256, x => (float)random.NextDouble()))
            {
                IQueryable result = _provider.Run(simpleSelectMany, data1, data2);

                int index = 0;
                foreach (float value in result)
                {
                    value.AssertIfClose(data1[index] + data2[index]);
                    index++;
                }
            }
        }

        [Test]
        public void TestProvider()
        {
            Assert.AreNotEqual(_provider, null);
        }

        [Test]
        public void TwoDimensionalIndexing()
        {
            var random = new Random();

            CompiledQuery query = _provider.Compile<DataParallelArray2D<float>,
                DataParallelArray2D<float>>((d1, d2) => from value1 in d1
                                                        from value2 in d2
                                                        select d1[d1.CurrentX, d1.CurrentY] + d2[d2.CurrentX, d2.CurrentY]);

            var data1 = new DataParallelArray2D<float>(_provider, 256, 256, (x, y) => (float)random.NextDouble());
            var data2 = new DataParallelArray2D<float>(_provider, 256, 256, (x, y) => (float)random.NextDouble());

            var result = _provider.Run(query, data1, data2) as DataParallelArray2D<float>;

            Assert.IsNotNull(result);

            for (int x = 0; x < 255; x++)
                for (int y = 0; y < 255; y++)
                    result[x, y].AssertIfClose(data1[x, y] + data2[x, y]);
        }
    }
}