using AutoDto.SourceGen;
using AutoDto.SourceGen.Debounce;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.Debouncer;

public class DebouncerTest : BaseUnitTest
{
    private class DebouncerData
    {
        public int Count { get; set; }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200)]
    public void TestDebouncerWithZeroPeriod(int ms)
    {
        int i = 0;
        void ExecuteAction(DebouncerData data)
        {
            Thread.Sleep(100);
            //data.Count++;
            Interlocked.Increment(ref i);
        }

        var d = new DebouncerData();
        var debouncer = new Debouncer<DebouncerData>(ExecuteAction, TimeSpan.FromMilliseconds(ms), false, false);

        Parallel.For(0, 3, _ => 
        {
            debouncer.RunAction(d).Wait();
        });

        Assert.Equal(3, i);
    }

    [Fact]
    public void TestDebouncerWith300Period()
    {
        int i = 0;
        void ExecuteAction(DebouncerData data)
        {
            Thread.Sleep(100);
            Interlocked.Increment(ref i);
        }

        var d = new DebouncerData();
        var debouncer = new Debouncer<DebouncerData>(ExecuteAction, TimeSpan.FromMilliseconds(300), false, false);

        Parallel.For(0, 3, _ =>
        {
            debouncer.RunAction(d).Wait();
        });

        Assert.Equal(1, i);
    }

    //[Fact]
    //public void TestDebouncerWith1000Period()
    //{
    //    int i = 0;
    //    void ExecuteAction(DebouncerData data)
    //    {
    //        Interlocked.Increment(ref i);
    //    }

    //    var d = new DebouncerData();
    //    var debouncer = new Debouncer<DebouncerData>(ExecuteAction, TimeSpan.FromMilliseconds(1000), new LoggerMock(), false);

    //    Parallel.For(0, 5, _ =>
    //    {
    //        Thread.Sleep(10 * i);
    //        debouncer.RunAction(d);
    //    });

    //    Assert.Equal(1, i);
    //}
}
