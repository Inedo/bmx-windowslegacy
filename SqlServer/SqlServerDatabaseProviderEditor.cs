using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Windows.SqlServer
{
    public sealed class SqlServerDatabaseProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtConnectionString;

        protected override void CreateChildControls()
        {
            this.txtConnectionString = new ValidatingTextBox()
            {
                Required = true,
                Width = 300
            };

            Controls.Add(
                new FormFieldGroup(
                    "Connection String",
                    "Enter the connection string used to connect to the database.",
                    false,
                    new StandardFormField("Connection String:", this.txtConnectionString)
                )
            );
        }
   
        public override void BindToForm(ProviderBase extension)
        {
            var sqlProv = (SqlServerDatabaseProvider)extension;
            txtConnectionString.Text = sqlProv.ConnectionString;
        }

        public override ProviderBase CreateFromForm()
        {
            return new SqlServerDatabaseProvider() 
            { 
                ConnectionString = txtConnectionString.Text
            };
        }
    }
}
