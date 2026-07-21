using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Tedd;

namespace TeddObjectFlattenerTests
{
    public class ObjectFlattenerTestsClass
    {
        public class PrimitiveModel
        {
            public string? StringValue { get; set; }
            public int IntValue { get; set; }
            public bool BoolValue { get; set; }
            public double DoubleValue { get; set; }
            public object? NullValue { get; set; }
        }

        public class ArrayModel
        {
            public int[]? Numbers { get; set; }
            public PrimitiveModel[]? Objects { get; set; }
        }

        public class NestedModel
        {
            public PrimitiveModel? Inner { get; set; }
        }

        public class CircularModel
        {
            public CircularModel? Self { get; set; }
        }

        [Fact]
        public void Flatten_NullObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFlattener.Flatten<object>(null!));
        }

        [Theory]
        [InlineData("test")]
        [InlineData("")]
        public void Flatten_String(string value)
        {
            var result = ObjectFlattener.Flatten(value);
            Assert.Equal(value, result[""]);
        }

        [Theory]
        [InlineData(123, "123")]
        [InlineData(-1, "-1")]
        [InlineData(0, "0")]
        public void Flatten_Number(int value, string expected)
        {
            var result = ObjectFlattener.Flatten(value);
            Assert.Equal(expected, result[""]);
        }

        [Theory]
        [InlineData(true, "true")]
        [InlineData(false, "false")]
        public void Flatten_Boolean(bool value, string expected)
        {
            var result = ObjectFlattener.Flatten(value);
            Assert.Equal(expected, result[""]);
        }

        [Fact]
        public void Flatten_PrimitiveModel()
        {
            var model = new PrimitiveModel
            {
                StringValue = "test",
                IntValue = 123,
                BoolValue = true,
                DoubleValue = 12.34,
                NullValue = null
            };

            var result = ObjectFlattener.Flatten(model);

            Assert.Equal("test", result["StringValue"]);
            Assert.Equal("123", result["IntValue"]);
            Assert.Equal("true", result["BoolValue"]);
            Assert.Equal("12.34", result["DoubleValue"]);
            Assert.Null(result["NullValue"]);
        }

        [Fact]
        public void Flatten_Array()
        {
            var model = new ArrayModel
            {
                Numbers = new[] { 1, 2, 3 },
                Objects = new[]
                {
                    new PrimitiveModel { IntValue = 10 },
                    new PrimitiveModel { IntValue = 20 }
                }
            };
            var result = ObjectFlattener.Flatten(model);

            Assert.Equal("1", result["Numbers:0"]);
            Assert.Equal("2", result["Numbers:1"]);
            Assert.Equal("3", result["Numbers:2"]);
            Assert.Equal("10", result["Objects:0:IntValue"]);
            Assert.Equal("20", result["Objects:1:IntValue"]);
        }

        [Fact]
        public void Flatten_NestedObject()
        {
            var model = new NestedModel
            {
                Inner = new PrimitiveModel { IntValue = 42 }
            };

            var result = ObjectFlattener.Flatten(model);

            Assert.Equal("42", result["Inner:IntValue"]);
        }

        [Fact]
        public void Flatten_InvalidJsonThrowsInvalidOperationException()
        {
            var circular = new CircularModel();
            circular.Self = circular;

            var ex = Assert.Throws<InvalidOperationException>(() => ObjectFlattener.Flatten(circular));
            Assert.IsType<JsonException>(ex.InnerException);
        }

        [Fact]
        public void Unflatten_NullDict_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectFlattener.Unflatten<PrimitiveModel>(null!));
        }

        [Fact]
        public void Unflatten_EmptyDict_ReturnsNull()
        {
            var dict = new Dictionary<string, string?>();
            var result = ObjectFlattener.Unflatten<PrimitiveModel>(dict);
            Assert.Null(result);
        }

        [Fact]
        public void Unflatten_PrimitiveModel()
        {
            var dict = new Dictionary<string, string?>
            {
                { "StringValue", "test" },
                { "IntValue", "123" },
                { "BoolValue", "true" },
                { "DoubleValue", "12.34" },
                { "NullValue", null }
            };

            var result = ObjectFlattener.Unflatten<PrimitiveModel>(dict);

            Assert.NotNull(result);
            Assert.Equal("test", result.StringValue);
            Assert.Equal(123, result.IntValue);
            Assert.True(result.BoolValue);
            Assert.Equal(12.34, result.DoubleValue);
            Assert.Null(result.NullValue);
        }

        [Fact]
        public void Unflatten_ArrayModel()
        {
            var dict = new Dictionary<string, string?>
            {
                { "Numbers:0", "1" },
                { "Numbers:1", "2" },
                { "Numbers:2", "3" },
                { "Objects:0:IntValue", "10" },
                { "Objects:1:IntValue", "20" }
            };

            var result = ObjectFlattener.Unflatten<ArrayModel>(dict);

            Assert.NotNull(result);
            Assert.NotNull(result.Numbers);
            Assert.Equal(3, result.Numbers.Length);
            Assert.Equal(1, result.Numbers[0]);
            Assert.Equal(2, result.Numbers[1]);
            Assert.Equal(3, result.Numbers[2]);

            Assert.NotNull(result.Objects);
            Assert.Equal(2, result.Objects.Length);
            Assert.Equal(10, result.Objects[0].IntValue);
            Assert.Equal(20, result.Objects[1].IntValue);
        }

        [Fact]
        public void Unflatten_NestedModel()
        {
            var dict = new Dictionary<string, string?>
            {
                { "Inner:IntValue", "42" }
            };

            var result = ObjectFlattener.Unflatten<NestedModel>(dict);

            Assert.NotNull(result);
            Assert.NotNull(result.Inner);
            Assert.Equal(42, result.Inner.IntValue);
        }

        [Fact]
        public void Unflatten_PathConflict_ObjectVsIndex()
        {
            var dict = new Dictionary<string, string?>
            {
                { "Prop:Child", "value1" },
                { "Prop:0", "value2" }
            };

            var ex = Assert.Throws<InvalidOperationException>(() => ObjectFlattener.Unflatten<object>(dict));
            Assert.Contains("Path conflict", ex.Message);
        }

        [Fact]
        public void Unflatten_PathConflict_ArrayVsProperty()
        {
            var dict = new Dictionary<string, string?>
            {
                { "Arr:0", "value1" },
                { "Arr:Child", "value2" }
            };

            var ex = Assert.Throws<InvalidOperationException>(() => ObjectFlattener.Unflatten<object>(dict));
            Assert.Contains("Path conflict", ex.Message);
        }

        [Theory]
        [InlineData("Arr:0:-1", "Invalid final index")]

        public void Unflatten_InvalidIndex_ThrowsInvalidOperationException(string key, string expectedErrorSubstring)
        {
            var dict = new Dictionary<string, string?>
            {
                { key, "value" }
            };

            var ex = Assert.Throws<InvalidOperationException>(() => ObjectFlattener.Unflatten<object>(dict));
            Assert.Contains(expectedErrorSubstring, ex.Message);
        }

        [Fact]
        public void Unflatten_NewtonsoftJsonException()
        {
            var dict = new Dictionary<string, string?>
            {
                { "IntValue", "not_a_number" }
            };

            var ex = Assert.Throws<InvalidOperationException>(() => ObjectFlattener.Unflatten<PrimitiveModel>(dict));
            Assert.Contains("Newtonsoft.Json failed to deserialize", ex.InnerException?.Message ?? ex.Message);
        }

        [Fact]
        public void PathComparer_NullChecks()
        {
            var t = typeof(ObjectFlattener).GetNestedType("PathComparer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var comparer = (IComparer<string>)Activator.CreateInstance(t!)!;

            Assert.Equal(0, comparer.Compare(null, null));
            Assert.Equal(-1, comparer.Compare(null, "a"));
            Assert.Equal(1, comparer.Compare("a", null));
        }

        [Fact]
        public void PathComparer_LengthDifference()
        {
            var t = typeof(ObjectFlattener).GetNestedType("PathComparer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var comparer = (IComparer<string>)Activator.CreateInstance(t!)!;

            Assert.Equal(-1, comparer.Compare("A", "A:B"));
            Assert.Equal(1, comparer.Compare("A:B", "A"));
        }
    }
}
