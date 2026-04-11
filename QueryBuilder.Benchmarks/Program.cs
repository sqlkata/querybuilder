// BenchmarkDotNet: https://benchmarkdotnet.org/
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
