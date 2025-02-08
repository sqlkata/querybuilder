// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using QueryBuilder.Benchmarks;

SelectsBenchmarkTests.TestAll();

BenchmarkRunner.Run<SelectsBenchmark>();
