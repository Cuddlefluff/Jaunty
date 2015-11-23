using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jaunty.Tests
{
    using PgSql;

    using DataAnnotations.Schema;
    using System.Collections.Generic;
    using System.Linq;

    [Database("Test"), Table("TestTable", Schema = "public")]
    public class TestTable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public Guid TheSecondId { get; set; }
        public string Name { get; set; }
        [Column("Alias")]
        public string Foo { get; set; }
        public DateTime TheDate { get; set; }
    }

    [TestClass]
    public class DevTests
    {
        private class DoTimer : IDisposable
        {
            readonly Action<TimeSpan> _onEnd;
            readonly System.Diagnostics.Stopwatch _sw;
            public DoTimer(Action<TimeSpan> onEnd)
            {
                _onEnd = onEnd;
                _sw = new System.Diagnostics.Stopwatch();
                _sw.Start();
            }

            public void Dispose()
            {
                _sw.Stop();

                _onEnd(_sw.Elapsed);
            }

        }

        private IDisposable Time(Action<TimeSpan> onEnd)
        {
            return new DoTimer(onEnd);
        }

        [TestMethod]
        public void ShowSqlOutput()
        {

            var sqlBuilder = new SqlClient.SqlClientSqlBuilder<TestTable>();
            var pgBuilder = new PgSql.PgSqlBuilder<TestTable>();

            Console.WriteLine(sqlBuilder.ObjectName);
            Console.WriteLine(pgBuilder.ObjectName);

            Console.WriteLine("Inserts");

            Console.WriteLine(sqlBuilder.InsertString());
            Console.WriteLine(pgBuilder.InsertString());

            var table = new TestTable();

            var mutator = table.SetProperties(new { Name = "Hallo", ApeShit = "Fjert" });

            Console.WriteLine("Updates");

            Console.WriteLine(sqlBuilder.UpdateString());
            Console.WriteLine(pgBuilder.UpdateString());
            
            Console.WriteLine(sqlBuilder.UpdateString(mutator));
            Console.WriteLine(pgBuilder.UpdateString(mutator));

            Console.WriteLine("Deletes");

            Console.WriteLine(sqlBuilder.DeleteString());
            Console.WriteLine(pgBuilder.DeleteString());

            Console.WriteLine("Find By Id");

            Console.WriteLine(sqlBuilder.FindByIdString(table));
            Console.WriteLine(pgBuilder.FindByIdString(table));

            





        }

        [TestMethod]
        public void PgSql()
        {
            PgSqlCommandExecutor.Configure();

            using (var connection = new Npgsql.NpgsqlConnection("host=localhost;username=postgres;password=Norther;database=Test"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {

                    var testData = new TestTable() {
                        Foo = "Foo",
                        Name = "Hello",
                        TheDate = DateTime.Now
                    };

                    TestTable result;

                    

                    using (Time(elapsed => Console.WriteLine("Insert : " + elapsed)))
                    {
                        result = connection.Insert(testData, transaction);
                    }

                    Console.WriteLine("Database generated UID : " + result.TheSecondId);

                    List<TestTable> bulkResult;

                    using (Time(elapsed => Console.WriteLine("Bulk insert : " + elapsed)))
                    {
                        bulkResult = connection.Insert(Enumerable.Range(0, 100).Select(i => new TestTable() { Name = i.ToString() }), transaction).ToList();

                        Assert.AreEqual(100, bulkResult.Count());
                    }


                    using (Time(elapsed => Console.WriteLine("Bulk update : " + elapsed)))
                    {
                        bulkResult = connection.Update(
                            bulkResult.Select(item => item.SetProperties(new TestTable() { Name = item.Name + " strongly typed mutation" })),
                            transaction).ToList();
                    }

                    using (Time(elapsed => Console.WriteLine("Update : " + elapsed)))
                    {
                        var newDate = DateTime.Now.AddYears(-50);
                        var update = connection.Update(
                                result.SetProperties(
                                    new { TheDate = newDate }
                                ),
                                transaction
                            );
                        Assert.AreEqual(newDate.Year, update.TheDate.Year);
                    }


                    using (Time(elapsed => Console.WriteLine("FindById : " + elapsed)))
                    {
                        Assert.IsNotNull(connection.FindById<TestTable>(new { result.Id, result.TheSecondId }, transaction));
                    }

                    transaction.Commit();
                }

            }



        }
    }
}
