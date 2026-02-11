
using AquaSolution.Data.Connection;
using AquaSolution.Shared.HRMSLOCAL;
using AquaSolution.Shared.SemiReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using static AquaSolution.Server.Services.SemiReport.RollReportService.RollReportService;

namespace AquaSolution.Server.Services.SemiReport.RollReportService
{
    public class RollReportService : IRollReportService
    {
        private readonly IConfiguration _config;

        public RollReportService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<RollReportDto>> GetAllAsync()
        {
            try
            {
                var result = new List<RollReportDto>();
                var connStr = _config.GetConnectionString("SemiConnection");

                const string sql = @"
                    SELECT
                        PackNo,
                        Result,
                        ScanTime,
                        CusPackno,
                        ModelCode
                    FROM dbo.tbl_ScanRoll
                    ORDER BY ScanTime DESC;
                ";

                await using var con = new SqlConnection(connStr);
                await using var cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new RollReportDto
                    {
                        PackNo = reader.IsDBNull(0) ? null : reader.GetString(0),
                        Result = reader.IsDBNull(1) ? false : reader.GetBoolean(1),
                        ScanTime = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                        CusPackno = reader.IsDBNull(3) ? null : reader.GetString(3),
                        ModelCode = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }


                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }


}
