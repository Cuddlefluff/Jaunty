
namespace Jaunty.SqlClient
{
    public class SqlClientCommandExecutor : CommandExecutor
    {
        public override ISqlBuilder<T> GetBuilder<T>()
        {
            return CommandExecutor<SqlClientSqlBuilder<T>, T>.Instance;
        }

        public static void Configure()
        {
            CommandExecutorExtensions.SetExecutor(new SqlClientCommandExecutor());
        }
    }
}
