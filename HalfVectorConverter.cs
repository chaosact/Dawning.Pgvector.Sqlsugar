using System.Data;
using SqlSugar;

namespace Dawning.Pgvector.SqlSugar;

public class HalfVectorConverter : ISugarDataConverter
{
    public SugarParameter ParameterConverter<T>(object columnValue, int columnIndex)
    {

        if (columnValue is not Pgvector.HalfVector value)
        {
            throw new ArgumentException("columnValue must be a Pgvector.HalfVector");
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
