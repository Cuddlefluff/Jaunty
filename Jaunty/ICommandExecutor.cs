using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Jaunty
{
    /// <summary>
    /// Represents an object that can do database work
    /// </summary>
    public interface ICommandExecutor
    {
        // TODO: Add documentation.

        ISqlBuilder<T> GetBuilder<T>();

        bool Delete<T>(IDbConnection connection, T obj, IDbTransaction transaction = null);
        int Delete<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null);
        Task<bool> DeleteAsync<T>(IDbConnection connection, T obj, IDbTransaction transaction = null);
        Task<int> DeleteAsync<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null);
        bool DeleteById<T>(IDbConnection connection, object id, IDbTransaction transaction = null);
        int DeleteById<T>(IDbConnection connection, IEnumerable<object> coll, IDbTransaction transaction = null);
        Task<bool> DeleteByIdAsync<T>(IDbConnection connection, object id, IDbTransaction transaction = null);
        Task<int> DeleteByIdAsync<T>(IDbConnection connection, IEnumerable<object> ids, IDbTransaction transaction = null);
        T FindById<T>(IDbConnection connection, object id, IDbTransaction transaction = null);
        Task<T> FindByIdAsync<T>(IDbConnection connection, object id, IDbTransaction transaction = null);
        T Insert<T>(IDbConnection connection, T obj, IDbTransaction transaction = null);
        T Insert<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null);
        IEnumerable<T> Insert<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null);
        IEnumerable<T> Insert<T>(IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null);
        Task<T> InsertAsync<T>(IDbConnection connection, T obj, IDbTransaction transaction = null);
        Task<T> InsertAsync<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null);
        Task<IEnumerable<T>> InsertAsync<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null);
        Task<IEnumerable<T>> InsertAsync<T>(IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null);
        T Update<T>(IDbConnection connection, T obj, IDbTransaction transaction = null);
        T Update<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null);
        IEnumerable<T> Update<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null);
        IEnumerable<T> Update<T>(IDbConnection connection, IEnumerable<IMutator<T>> coll, IDbTransaction transaction = null);
        Task<T> UpdateAsync<T>(IDbConnection connection, T obj, IDbTransaction transaction = null);
        Task<T> UpdateAsync<T>(IDbConnection connection, IMutator<T> mutator, IDbTransaction transaction = null);
        Task<IEnumerable<T>> UpdateAsync<T>(IDbConnection connection, IEnumerable<T> coll, IDbTransaction transaction = null);
        Task<IEnumerable<T>> UpdateAsync<T>(IDbConnection connection, IEnumerable<IMutator<T>> mutators, IDbTransaction transaction = null);
    }
}