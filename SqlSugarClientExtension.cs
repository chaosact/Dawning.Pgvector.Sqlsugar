using SqlSugar;
using Pgvector;

namespace Dawning.Pgvector.SqlSugar;

public static class SqlSugarClientExtension
{

    private static KeyValuePair<string, SugarParameter[]?> Identity(string sql, SugarParameter[]? pars) => new(sql, pars);

    public static void UsePgVector(
      this ISqlSugarClient client,
      Func<string, SugarParameter[]?, KeyValuePair<string, SugarParameter[]?>>? onExecutingChangeSql = null)
    {
        var finalOnExecutingChangeSql = onExecutingChangeSql ?? Identity;
        client.Aop.OnExecutingChangeSql = (sql, pars) =>
        {
            if (pars != null)
            {
                foreach (var sugarParameter in pars)
                {
                    if (sugarParameter.Value is Vector
                       || sugarParameter.Value is HalfVector
                       || sugarParameter.Value is SparseVector)
                    {
                        sugarParameter.DbType = System.Data.DbType.Object;
                    }
                }
            }

            return finalOnExecutingChangeSql(sql, pars);
        };

        var expMethods = new List<SqlFuncExternal>()
        {
          new()
          {
            UniqueMethodName = "L2Distance",
            MethodValue = CreateBinaryOperator("<->")
          },
          new()
          {
            UniqueMethodName = "CosineDistance",
            MethodValue = CreateBinaryOperator("<=>")
          },
          new()
          {
            UniqueMethodName = "InnerProduct",
            MethodValue = CreateBinaryOperator("<#>")
          }
        };

        client.CurrentConnectionConfig.ConfigureExternalServices.SqlFuncServices ??= new();
        client.CurrentConnectionConfig.ConfigureExternalServices.SqlFuncServices.AddRange(expMethods);
    }

    private static Func<MethodCallExpressionModel, DbType, ExpressionContext, string> CreateBinaryOperator(string op)
    {
        return (expInfo, dbType, _) =>
        {
            return dbType switch
            {
                DbType.PostgreSQL => $"({expInfo.Args[0].MemberName} {op} {expInfo.Args[1].MemberName})",
                _ => throw new NotSupportedException("Only PostgreSQL is supported.")
            };
        };
    }

    //private static float L2Distance(Vector one, Vector other)
    //{
    //    throw new NotImplementedException("This method is a stub, please use it in LINQ expression.");
    //}

    //private static float InnerProduct(Vector one, Vector other)
    //{
    //    throw new NotImplementedException("This method is a stub, please use it in LINQ expression.");
    //}

    //private static float CosineDistance(Vector one, Vector other)
    //{
    //    throw new NotImplementedException("This method is a stub, please use it in LINQ expression.");
    //}
}
