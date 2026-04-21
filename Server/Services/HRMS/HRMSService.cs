
using AquaSolution.Shared.HRMS;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace AquaSolution.Server.Services.HRMS
{
    public class HRMSService : IHRMSService
    {
        private readonly IConfiguration _configuration;

        public HRMSService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static readonly PasswordHasher<object> _hasher = new PasswordHasher<object>();
        private readonly object _pwdUser = new object();
        public string Hash(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                plainPassword = "123456";
            return _hasher.HashPassword(_pwdUser, plainPassword);
        }
        #region Insert Data
        public async Task<bool> ImportExcelAsync(List<BonusYearDto> data)
        {
            if (data == null || data.Count == 0) return true;

            string hrmsConnection = _configuration.GetConnectionString("HRMSLOCAL");

            await using var conn = new SqlConnection(hrmsConnection);
            await conn.OpenAsync();

            await DeleteAllAsync(conn);
            await InsertBonusYearAsync(conn, data);
          //  await InsertUsersAsync(conn, data);

            return true;
        }

        private async Task DeleteAllAsync(SqlConnection conn)
        {
            const string sql = @"
                    DELETE FROM dbo.TblBonusRateResult;
                    DELETE FROM dbo.TblUser;";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task InsertBonusYearAsync(SqlConnection conn, IEnumerable<BonusYearDto> data)
        {
            const string sql = @"
                    INSERT INTO dbo.TblBonusRateResult
                    (
                        EmpWorkDay, EmpName, EmpCMND, EmpDept, EmpJoinDate,
                        Q1Rated, Q2Rated, Q3Rated, Q4Rated,
                        Q1Ratio, Q2Ratio, Q3Ratio, Q4Ratio,
                        YearRatio, NoteRatio, WorkTimeRatio,
                        AwardYearRatio, BonusYear,
                        RankForMeritIncrease, RankAverageBonusRate, RankQ1Ratio, RankQ2Ratio, RankQ3Ratio, RankQ4Ratio
                    )
                    VALUES
                    (
                        @EmpWorkDay, @EmpName, @EmpCMND, @EmpDept, @EmpJoinDate,
                        @Q1Rated, @Q2Rated, @Q3Rated, @Q4Rated,
                        @Q1Ratio, @Q2Ratio, @Q3Ratio, @Q4Ratio,
                        @YearRatio, @NoteRatio, @WorkTimeRatio,
                        @AwardYearRatio, @BonusYear,
                        @RankForMeritIncrease,@RankAverageBonusRate,@RankQ1Ratio,@RankQ2Ratio,@RankQ3Ratio,@RankQ4Ratio
                    );";

            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@EmpWorkDay", SqlDbType.NVarChar);
            cmd.Parameters.Add("@EmpName", SqlDbType.NVarChar);
            cmd.Parameters.Add("@EmpCMND", SqlDbType.NVarChar);
            cmd.Parameters.Add("@EmpDept", SqlDbType.NVarChar);
            cmd.Parameters.Add("@EmpJoinDate", SqlDbType.DateTime);

            cmd.Parameters.Add("@Q1Rated", SqlDbType.NVarChar);
            cmd.Parameters.Add("@Q2Rated", SqlDbType.NVarChar);
            cmd.Parameters.Add("@Q3Rated", SqlDbType.NVarChar);
            cmd.Parameters.Add("@Q4Rated", SqlDbType.NVarChar);

            cmd.Parameters.Add("@Q1Ratio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@Q2Ratio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@Q3Ratio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@Q4Ratio", SqlDbType.NVarChar);

            cmd.Parameters.Add("@YearRatio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@NoteRatio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@WorkTimeRatio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@AwardYearRatio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@BonusYear", SqlDbType.NVarChar);

            cmd.Parameters.Add("@RankForMeritIncrease", SqlDbType.NVarChar);
            cmd.Parameters.Add("@RankAverageBonusRate", SqlDbType.NVarChar);
            cmd.Parameters.Add("@RankQ1Ratio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@RankQ2Ratio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@RankQ3Ratio", SqlDbType.NVarChar);
            cmd.Parameters.Add("@RankQ4Ratio", SqlDbType.NVarChar);
            foreach (var dto in data)
            {
                cmd.Parameters["@EmpWorkDay"].Value = (object?)dto.EmpWorkDay ?? DBNull.Value;
                cmd.Parameters["@EmpName"].Value = (object?)dto.EmpName ?? DBNull.Value;
                cmd.Parameters["@EmpCMND"].Value = (object?)dto.EmpCMND ?? DBNull.Value;
                cmd.Parameters["@EmpDept"].Value = (object?)dto.EmpDept ?? DBNull.Value;
                cmd.Parameters["@EmpJoinDate"].Value = dto.EmpJoinDate == default ? DBNull.Value : dto.EmpJoinDate;

                cmd.Parameters["@Q1Rated"].Value = (object?)dto.Q1Rated ?? DBNull.Value;
                cmd.Parameters["@Q2Rated"].Value = (object?)dto.Q2Rated ?? DBNull.Value;
                cmd.Parameters["@Q3Rated"].Value = (object?)dto.Q3Rated ?? DBNull.Value;
                cmd.Parameters["@Q4Rated"].Value = (object?)dto.Q4Rated ?? DBNull.Value;

                cmd.Parameters["@Q1Ratio"].Value = (object?)dto.Q1Ratio ?? DBNull.Value;
                cmd.Parameters["@Q2Ratio"].Value = (object?)dto.Q2Ratio ?? DBNull.Value;
                cmd.Parameters["@Q3Ratio"].Value = (object?)dto.Q3Ratio ?? DBNull.Value;
                cmd.Parameters["@Q4Ratio"].Value = (object?)dto.Q4Ratio ?? DBNull.Value;

                cmd.Parameters["@YearRatio"].Value = (object?)dto.YearRation ?? DBNull.Value;
                cmd.Parameters["@NoteRatio"].Value = (object?)dto.NoteRatio ?? DBNull.Value;
                cmd.Parameters["@WorkTimeRatio"].Value = (object?)dto.WorkTimeRation ?? DBNull.Value;
                cmd.Parameters["@AwardYearRatio"].Value = (object?)dto.AwardYearRatio ?? DBNull.Value;
                cmd.Parameters["@BonusYear"].Value = (object?)dto.BonusYear ?? DBNull.Value;

                cmd.Parameters["@RankForMeritIncrease"].Value = (object?)dto.RankForMeritIncrease ?? DBNull.Value;
                cmd.Parameters["@RankAverageBonusRate"].Value = (object?)dto.RankAverageBonusRate ?? DBNull.Value;
                cmd.Parameters["@RankQ1Ratio"].Value = (object?)dto.RankQ1Ratio ?? DBNull.Value;
                cmd.Parameters["@RankQ2Ratio"].Value = (object?)dto.RankQ2Ratio ?? DBNull.Value;
                cmd.Parameters["@RankQ3Ratio"].Value = (object?)dto.RankQ3Ratio ?? DBNull.Value;
                cmd.Parameters["@RankQ4Ratio"].Value = (object?)dto.RankQ4Ratio ?? DBNull.Value;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task InsertUsersAsync(SqlConnection conn, IEnumerable<BonusYearDto> data)
        {
            const string sql = @"
                    INSERT INTO dbo.TblUser
                    (
                        UserName,
                        NormalizedUserName,
                        EmailConfirmed,
                        PasswordHash,
                        SecurityStamp,
                        ConcurrencyStamp,
                        PhoneNumberConfirmed ,
                        TwoFactorEnabled,
                        LockoutEnd,
                        LockoutEnabled,
                        AccessFailedCount
                    )
                    VALUES
                    (
                        @UserName,
                        @NormalizedUserName,
                        @EmailConfirmed,
                        @PasswordHash,
                        @SecurityStamp,
                        @ConcurrencyStamp,
                        @PhoneNumberConfirmed,
                        @TwoFactorEnabled,
                        @LockoutEnd,
                        @LockoutEnabled,
                        @AccessFailedCount
                    );";

            await using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@NormalizedUserName", SqlDbType.NVarChar, 256);
            cmd.Parameters.Add("@EmailConfirmed", SqlDbType.Bit);
            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, -1);
            cmd.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar, 64);
            cmd.Parameters.Add("@ConcurrencyStamp", SqlDbType.NVarChar, 64);
            cmd.Parameters.Add("@PhoneNumberConfirmed", SqlDbType.Bit);
            cmd.Parameters.Add("@TwoFactorEnabled", SqlDbType.Bit);
            cmd.Parameters.Add("@LockoutEnd", SqlDbType.DateTime);
            cmd.Parameters.Add("@LockoutEnabled", SqlDbType.Bit);
            cmd.Parameters.Add("@AccessFailedCount", SqlDbType.Int);

            foreach (var dto in data)
            {
                var userName = (dto.EmpWorkDay ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(userName)) continue;

                var rawPassword = (dto.EmpCMND ?? string.Empty).Trim();

                cmd.Parameters["@UserName"].Value = userName;
                cmd.Parameters["@NormalizedUserName"].Value = userName;
                cmd.Parameters["@EmailConfirmed"].Value = false;
                cmd.Parameters["@PasswordHash"].Value = CommonHelper.SHA256EncryptString(rawPassword);
                cmd.Parameters["@SecurityStamp"].Value = Guid.NewGuid().ToString("N");
                cmd.Parameters["@ConcurrencyStamp"].Value = Guid.NewGuid().ToString("N");
                cmd.Parameters["@PhoneNumberConfirmed"].Value = false;
                cmd.Parameters["@TwoFactorEnabled"].Value = false;
                cmd.Parameters["@LockoutEnd"].Value = DBNull.Value;
                cmd.Parameters["@LockoutEnabled"].Value = false;
                cmd.Parameters["@AccessFailedCount"].Value = 0;

                await cmd.ExecuteNonQueryAsync();
            }
        }
        #endregion
        public static partial class CommonHelper
        {
            public static string SHA256EncryptString(string data)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = SHA256.Create().ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
