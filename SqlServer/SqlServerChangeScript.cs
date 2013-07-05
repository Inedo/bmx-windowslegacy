using System;
using System.Data;
using System.Collections.Generic;
using System.Text;


using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.Database;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{
    [Serializable]
    public sealed class SqlServerChangeScript : ChangeScript
    {
        internal SqlServerChangeScript(DataRow dr)
            : base
            (
                (Int64)dr["Numeric_Release_Number"],
                (int)dr["Script_Id"],
                (string)dr["Batch_Name"],
                (DateTime)dr["Executed_Date"],
                ((string)dr["Success_Indicator"] == "Y")
            ) {}
    }
}
