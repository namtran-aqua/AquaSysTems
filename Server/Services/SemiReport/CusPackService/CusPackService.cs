using AquaSolution.Data.Data.Entities;
using AquaSolution.Server.Services.SemiReport.CusPackService;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;
using AquaSolution.Shared.SemiReport;
using Microsoft.Data.SqlClient;
using System.Data;

public class CusPackService : ICusPackService
{
    private readonly IConfiguration _config;

    public CusPackService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<List<CusPackNoDto>> GetAllAsync()
    {
        var result = new List<CusPackNoDto>();
        var connStr = _config.GetConnectionString("SemiConnection");

        const string sql = @"
            SELECT Id,
                CusPackNo,
                ModelCode
            FROM dbo.tbl_Roll
            ORDER BY CusPackNo ASC;
        ";

        await using var con = new SqlConnection(connStr);
        await using var cmd = new SqlCommand(sql, con);

        await con.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new CusPackNoDto
            {
                Id = reader.GetInt32(0),
                CusPackNo = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                ModelCode = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            });

        }

        return result;
    }
    public async Task<bool> CreatedAsync(CusPackNoDto cusPackDto)
    {
        var connStr = _config.GetConnectionString("SemiConnection");

        const string sql = @"
        INSERT INTO dbo.tbl_Roll (CusPackNo, ModelCode)
        VALUES (@CusPackNo, @ModelCode);
    ";

        await using var con = new SqlConnection(connStr);
        await using var cmd = new SqlCommand(sql, con);


        cmd.Parameters.Add("@CusPackNo", SqlDbType.NVarChar).Value = (object?)cusPackDto.CusPackNo ?? DBNull.Value;
        cmd.Parameters.Add("@ModelCode", SqlDbType.NVarChar).Value = (object?)cusPackDto.ModelCode ?? DBNull.Value;
        
        await con.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();

        return rows > 0;
    }
    public async Task<bool> DeleteAsync(int cusPackId)
    {
        var connStr = _config.GetConnectionString("SemiConnection");

        const string sql = @"
            DELETE FROM dbo.tbl_Roll
            WHERE Id = @Id;
            ";

        await using var con = new SqlConnection(connStr);
        await using var cmd = new SqlCommand(sql, con);

        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = cusPackId;

        await con.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();

        return rows > 0;
    }

    public async Task<bool> UpdateAsync(CusPackNoDto cusPackDto)
    {
        var connStr = _config.GetConnectionString("SemiConnection");

        const string sql = @"
            UPDATE dbo.tbl_Roll
            SET CusPackNo = @CusPackNo,
                ModelCode = @ModelCode
            WHERE Id = @Id;
            ";

        await using var con = new SqlConnection(connStr);
        await using var cmd = new SqlCommand(sql, con);

        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = cusPackDto.Id;

        cmd.Parameters.Add("@CusPackNo", SqlDbType.NVarChar).Value = (object?)cusPackDto.CusPackNo ?? DBNull.Value;
        cmd.Parameters.Add("@ModelCode", SqlDbType.NVarChar).Value = (object?)cusPackDto.ModelCode ?? DBNull.Value;

        await con.OpenAsync();
        var rows = await cmd.ExecuteNonQueryAsync();

        return rows > 0;
    }
    
}
