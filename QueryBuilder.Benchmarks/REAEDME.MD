# QueryBuilder.Benchmarks

This project is a benchmark suite for measuring the performance of the SqlKata query builder library. It uses [BenchmarkDotNet](https://benchmarkdotnet.org/) to provide performance metrics for various query-building scenarios.

## About

- **Purpose:** Evaluate the performance of different SqlKata query operations.
- **Framework:** [BenchmarkDotNet](https://benchmarkdotnet.org/) is used for running and reporting benchmarks.
- **Scope:** Includes benchmarks for a few select scenarios.

## How to Use

1. **Build the Solution:**
   Make sure the solution is built in Release mode for accurate results.

2. **Run the Benchmarks:**
   Execute the following command from the root of the repository:

   ```cmd
   dotnet run -c Release --project .\QueryBuilder.Benchmarks\QueryBuilder.Benchmarks.csproj
   ```

3. **Select Benchmarks:**
   After running the command, a benchmark selector will appear. This is powered by BenchmarkDotNet's `BenchmarkSwitcher`, allowing you to choose which benchmarks to execute interactively.

4. **View Results:**
   BenchmarkDotNet will output the results to the console and generate detailed reports in the `BenchmarkDotNet.Artifacts` directory.

## References
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
