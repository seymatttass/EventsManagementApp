using EventManagementApp.Web.App_Start;
using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace EventManagementApp.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                ConfigureSerilogSimple();

                DependencyConfig.RegisterDependencies();

                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);

                System.Diagnostics.Debug.WriteLine("EventManagementApp başarıyla başlatıldı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application_Start hatası: {ex.Message}");
            }
        }

        private void ConfigureSerilogSimple()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 BAŞLAMA: Serilog test başlıyor...");

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                System.Diagnostics.Debug.WriteLine($"🔗 CONNECTION STRING: {connectionString}");

                CreateAppLogTableManually(connectionString);

                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.MSSqlServer(
                        connectionString: connectionString,
                        sinkOptions: new MSSqlServerSinkOptions
                        {
                            TableName = "AppLog",
                            AutoCreateSqlTable = false 
                        }
                    )
                    .CreateLogger();

                System.Diagnostics.Debug.WriteLine("✅ Logger oluşturuldu");

                Log.Information("TEST 1: Bu bir basit bilgi mesajıdır");
                Log.Warning("TEST 2: Bu bir uyarı mesajıdır");
                Log.Error("TEST 3: Bu bir hata mesajıdır");

                System.Diagnostics.Debug.WriteLine("📤 Test logları gönderildi");

                System.Threading.Thread.Sleep(3000);

                CheckLogsInDatabase(connectionString);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ SERILOG HATASI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ INNER: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ STACK: {ex.StackTrace}");
            }
        }

        private void CreateAppLogTableManually(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("🔗 Veritabanına bağlanıldı");

                    var dropTable = "IF EXISTS (SELECT * FROM sysobjects WHERE name='AppLog' AND xtype='U') DROP TABLE AppLog";
                    using (var cmd = new SqlCommand(dropTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("🗑️ Eski AppLog tablosu silindi");
                    }

                    var createTable = @"
                        CREATE TABLE AppLog (
                            Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            Message nvarchar(max) NULL,
                            MessageTemplate nvarchar(max) NULL,
                            Level nvarchar(128) NULL,
                            TimeStamp datetime NOT NULL,
                            Exception nvarchar(max) NULL,
                            Properties nvarchar(max) NULL
                        )";

                    using (var cmd = new SqlCommand(createTable, connection))
                    {
                        cmd.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine("✅ AppLog tablosu oluşturuldu");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Tablo oluşturma hatası: {ex.Message}");
            }
        }

        private void CheckLogsInDatabase(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var checkQuery = "SELECT COUNT(*) FROM AppLog";
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        var count = (int)cmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"📊 VERITABANINDA LOG SAYISI: {count}");

                        if (count > 0)
                        {
                            var selectQuery = "SELECT TOP 3 Level, Message, TimeStamp FROM AppLog ORDER BY TimeStamp DESC";
                            using (var selectCmd = new SqlCommand(selectQuery, connection))
                            using (var reader = selectCmd.ExecuteReader())
                            {
                                System.Diagnostics.Debug.WriteLine("📋 SON LOGLAR:");
                                while (reader.Read())
                                {
                                    System.Diagnostics.Debug.WriteLine($"   {reader["Level"]} | {reader["Message"]} | {reader["TimeStamp"]}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("❌ VERİTABNANINDA LOG YOK!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Veritabanı kontrol hatası: {ex.Message}");
            }
        }

        protected void Application_End()
        {
            try
            {
                Log.Information("🔴 Uygulama sonlandırılıyor");
                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Application_End Log Error: {ex.Message}");
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                try
                {
                    Log.Error(exception, "🚨 Uygulama hatası: {ErrorMessage}", exception.Message);
                }
                catch (Exception logEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Uygulama hatası: {exception.Message}");
                    System.Diagnostics.Debug.WriteLine($"Log hatası: {logEx.Message}");
                }
            }
        }
    }
}