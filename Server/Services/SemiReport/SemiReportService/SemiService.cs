using AntDesign;
using AquaSolution.Data.Connection;
using AquaSolution.Shared.HRMSLOCAL;
using AquaSolution.Shared.SemiReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using static AquaSolution.Server.Services.SemiReport.SemiReportService.SemiService;

namespace AquaSolution.Server.Services.SemiReport.SemiReportService
{
    public class SemiService : ISemiService
    {


        private readonly IConfiguration _config;

        public SemiService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<SemiReportDto>> GetAllAsync()
        {
            try
            {
                var result = new List<SemiReportDto>();
                var connStr = _config.GetConnectionString("SemiConnection");

                const string sql = @"
                    SELECT
                        a.InnerBarcode,
                        a.ScanTimeInner,
                        a.ScrapBarcode,
                        a.ScanScrapTime,
                        a.OuterBarcode,
                        a.ScanOuterTime,
                        a.MotoBarcode,
                        a.ScanMotoTime,
                        b.Description,
                        a.Model
                    FROM dbo.tbl_ScanSemi a
                    LEFT JOIN dbo.tbl_Defect b
                        ON a.ScrapBarcode = b.ScrapBarcode
                    ORDER BY a.ScanTimeInner DESC;
                ";

                await using var con = new SqlConnection(connStr);
                await using var cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new SemiReportDto
                    {
                        InnerBarcode = reader.GetString(0),
                        ScanTimeInner = reader.GetDateTime(1),
                        ScrapBarcode = reader.IsDBNull(2) ? null : reader.GetString(2),
                        ScanTimeScrap = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        OuterBarcode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        ScanTimeOuter = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                        MotorBarcode = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ScanTimeMotor = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        ScrapDescription = reader.IsDBNull(8) ? null : reader.GetString(8),
                        Model = reader.IsDBNull(9) ? null : reader.GetString(9)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<bool> GetInner4HourStatusAsync()
        {
            var result = false; 
            var connStr = _config.GetConnectionString("SemiConnection");

            const string sql = @"
                SELECT IsActive
                FROM tbl_Logic
                WHERE LogicName = 'Inner4Hour'
            ";

            await using var con = new SqlConnection(connStr);
            await using var cmd = new SqlCommand(sql, con);

            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                result = !reader.IsDBNull(0) && reader.GetBoolean(0);
            }

            return result;
        }
        public async Task<bool> UpdateInner4HourStatusAsync(bool isActive)
        {
            var connStr = _config.GetConnectionString("SemiConnection");

            const string sql = @"
                UPDATE tbl_Logic
                SET IsActive = @IsActive
                WHERE LogicName = 'Inner4Hour'
            ";

            try
            {
                await using var con = new SqlConnection(connStr);
                await using var cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@IsActive", isActive);

                await con.OpenAsync();

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error: {ex.Message}");
                return false;
            }
        }
    }


}
