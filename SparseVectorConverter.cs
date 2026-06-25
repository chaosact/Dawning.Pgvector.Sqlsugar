using System.Data;
using SqlSugar;
using Pgvector;

namespace Dawning.Pgvector.SqlSugar;

public class SparseVectorConverter : ISugarDataConverter
{
    public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
    {

        if (columnValue is not SparseVector value)
        {
            throw new ArgumentException("columnValue must be a Pgvector.SparseVector");
        }

        return new SugarParameter($"@vector_p_{columnIndex}", null)
        {
            Value = value,
            DbType = System.Data.DbType.Object
        };
    }

    public T QueryConverter<T>(IDataRecord dataRecord, int dataRecordIndex)
    {
        var value = dataRecord.GetValue(dataRecordIndex);
        return (T)Convert.ChangeType(value, typeof(T));
    }
}
