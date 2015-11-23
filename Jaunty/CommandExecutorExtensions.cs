using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Jaunty
{
    /// <summary>
    /// Provides the static extensions methods that can be used on an IDbConnection object instance
    /// </summary>
    public static class CommandExecutorExtensions
    {
        private static ICommandExecutor _executor;

        /// <summary>
        /// Sets the current executor
        /// </summary>
        /// <param name="executor"></param>
        public static void SetExecutor(ICommandExecutor executor)
        {
            _executor = executor;
        }

        static CommandExecutorExtensions()
        {

            var provider = System.Configuration.ConfigurationManager.AppSettings["Jaunty:Provider"];

            if(!string.IsNullOrEmpty(provider))
            {
                var executor = System.Type.GetType(provider, true);

                _executor = System.Activator.CreateInstance(executor) as ICommandExecutor;
            }

        }

        private static void CheckExecutor()
        {
            if(_executor == null)
            {
                throw new System.InvalidOperationException("You must select a provider. Example : SqlClientCommandExecutor.Configure()");
            }
        }

        /// <summary>
        /// Inserts an item into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Object to insert</param>
        /// <param name="transaction"></param>
        /// <returns>Inserted row from the database</returns>
        public static T Insert<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Insert(connection, obj, transaction);
        }

        /// <summary>
        /// Inserts all the objects in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection of items to insert to the database</param>
        /// <param name="transaction"></param>
        /// <returns>An enumeration of all the rows inserted into the database</returns>
        public static IEnumerable<T> Insert<T>(this IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Insert(connection, coll, transaction);
        }

        /// <summary>
        /// Inserts an object to the database with only the properties that are explicitly set with the mutator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="mutator">Mutator containing the properties to set</param>
        /// <param name="transaction"></param>
        /// <returns>The row created in the database</returns>
        public static T Insert<T>(this IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Insert(connection, mutator, transaction);
        }


        /// <summary>
        /// Inserts a collection of items into the database, based on properties explicitly set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="mutators">Mutators to use for insertion</param>
        /// <param name="transaction"></param>
        /// <returns>Inserted row from the database</returns>
        public static IEnumerable<T> Insert<T>(this IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Insert(connection, mutators, transaction);
        }

        /// <summary>
        /// Deletes a row from the database represented by the keys on the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Object to delete</param>
        /// <param name="transaction"></param>
        /// <returns>True if the row could be found, false otherwise</returns>
        public static bool Delete<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Delete(connection, obj, transaction);
        }

        /// <summary>
        /// Deletes all the objects in the collection from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection containing all the objects to delete</param>
        /// <param name="transaction"></param>
        /// <returns>How many rows were deleted</returns>
        public static int Delete<T>(this IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Delete(connection, coll, transaction);
        }

        /// <summary>
        /// Updates the database with the modified properties reported by the mutator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="mutator">Mutator which contains the changeset for the object</param>
        /// <param name="transaction"></param>
        /// <returns>An updated version of the object from the database</returns>
        public static T Update<T>(this IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Update(connection, mutator, transaction);
        }

        /// <summary>
        /// Deletes a row from the database represented by the keys on the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="id">Object to delete</param>
        /// <param name="transaction"></param>
        /// <returns>True if the row could be found, false otherwise</returns>
        public static bool DeleteById<T>(this IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.DeleteById<T>(connection, id, transaction);
        }

        /// <summary>
        /// Deletes all the objects in the collection from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection containing all the objects to delete</param>
        /// <param name="transaction"></param>
        /// <returns>How many rows were deleted</returns>
        public static int DeleteById<T>(this IDbConnection connection, IEnumerable<object> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.DeleteById<T>(connection, coll, transaction);
        }

        /// <summary>
        /// Updates all the objects in the collection using the properties supplied by the mutator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection of mutators</param>
        /// <param name="transaction"></param>
        /// <returns>A collection of all the rows that was updated in the database</returns>
        public static IEnumerable<T> Update<T>(this IDbConnection connection, IEnumerable<IMutator<T>> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Update(connection, coll, transaction);
        }

        /// <summary>
        /// Overwrites all columns on the row in the database with the values supplied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Object to update</param>
        /// <param name="transaction"></param>
        /// <returns>The updated row from the database</returns>
        public static T Update<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Update(connection, obj, transaction);
        }

        /// <summary>
        /// Overwrites all columns on all the rows supplied
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection of objects to overwrite</param>
        /// <param name="transaction"></param>
        /// <returns>An enumeration of all the rows updated in the database</returns>
        public static IEnumerable<T> Update<T>(this IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.Update(connection, coll, transaction);
        }

        /// <summary>
        /// Retrieves an item by its key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="id">An object containing the keys to locate the row</param>
        /// <param name="transaction"></param>
        /// <returns>The row from the database</returns>
        public static T FindById<T>(this IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.FindById<T>(connection, id, transaction);
        }

        // Async

        /// <summary>
        /// Inserts an item into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Object to insert</param>
        /// <param name="transaction"></param>
        /// <returns>Inserted row from the database</returns>
        public static Task<T> InsertAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.InsertAsync<T>(connection, obj, transaction);
        }

        /// <summary>
        /// Inserts a collection of items into the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection to insert</param>
        /// <param name="transaction"></param>
        /// <returns>Inserted row from the database</returns>
        public static Task<IEnumerable<T>> InsertAsync<T>(this IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.InsertAsync(connection, coll, transaction);
        }

        /// <summary>
        /// Inserts an item into the database using a mutator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="mutator">Mutated object to insert</param>
        /// <param name="transaction"></param>
        /// <returns>Inserted row from the database</returns>
        public static Task<T> InsertAsync<T>(this IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.InsertAsync(connection, mutator, transaction);
        }

        /// <summary>
        /// Inserts a collection of items into the database, based on properties explicitly set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="mutators">Mutators to use for insertion</param>
        /// <param name="transaction"></param>
        /// <returns>Inserted row from the database</returns>
        public static Task<IEnumerable<T>> InsertAsync<T>(this IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction)
        {
            CheckExecutor();

            return _executor.InsertAsync(connection, mutators, transaction);
        }

        /// <summary>
        /// Updates the database with the modified properties reported by the mutator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Mutator which contains the changeset for the object</param>
        /// <param name="transaction"></param>
        /// <returns>An updated version of the object from the database</returns>
        public static Task<T> UpdateAsync<T>(this IDbConnection connection, IMutator<T> obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.UpdateAsync(connection, obj, transaction);
        }

        /// <summary>
        /// Updates the database with the modified properties reported by the mutators
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Mutator which contains the changeset for the object</param>
        /// <param name="transaction"></param>
        /// <returns>An enumeration of all the updated of the object from the database</returns>
        public static Task<IEnumerable<T>> UpdateAsync<T>(this IDbConnection connection, IEnumerable<IMutator<T>> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.UpdateAsync(connection, coll, transaction);
        }

        /// <summary>
        /// Ovewrites the row the database with the supplied object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Item to overwrite</param>
        /// <param name="transaction"></param>
        /// <returns>An updated version of the row from the database</returns>
        public static Task<T> UpdateAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.UpdateAsync(connection, obj, transaction);
        }

        /// <summary>
        /// Overwrites all the rows in the database that matches the list of objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection of objects to overwrite</param>
        /// <param name="transaction"></param>
        /// <returns>An enumeration of overwritten rows from the database</returns>
        public static Task<IEnumerable<T>> UpdateAsync<T>(this IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.UpdateAsync(connection, coll, transaction);
        }

        /// <summary>
        /// Deletes a row from the database represented by the keys on the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="obj">Object to delete</param>
        /// <param name="transaction"></param>
        /// <returns>True if the row could be found, false otherwise</returns>
        public static Task<bool> DeleteAsync<T>(this IDbConnection connection, T obj, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.DeleteAsync(connection, obj, transaction);
        }

        /// <summary>
        /// Deletes all rows that matches the set in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection of objects to delete</param>
        /// <param name="transaction"></param>
        /// <returns>Amount of rows deleted</returns>
        public static Task<int> DeleteAsync<T>(this IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.DeleteAsync(connection, coll, transaction);
        }

        /// <summary>
        /// Deletes a row from the database represented by the keys on the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="id">Object to delete</param>
        /// <param name="transaction"></param>
        /// <returns>True if the row could be found, false otherwise</returns>
        public static Task<bool> DeleteByIdAsync<T>(this IDbConnection connection, object id, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.DeleteByIdAsync<T>(connection, id, transaction);
        }

        /// <summary>
        /// Deletes all rows that matches the keys of the objects in the collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="coll">Collection of objects to delete</param>
        /// <param name="transaction"></param>
        /// <returns>Amount of rows deleted</returns>
        public static Task<int> DeleteByIdAsync<T>(this IDbConnection connection, IEnumerable<object> coll, IDbTransaction transaction = null)
        {
            CheckExecutor();

            return _executor.DeleteByIdAsync<T>(connection, coll, transaction);
        }
        

    }
}
