using static SqlKata.Expressions;
using SqlKata.Compilers;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests
{
    /// <summary>
    /// If you want to test this queries against a database use NorthWind database
    /// </summary>
    public class DefineTest : TestSupport
    {
        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Products] WHERE [ProductName] = 'Anto'")]
        public void Test_Define_Where(string engine, string sqlText)
        {
            var query = new Query("Products")
                .Define("@name", "Anto")
                .Where("ProductName", Variable("@name"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT * FROM [Products] WHERE [unitprice] > (SELECT AVG([unitprice]) AS [avg] FROM [Products] WHERE [UnitsInStock] > 10) AND [UnitsOnOrder] > 5")]
        public void Test_Define_SubQuery(string engine, string sqlText)
        {
            var subquery = new Query("Products")
                .AsAverage("unitprice")
                .Define("@UnitsInSt", 10)
                .Where("UnitsInStock", ">", Variable("@UnitsInSt"));

            var query = new Query("Products")
                .Where("unitprice", ">", subquery)
                .Where("UnitsOnOrder", ">", 5);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }


        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [ProductName] FROM [Products] WHERE LOWER([ProductName]) like '%coffee'",
            false)]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [ProductName] FROM [Products] WHERE [ProductName] like '%Coffee'",
            true)]
        public void Test_Define_WhereEnds(string engine, string sqlText, bool caseSensitive)
        {
            var query = new Query("Products")
                .Select("ProductId", "ProductName")
                .Define("@product", "Coffee")
                .WhereEnds("ProductName", Variable("@product"), caseSensitive);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }


        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '12%'",
            false)]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '12%'",
            true)]
        public void Test_Define_WhereStarts(string engine, string sqlText, bool caseSensitive)
        {
            var query = new Query("Products")
                .Select("ProductId", "QuantityPerUnit")
                .Define("@perUnit", "12")
                .WhereStarts("QuantityPerUnit", Variable("@perUnit"), caseSensitive);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE LOWER([QuantityPerUnit]) like '%500%'",
            false)]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [QuantityPerUnit] FROM [Products] WHERE [QuantityPerUnit] like '%500%'",
            true)]
        public void Test_Define_WhereContains(string engine, string sqlText, bool caseSensitive)
        {
            var query = new Query("Products")
                .Define("@perUnit", "500")
                .Select("ProductId", "QuantityPerUnit")
                .WhereContains("QuantityPerUnit", Variable("@perUnit"), caseSensitive);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE LOWER([SupplierID]) like '20'",
            false)]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ProductId], [ProductName], [SupplierID] FROM [Products] WHERE [SupplierID] like '20'",
            true)]
        public void Test_Define_WhereLike(string engine, string sqlText, bool caseSensitive)
        {
            var query = new Query("Products")
                .Select("ProductId", "ProductName", "SupplierID")
                .Define("@id", "20")
                .WhereLike("SupplierID", Variable("@id"), caseSensitive);

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(
            EngineCodes.SqlServer,
            "SELECT [ShipperID], [CompanyName] FROM [Shippers] WHERE [ShipperID] IN (SELECT [ShipVia] FROM [Orders] WHERE [ShipVia] = 3)")]
        public void Test_Define_WhereInSubquery(string engine, string sqlText)
        {
            var subquery = new Query("Orders")
                .Define("@shipId", 3)
                .Select("ShipVia").Where("ShipVia", Variable("@shipId"));


            var query = new Query("Shippers")
                .Select("ShipperID", "CompanyName")
                .WhereIn("ShipperID", subquery);


            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Table] HAVING [Id] = 1")]
        public void Test_Define_Having(string engine, string sqlText)
        {
            var query = new Query("Table")
                .Define("@foo", 1)
                .Having("Id", "=", Variable("@foo"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory(Skip = "Not implemented")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT [Employees].[LastName], COUNT(Orders.OrderID) AS NumberOfOrders FROM [Orders] \nINNER JOIN [Employees] ON [Employees].[EmployeeID] = [Orders].[EmployeeID] GROUP BY [LastName] HAVING COUNT(Orders.OrderID) > 80")]
        public void Test_Define_HavingRaw(string engine, string sqlText)
        {
            var query = new Query("Orders")
                .Define("@count", 80)
                .Select("Employees.LastName")
                .SelectRaw("COUNT(Orders.OrderID) AS NumberOfOrders")
                .Join("Employees", "Employees.EmployeeID", "Orders.EmployeeID")
                .GroupBy("LastName")
                .HavingRaw("COUNT(Orders.OrderID) > @count");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like 'u%'")]
        public void Test_Define_HavingStarts(string engine, string sqlText)
        {
            var query = new Query("Customers")
                .Define("@label", "U")
                .SelectRaw("COUNT(CustomerID)")
                .Select("Country")
                .GroupBy("Country")
                .HavingStarts("Country", Variable("@label"));

            var result = CompileFor(engine, query);

            Assert.Equal(
                sqlText,
                result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d'")]
        public void Test_Define_Having_Ends(string engine, string sqlText)
        {
            var query = new Query("Customers")
                .Define("@label", "d")
                .SelectRaw("COUNT(CustomerID)")
                .Select("Country")
                .GroupBy("Country")
                .HavingEnds("Country", Variable("@label"));

            var result = CompileFor(engine, query);

            Assert.Equal(
                sqlText,
                result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT COUNT(CustomerID), [Country] FROM [Customers] GROUP BY [Country] HAVING LOWER([Country]) like '%d%'")]
        public void Test_Define_Having_Contains(string engine, string sqlText)
        {
            var query = new Query("Customers")
                .Define("@label", "d")
                .SelectRaw("COUNT(CustomerID)")
                .Select("Country")
                .GroupBy("Country")
                .HavingContains("Country", Variable("@label"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT COUNT(*) AS [count] FROM [Orders] WHERE ([ShipRegion] != NULL)")]
        public void Test_Define_NestedCondition(string engine, string sqlText)
        {
            var query = new Query("Orders")
                .Define("@shipReg", null)
                .Define("@one", 1)
                .Where(q =>
                        q.Where("ShipRegion", "!=", Variable("@shipReg"))
                    //    .WhereRaw("1 = @one")
                ).AsCount();

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }


        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Orders] WHERE CAST([RequiredDate] AS DATE) = '1996-08-01'")]
        [InlineData(EngineCodes.PostgreSql, "SELECT * FROM \"Orders\" WHERE \"RequiredDate\"::date = '1996-08-01'")]
        [InlineData(EngineCodes.Sqlite,
            "SELECT * FROM \"Orders\" WHERE strftime('%Y-%m-%d', \"RequiredDate\") = cast('1996-08-01' as text)")]
        [InlineData(EngineCodes.Firebird,
            "SELECT * FROM \"ORDERS\" WHERE CAST(\"REQUIREDDATE\" as DATE) = '1996-08-01'")]
        public void Test_Define_WhereDate(string engine, string sqlText)
        {
            var dateObj = new System.DateTime(year: 1996, month: 8, day: 1);

            var query = new Query("Orders")
                .Define("@d", dateObj)
                .WhereDate("RequiredDate", Variable("@d"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Orders] WHERE DATEPART(YEAR, [RequiredDate]) = 1996")]
        public void Test_Define_WhereDatePart(string engine, string sqlText)
        {
            var query = new Query("Orders")
                .Define("@d", 1996)
                .WhereDatePart("year", "RequiredDate", "=", Variable("@d"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer, "SELECT * FROM [Orders] WHERE CAST([RequiredDate] AS TIME) != '00:00:00'")]
        public void Test_Define_WhereTime(string engine, string sqlText)
        {
            var query = new Query("Orders")
                .Define("@d", "00:00:00")
                .WhereTime("RequiredDate", "!=", Variable("@d"));

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT * FROM [Customers] WHERE EXISTS (SELECT 1 FROM [Orders] WHERE [ShipPostalCode] = '8200')")]
        public void Test_Define_WhereExists(string engine, string sqlText)
        {
            var query = new Query("Customers")
                .WhereExists(q => q.From("Orders")
                    .Define("@postal", "8200")
                    .Where("ShipPostalCode", Variable("@postal"))
                );

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "WITH [prodCTE] AS (SELECT [Categories].[CategoryName], [Products].[UnitPrice] FROM [Products] \nINNER JOIN [Categories] ON [Categories].[CategoryID] = [Products].[CategoryID] WHERE [Products].[UnitPrice] > 10)\nSELECT * FROM [prodCTE]")]
        public void Test_Define_With(string engine, string sqlText)
        {
            var query = new Query("Products")
                .Define("@unit", 10)
                .Join("Categories", "Categories.CategoryID", "Products.CategoryID")
                .Select("Categories.CategoryName", "Products.UnitPrice")
                .Where("Products.UnitPrice", ">", Variable("@unit"));

            var queryCTe = new Query("prodCTE")
                .With("prodCTE", query);

            var result = CompileFor(engine, queryCTe);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory(Skip = "not implemented")]
        [InlineData(EngineCodes.SqlServer,
            "WITH [prodCTE] AS (SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > 10 AND  2 = 2)\nSELECT [CategoryName], [UnitPrice] FROM [prodCTE]")]
        public void Test_Define_WithRaw(string engine, string sqlText)
        {
            //WithRaw
            var query = new Query("prodCTE")
                .Define("@unit", 10)
                .Define("@foo", 2)
                .Select("CategoryName", "UnitPrice")
                .WithRaw("prodCTE",
                    "SELECT c.CategoryName, p.UnitPrice FROM Products p INNER JOIN Categories c ON c.CategoryID = p.CategoryID WHERE p.UnitPrice > @unit AND  2 = @foo");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') UNION SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'")]
        public void Test_Define_Union(string engine, string sqlText)
        {
            var q1 = new Query("Suppliers")
                .Define("@foo", "Beirut")
                .Select("City")
                .Where("City", Variable("@foo"));

            var q2 = new Query("Customers")
                .Define("@city", "Z")
                .Select("City")
                .Union(q1)
                .WhereNotLike("City", Variable("@city"));

            var result = CompileFor(engine, q2);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') EXCEPT SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'")]
        public void Test_Define_Except(string engine, string sqlText)
        {
            var q1 = new Query("Suppliers")
                .Define("@foo", "Beirut")
                .Select("City")
                .Where("City", Variable("@foo"));

            var q2 = new Query("Customers")
                .Define("@city", "Z")
                .Select("City")
                .Except(q1)
                .WhereNotLike("City", Variable("@city"));

            var result = CompileFor(engine, q2);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory]
        [InlineData(EngineCodes.SqlServer,
            "SELECT [City] FROM [Customers] WHERE NOT (LOWER([City]) like 'z') INTERSECT SELECT [City] FROM [Suppliers] WHERE [City] = 'Beirut'")]
        public void Test_Define_Intersect(string engine, string sqlText)
        {
            var q1 = new Query("Suppliers")
                .Define("@foo", "Beirut")
                .Select("City")
                .Where("City", Variable("@foo"));

            var q2 = new Query("Customers")
                .Define("@city", "Z")
                .Select("City")
                .Intersect(q1)
                .WhereNotLike("City", Variable("@city"));

            var result = CompileFor(engine, q2);

            Assert.Equal(sqlText, result.ToString());
        }

        [Theory(Skip = "not implemented")]
        [InlineData(EngineCodes.SqlServer,
            "SELECT [City] FROM [Customers] UNION ALL SELECT City FROM Suppliers WHERE 1 = 1 AND 2 = 2")]
        public void Test_Define_CombineRaw(string engine, string sqlText)
        {
            var query = new Query("Customers")
                .Define("@foo", 1)
                .Define("@faa", 2)
                .Select("City")
                .CombineRaw("UNION ALL SELECT City FROM Suppliers WHERE 1 = @foo AND 2 = @faa");

            var result = CompileFor(engine, query);

            Assert.Equal(sqlText, result.ToString());
        }
    }
}
