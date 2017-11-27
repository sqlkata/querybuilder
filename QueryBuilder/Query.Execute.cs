using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SqlKata.Compilers;

namespace SqlKata
{
    public partial class Query
    {
        public IDbConnection Connection;
        public Compiler Compiler;

        public Query(IDbConnection connection, Compiler compiler) : base()
        {

            this.Connection = connection;
            this.Compiler = compiler;

        }

        public Query(IDbConnection connection, Compiler compiler, string table) : base()
        {

            From(table);

            this.Connection = connection;
            this.Compiler = compiler;

        }

        public IEnumerable<T> Get<T>()
        {
            var result = this.Compiler.Compile(this);

            var list = this.Connection.Query<T>(result.Sql, result.Bindings);

            return list;
        }

        public IEnumerable<dynamic> Get()
        {
            return Get<dynamic>();
        }

        public async Task<IEnumerable<T>> GetAsync<T>()
        {
            var result = this.Compiler.Compile(this);

            var list = await this.Connection.QueryAsync<T>(result.Sql, result.Bindings);

            return list;
        }

        public PaginationResult<T> Paginate<T>(int page, int perPage = 25)
        {

            if (page < 1)
            {
                throw new ArgumentException("Page param should be greater than or equal to 1", nameof(page));
            }

            if (perPage < 1)
            {
                throw new ArgumentException("PerPage param should be greater than or equal to 1", nameof(perPage));
            }

            var count = this.Clone().Count();

            var list = this.ForPage(page, perPage).Get<T>();

            return new PaginationResult<T>
            {
                Query = this.Clone(),
                Page = page,
                PerPage = perPage,
                Count = count,
                List = list
            };

        }

        public PaginationResult<dynamic> Paginate(int page, int perPage = 25)
        {
            return Paginate<dynamic>(page, perPage);
        }

        public async Task<IEnumerable<dynamic>> GetAsync()
        {
            return await GetAsync<dynamic>();
        }

        public T FirstOrDefault<T>()
        {

            var result = this.Compiler.Compile(this);

            // Make sure to limit the query to one 1 record
            this.Limit(1);

            var item = this.Connection.QueryFirstOrDefault<T>(result.Sql, result.Bindings);

            return item;

        }

        public dynamic FirstOrDefault()
        {
            return FirstOrDefault<dynamic>();
        }

        public async Task<T> FirstOrDefaultAsync<T>()
        {

            var result = this.Compiler.Compile(this);

            // Make sure to limit the query to one 1 record
            this.Limit(1);

            var item = await this.Connection.QueryFirstOrDefaultAsync<T>(result.Sql, result.Bindings);

            return item;

        }

        public async Task<dynamic> FirstOrDefaultAsync()
        {
            return await FirstOrDefaultAsync<dynamic>();
        }

        public T First<T>()
        {

            var result = this.Compiler.Compile(this);

            // Make sure to limit the query to one 1 record
            this.Limit(1);

            var item = this.Connection.QueryFirst<T>(result.Sql, result.Bindings);

            return item;

        }

        public dynamic First()
        {
            return First<dynamic>();
        }


        public async Task<T> FirstAsync<T>()
        {

            var result = this.Compiler.Compile(this);

            // Make sure to limit the query to one 1 record
            this.Limit(1);

            var item = await this.Connection.QueryFirstAsync<T>(result.Sql, result.Bindings);

            return item;

        }

        public async Task<dynamic> FirstAsync()
        {
            return await FirstAsync<dynamic>();
        }

        public int Insert(Dictionary<string, object> data)
        {

            this.AsInsert(data);

            var result = this.Compiler.Compile(this);

            return this.Connection.Execute(result.Sql, result.Bindings);

        }

        public int Insert(IEnumerable<string> columns, Query query)
        {

            this.AsInsert(columns, query);

            var result = this.Compiler.Compile(this);

            return this.Connection.Execute(result.Sql, result.Bindings);

        }

        public async Task<int> InsertAsync(Dictionary<string, object> data)
        {

            this.AsInsert(data);

            var result = this.Compiler.Compile(this);

            return await this.Connection.ExecuteAsync(result.Sql, result.Bindings);

        }

        public async Task<int> InsertAsync(IEnumerable<string> columns, Query query)
        {

            this.AsInsert(columns, query);

            var result = this.Compiler.Compile(this);

            return await this.Connection.ExecuteAsync(result.Sql, result.Bindings);

        }

        public int Update(Dictionary<string, object> data)
        {

            this.AsUpdate(data);

            var result = this.Compiler.Compile(this);

            return this.Connection.Execute(result.Sql, result.Bindings);

        }

        public async Task<int> UpdateAsync(Dictionary<string, object> data)
        {

            this.AsUpdate(data);

            var result = this.Compiler.Compile(this);

            return await this.Connection.ExecuteAsync(result.Sql, result.Bindings);

        }

        public int Delete()
        {

            this.AsDelete();

            var result = this.Compiler.Compile(this);

            return this.Connection.Execute(result.Sql, result.Bindings);

        }

        public async Task<int> DeleteAsync()
        {

            this.AsDelete();

            var result = this.Compiler.Compile(this);

            return await this.Connection.ExecuteAsync(result.Sql, result.Bindings);

        }

        public int Count(params string[] columns)
        {

            var result = this.Compiler.Compile(this.AsCount(columns));

            var scalar = this.Connection.ExecuteScalar<int>(result.Sql, result.Bindings);

            return scalar;

        }

        public async Task<int> CountAsync(params string[] columns)
        {

            var result = this.Compiler.Compile(this.AsCount(columns));

            var scalar = await this.Connection.ExecuteScalarAsync<int>(result.Sql, result.Bindings);

            return scalar;

        }

        public long LongCount(params string[] columns)
        {

            var result = this.Compiler.Compile(this.AsCount(columns));

            var scalar = this.Connection.ExecuteScalar<long>(result.Sql, result.Bindings);

            return scalar;

        }

        public async Task<long> LongCountAsync(params string[] columns)
        {

            var result = this.Compiler.Compile(this.AsCount(columns));

            var scalar = await this.Connection.ExecuteScalarAsync<long>(result.Sql, result.Bindings);

            return scalar;

        }

        public double Average(string column)
        {

            var result = this.Compiler.Compile(this.AsAverage(column));

            var scalar = this.Connection.ExecuteScalar<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public async Task<double> AverageAsync(string column)
        {

            var result = this.Compiler.Compile(this.AsAverage(column));

            var scalar = await this.Connection.ExecuteScalarAsync<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public double Max(string column)
        {

            var result = this.Compiler.Compile(this.AsMax(column));

            var scalar = this.Connection.ExecuteScalar<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public async Task<double> MaxAsync(string column)
        {

            var result = this.Compiler.Compile(this.AsMax(column));

            var scalar = await this.Connection.ExecuteScalarAsync<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public double Min(string column)
        {

            var result = this.Compiler.Compile(this.AsMin(column));

            var scalar = this.Connection.ExecuteScalar<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public async Task<double> MinAsync(string column)
        {

            var result = this.Compiler.Compile(this.AsMin(column));

            var scalar = await this.Connection.ExecuteScalarAsync<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public double Sum(string column)
        {

            var result = this.Compiler.Compile(this.AsSum(column));

            var scalar = this.Connection.ExecuteScalar<double>(result.Sql, result.Bindings);

            return scalar;

        }

        public async Task<double> SumAsync(string column)
        {

            var result = this.Compiler.Compile(this.AsSum(column));

            var scalar = await this.Connection.ExecuteScalarAsync<double>(result.Sql, result.Bindings);

            return scalar;

        }


    }

}