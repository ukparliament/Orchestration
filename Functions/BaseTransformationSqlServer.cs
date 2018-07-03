using System;
using System.Data;
using System.Data.SqlClient;

namespace Functions
{
    public class BaseTransformationSqlServer<T, K> : BaseTransformation<T, K>
        where T : ITransformationSettings, new()
        where K : DataSet

    {
        protected string connectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_InterimSqlServer", EnvironmentVariableTarget.Process);

        public override K GetSource(string dataUrl, T settings)
        {
            string url = settings.ParameterizedString(dataUrl);

            DataSet source = new DataSet();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = url;
                        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command))
                        {
                            sqlDataAdapter.FillLoadOption = LoadOption.OverwriteChanges;
                            sqlDataAdapter.Fill(source);
                        }
                    }
                }
                if ((source.Tables == null) || (source.Tables.Count == 0) || (source.Tables[0].Rows.Count == 0))
                    return null;
            }
            catch (Exception e)
            {
                logger.Exception(e);
                return null;
            }
            return source as K;
        }

        protected Uri GiveMeUri(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            else
                if (Uri.TryCreate($"{idNamespace}{id}", UriKind.Absolute, out Uri uri))
                    return uri;
                else
                {
                    logger.Warning($"Invalid url '{id}' found");
                    return null;
                }
        }

        protected string GetText(object dataValue)
        {
            if ((dataValue == null) || (dataValue is DBNull))
                return null;
            else
                return dataValue.ToString();
        }
    }
}
