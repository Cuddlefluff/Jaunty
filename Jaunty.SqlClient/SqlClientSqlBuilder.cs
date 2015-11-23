using System;
using System.Linq;
using System.Collections.Generic;

namespace Jaunty.SqlClient
{

    public class SqlClientSqlBuilder<T> : SqlBuilder<T>
    {
        // This class will provide all the required SQL commands to do CRUD operations

        readonly string _insertString;
        readonly string _deleteString;
        readonly string _updateString;
        readonly string _findByIdstring;
        readonly string _outputString;

        private string EncloseObject(object input)
        {
            return string.Concat("[", input.ToString(), "]");
        }

        public SqlClientSqlBuilder() : base()
        {
            _outputString = InitOutputSql();

            _insertString = InitInsertSql();
            _deleteString = InitDeleteSql();
            _updateString = InitUpdateSql();
            _findByIdstring = InitFindByIdSql();
            
        }

        private string ObjectFullName
        {
            get
            {
                return string.Join(".", ObjectFullPath.Select(EncloseObject));
            }
        }

        private string SqlAs(DataProperty property)
        {
            return $"[{property}] AS [{property.Name}]";
        }
    

        private string SqlSet(DataProperty property)
        {
            return $"[{property}] = {property.GetParameterName()}";
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

        #region string caching for queries that aint never see no change
        private string InitInsertSql()
        {
            var properties = Properties.Where(p => !p.Flags.HasFlag(PropertyFlags.DatabaseGenerated)).ToList();

            if (properties.Count == 0)
            {
                return $"INSERT INTO {ObjectFullName} OUTPUT {_outputString}";
            }

            return $"INSERT INTO {ObjectFullName} ({SqlColumnList(properties)}) OUTPUT {_outputString} VALUES({SqlParameterList(properties)});";
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

            return $"UPDATE {ObjectFullName} SET {string.Join(", ", properties.Select(SqlSet))} OUTPUT {_outputString} WHERE {string.Join(" AND ", Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key)).Select(SqlSet))}";
        }

        private string InitFindByIdSql()
        {
            var keyProps = Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key));

            return $"SELECT {string.Join(", ", Properties.Select(SqlAs))} FROM {ObjectFullName} WHERE {string.Join(" AND ", keyProps.Select(SqlSet))}";
        }

        private string InitOutputSql()
        {
            var outputs = Properties;
            return string.Join(", ", outputs.Select(p => $"Inserted.[{p.ColumnName}] AS [{p.Name}]"));
        }

        #endregion

        public override string InsertString()
        {
            return _insertString;
        }

        public override string InsertString(IMutator<T> mutator)
        {
            var properties = mutator.Changeset.Select(p => Properties.Single(np => np.Name == p)).ToList();

            if(properties.Count == 0)
            {
                return $"INSERT INTO {ObjectFullName} OUTPUT {_outputString}";
            }

            return $"INSERT INTO {ObjectFullName} ({SqlColumnList(properties)}) OUTPUT {_outputString} VALUES({SqlParameterList(properties)});";

        }

        public override string DeleteString()
        {
            return _deleteString;
        }

        public override string UpdateString(IMutator<T> mutator)
        {
            // Get a list of modified properties from the mutator
            var properties = mutator.Changeset.Select(p => Properties.Single(np => np.Name == p)).ToList();

            if (properties.Count == 0)
                throw new InvalidOperationException("No properties changed");

            // Create an update statement for this changeset, so we don't send over any data that's unnecessary
            // or might otherwise impede performance because we're sending stuff like NVARCHAR(MAX), BINARY(MAX) or FILESTREAM even when it has no changes
            return $"UPDATE {ObjectFullName} SET {SqlSetList(properties)} OUTPUT {_outputString} WHERE {string.Join(" AND ", Properties.Where(p => p.Flags.HasFlag(PropertyFlags.Key)).Select(SqlSet))}";
        }


        public override string UpdateString()
        {
            // This once intentionally overwrites everything, so we don't need to specify anything special for it
            // the paramater obj is supplied just in case
            return _updateString;
        }

        public override string FindByIdString(object id)
        {
            return _findByIdstring;
        }

        protected override string InitSchemaName(Type t)
        {
            // In SQL Server, default to the schema dbo
            return base.InitSchemaName(t) ?? "dbo";
        }



    }
}
