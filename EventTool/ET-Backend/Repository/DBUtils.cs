using System.Data;

namespace ET_Backend.Repository;

public static class DbUtils
{
    public static IDbTransaction BeginSafeTransaction(this IDbConnection conn)
    {
        if (conn.State != ConnectionState.Open)
            conn.Open();
        return conn.BeginTransaction();
    }
}
