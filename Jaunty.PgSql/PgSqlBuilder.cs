using System;
using System.Linq;
using System.Collections.Generic;

namespace Jaunty.PgSql
{
    public class PgSqlBuilder<T> : SqlBuilder<T>
    {
        readonly string _insertString;
        readonly string _deleteString;
        readonly string _updateString;
        readonly string _findByIdstring;
        readonly string _returningString;

        public PgSqlBuilder() : base()
        {
            _returningString = InitReturningString();

            _insertString = InitInsertSql();
            _deleteString = InitDeleteSql();
            _updateString = InitUpdateSql();
            _findByIdstring = InitFindByIdSql();
            
        }

        private string EncloseObject(object input)
        {
            return string.Concat("\"", input.ToString(), "\"");
        }

        private string SqlAs(DataProperty property)
        {
            return $"\"{property}\" AS \"{property.Name}\"";
        }


        private string SqlSet(DataProperty property)
        {
            return $"\"{property}\" = {property.GetParameterName()}";
        }

        private string SqlSetList(IEnumerable<DataProperty> properties)
        {
            return string.Join(",", properties.Select(SqlSet));
        }

        private string SqlParameter(DataProperty property)
        {
            return property.GetParameterName();
        }

        private string SqlColumn(DataProperty property)
        {
            return EncloseObject(property);
        }

        private string SqlColumnList(IEnumerable<DataProperty> properties)
        {
            return string.Join(", ", properties.Select(SqlColumn));
        }

        private string SqlParameterList(IEnumerable<DataProperty> properties)
        {
            return string.Join(", ", properties.Select(SqlParameter));
        }


        private string ObjectFullName
        {
            get
            {
                return string.Join(".", ObjectFullPath.Select(EncloseObject));
            }
        }

        protected override string InitDatabaseName(Type t)
        {
            // PostGreSQL does not support cross-database queries
            // So unless the database name is equal to that supplied in the connection
            // all queries are going to fail
            return base.InitDatabaseName(t);
        }

        protected override string InitSchemaName(Type t)
        {
            // default schema public
            return base.InitSchemaName(t) ?? "public";
        }

        #region string caching for queries that aint never see no change

        private string InitInsertSql()
        {
            var properties = Properties.Where(p => !p.Flags.HasFlag(PropertyFlags.DatabaseGenerated)).ToList();

            if (properties.Count == 0)
            {
                return $"INSERT INTO {ObjectFullName} RETURNING {_returningString}";
            }

            return $"INSERT INTO {ObjectFullName} ({SqlColumnList(properties)}) VALUES({SqlParameterList(properties)}) RETURNING {_returningString}";
        }

        private string InitDeleteSql()
        {
            // We need the properties that are specified as keys
            var properties = Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key)).ToList();

            if (properties.Count == 0)
                throw new InvalidOperationException("No keys defined on table");

            // The point of this CRUD is to work with singular entities and make that process simpler and more streamlined
            // if we need to delete by anything other than primary keys, well then just use dapper directly.
            return $"DELETE FROM {ObjectFullName} WHERE {string.Join(" AND ", Properties.Select(SqlSet))}";
        }

        private string InitUpdateSql()
        {
            var properties = Properties.Where(p => !p.Flags.HasFlag(PropertyFlags.DatabaseGenerated)).ToList();

            if (properties.Count == 0)
                throw new InvalidOperationException("No fields defined that can be updated");

            return $"UPDATE {ObjectFullName} SET {SqlSetList(properties)} WHERE {string.Join(" AND ", Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key)).Select(SqlSet))} RETURNING {_returningString}";
        }

        private string InitFindByIdSql()
        {
            var keyProps = Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key));

            return $"SELECT {string.Join(", ", Properties.Select(SqlAs))} FROM {ObjectFullName} WHERE {string.Join(" AND ", keyProps.Select(SqlSet))}";
        }

        private string InitReturningString()
        {
            var outputs = Properties;
            return string.Join(", ", outputs.Select(SqlAs));
        }

        #endregion

        public override string DeleteString()
        {
            return _deleteString;
        }

        public override string FindByIdString(object id)
        {
            return _findByIdstring;
        }

        public override string InsertString()
        {
            return _insertString;
        }

        public override string InsertString(IMutator<T> mutator)
        {
            var properties = mutator.Changeset.Select(p => Properties.Single(np => np.Name == p)).ToList();

            if (properties.Count == 0)
            {
                return $"INSERT INTO {ObjectFullName} RETURNING {_returningString}";
            }

            return $"INSERT INTO {ObjectFullName} ({SqlColumnList(properties)}) VALUES({SqlParameterList(properties)}) RETURNING {_returningString}";

        }


        public override string UpdateString()
        {
            return _updateString;
        }

        public override string UpdateString(IMutator<T> obj)
        {
            // Get a list of modified properties from the mutator
            var properties = obj.Changeset.Select(p => Properties.Single(np => np.Name == p)).ToList();

            if (properties.Count == 0)
                throw new InvalidOperationException("No properties changed");

            // Create an update statement for this changeset, so we don't send over any data that's unnecessary
            // or might otherwise impede performance because we're sending stuff like NVARCHAR(MAX), BINARY(MAX) or FILESTREAM even when it has no changes
            return $"UPDATE {ObjectFullName} SET {SqlSetList(properties)} WHERE {string.Join(" AND ", Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key)).Select(SqlSet))} RETURNING {_returningString}";
        }
    }
}
