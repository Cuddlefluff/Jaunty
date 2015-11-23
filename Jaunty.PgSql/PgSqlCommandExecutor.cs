using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jaunty.PgSql
{
    public class PgSqlCommandExecutor : CommandExecutor
    {
        public override ISqlBuilder<T> GetBuilder<T>()
        {
            return CommandExecutor<PgSqlBuilder<T>, T>.Instance;
        }

        public static void Configure()
        {
            CommandExecutorExtensions.SetExecutor(new PgSqlCommandExecutor());
        }
    }
}
