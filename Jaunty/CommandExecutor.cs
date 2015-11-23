using System.Linq;
using System.Data;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jaunty
{
    /// <summary>
    /// Singleton which contains a instances of different executors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public static class CommandExecutor<T, U> where T : class, ISqlBuilder<U>, new()
    {
        private static T _instance; 

        /// <summary>
        /// Gets an instance of an SqlBuilder
        /// </summary>
        public static T Instance
        {
            get
            {
                return (_instance ?? (_instance = new T()));
            }
        }
    }


    /// <summary>
    /// Class representing commands that can be executed against the database.
    /// This is the class that database engine providers will inherit and register
    /// </summary>
    public abstract class CommandExecutor : ICommandExecutor
    {
        // This is the base class which glues SqlBuilder to Dapper.
        // Each method is comprised of :
        // Get builder for class
        // Get string from builder to perform query
        // Execute string with supplied data on Dapper

        /// <summary>
        /// Returns the builder for this class
        /// </summary>
        /// <typeparam name="T">The class type to target</typeparam>
        /// <returns></returns>
        public abstract ISqlBuilder<T> GetBuilder<T>();

        /// <summary>
        /// Inserts a row into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="obj"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T Insert<T>(IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var insertString = builder.InsertString();

            return connection.Query<T>(insertString, obj, transaction).Single();
        }

        /// <summary>
        /// Inserts a collection of rows into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<T> Insert<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var insertString = builder.InsertString();

            foreach(var item in coll)
            {
                yield return connection.Query<T>(insertString, item, transaction).Single();
            }

        }

        /// <summary>
        /// Inserts a row into the database, using a mutator which will make it only insert explicitly defined properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutator"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T Insert<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var insertString = builder.InsertString(mutator);

            return connection.Query<T>(insertString, mutator.Target, transaction).Single();
        }

        /// <summary>
        /// Inserts a collection of rows in the database, using mutators which will make it only insert explicitly defined properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutators"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<T> Insert<T>(IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            foreach(var mutator in mutators)
            {
                var insertString = builder.InsertString(mutator);

                yield return connection.Query<T>(insertString, mutator.Target, transaction).Single();
            }
            
        }

        /// <summary>
        /// Updates a database using the mutator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutator"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T Update<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var updateString = builder.UpdateString(mutator);

            return connection.Query<T>(updateString, mutator.Target, transaction).Single();
        }

        /// <summary>
        /// Overwrites a row in the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="obj"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T Update<T>(IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var updateString = builder.UpdateString();

            return connection.Query<T>(updateString, obj, transaction).Single();
        }

        /// <summary>
        /// Updates a collection of rows in the database using mutators
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<T> Update<T>(IDbConnection connection, IEnumerable<IMutator<T>> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            foreach(var obj in coll)
            {
                yield return connection.Query<T>(builder.UpdateString(obj), obj.Target, transaction).Single();
            }
        }

        /// <summary>
        /// Overwrites a collection of rows in the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public IEnumerable<T> Update<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            foreach(var item in coll)
            {
                yield return connection.Query<T>(builder.UpdateString(), item, transaction).Single();
            }

        }

        /// <summary>
        /// Deletes a row from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="obj"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool Delete<T>(IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return connection.Execute(deleteString, obj, transaction) > 0;
                
        }

        /// <summary>
        /// Deletes a collection of rows from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int Delete<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();
            var deleteString = builder.DeleteString();

            return connection.Execute(deleteString, coll, transaction);
        }

        /// <summary>
        /// Deletes a row in the database using the key or keys defined on the id object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool DeleteById<T>(IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return connection.Execute(deleteString, id, transaction) > 0;
        }

        /// <summary>
        /// Deletes a collection of rows from the database using the key or keys defined in the objects in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int DeleteById<T>(IDbConnection connection, IEnumerable<object> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return connection.Execute(deleteString, coll, transaction);
        }

        /// <summary>
        /// Returns a singular item from the database matching the specified keys on the id object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public T FindById<T>(IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var findString = builder.FindByIdString(id);

            return connection.Query<T>(findString, id, transaction).SingleOrDefault();
        }

        // Async

        /// <summary>
        /// As Insert, except async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="obj"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<T> InsertAsync<T>(IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var insertString = builder.InsertString();


            return (await connection.QueryAsync<T>(insertString, obj, transaction)).Single();
        }

        /// <summary>
        /// As Insert, except async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Task<IEnumerable<T>> InsertAsync<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();
            var insertString = builder.InsertString();

            return connection.QueryAsync<T>(insertString, coll, transaction);
        }

        /// <summary>
        /// As Insert, except async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutator"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<T> InsertAsync<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();
            var insertString = builder.InsertString(mutator);

            return (await connection.QueryAsync<T>(insertString, mutator.Target, transaction)).Single();
        }

        /// <summary>
        /// As Insert, except async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutators"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> InsertAsync<T>(IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null)
        {
            System.Collections.Concurrent.ConcurrentBag<T> results = new System.Collections.Concurrent.ConcurrentBag<T>();
            var builder = GetBuilder<T>();

            foreach(var mutator in mutators)
            {
                var insertString = builder.InsertString(mutator);

                results.Add((await connection.QueryAsync<T>(insertString, mutator.Target, transaction)).Single());
            }

            return results.AsEnumerable();
        }

        /// <summary>
        /// As Update, except async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutator"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<T> UpdateAsync<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var updateString = builder.UpdateString(mutator);

            return (await connection.QueryAsync<T>(updateString, mutator.Target, transaction)).Single();
        }

        /// <summary>
        /// As Update, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="obj"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<T> UpdateAsync<T>(IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();
            var updateString = builder.UpdateString();

            return (await connection.QueryAsync<T>(updateString, obj, transaction)).Single();
        }

        /// <summary>
        /// As Update, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="mutators"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> UpdateAsync<T>(IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null)
        {
            System.Collections.Concurrent.ConcurrentBag<T> results = new System.Collections.Concurrent.ConcurrentBag<T>();
            var builder = GetBuilder<T>();

            foreach(var mutator in mutators)
            {
                var updateString = builder.UpdateString(mutator);
                
                results.Add((await connection.QueryAsync<T>(updateString, mutator.Target, transaction)).Single()) ;
            }

            return results.AsEnumerable();
        }

        /// <summary>
        /// As Update, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> UpdateAsync<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            System.Collections.Concurrent.ConcurrentBag<T> results = new System.Collections.Concurrent.ConcurrentBag<T>();
            var builder = GetBuilder<T>();
            var updateString = builder.UpdateString();

            foreach(var obj in coll)
            {
                results.Add((await connection.QueryAsync<T>(updateString, obj, transaction)).Single());
            }

            return results.AsEnumerable();
        }

        /// <summary>
        /// As Delete, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="obj"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync<T>(IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return await connection.ExecuteAsync(deleteString, obj, transaction) > 0;
        }

        /// <summary>
        /// As Delete, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="coll"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Task<int> DeleteAsync<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return connection.ExecuteAsync(deleteString, coll, transaction);
        }

        /// <summary>
        /// As DeleteById, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<bool> DeleteByIdAsync<T>(IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return (await connection.ExecuteAsync(deleteString, id, transaction)) > 0;
        }

        /// <summary>
        /// As DeleteById, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="ids"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public Task<int> DeleteByIdAsync<T>(IDbConnection connection, IEnumerable<object> ids, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var deleteString = builder.DeleteString();

            return connection.ExecuteAsync(deleteString, ids, transaction);

        }

        /// <summary>
        /// As FindById, except Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<T> FindByIdAsync<T>(IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            var builder = GetBuilder<T>();

            var findByIdString = builder.FindByIdString(id);

            return (await connection.QueryAsync<T>(findByIdString, id, transaction)).SingleOrDefault();

        }
    }
}
