using System;
using CreamRoll.Queries;
using Xunit;

namespace CreamRoll.Tests {
    public class DynamicDictionaryTest {
        [Fact]
        public void AddValue() {
            dynamic dict = new DynamicDictionary();
            dict.myname = "chan";

            Assert.Equal("chan", dict.myname);

            dict.i = 1;
            dict.i++;
            Assert.Equal(2, dict.i);
        }
    }
}