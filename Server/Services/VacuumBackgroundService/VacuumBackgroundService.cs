//using MySql.Data.MySqlClient;
//using System.Data;
//using System.Data.Common;
//using System.Text;

//namespace AquaSolution.Server.Services.VacuumBackgroundService
//{
//    public class VacuumBackgroundService : BackgroundService
//    {
//        private readonly string _defaultConn;
//        private readonly string _mesConn;

//        // AutoId lớn nhất đã xử lý (chỉ RAM)
//        private static int _lastAutoId = 0;

//        private static readonly object _lock = new object();
//        private static string _currentDate = DateTime.Now.ToString("yyyyMMdd");
//        private static readonly string _logDir =
//            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
//        private static string _logFilePath = GetLogFilePath();

//        private static string GetLogFilePath()
//        {
//            Directory.CreateDirectory(_logDir);
//            return Path.Combine(_logDir, $"vacuum_{_currentDate}.log");
//        }

//        public VacuumBackgroundService(IConfiguration configuration)
//        {
//            _defaultConn = configuration.GetConnectionString("VacuumConnection");
//            _mesConn = configuration.GetConnectionString("MesConnection_9a61");
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            Log("===== VACUUM BACKGROUND SERVICE START =====");

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    await ProcessDataAsync();
//                }
//                catch (Exception ex)
//                {
//                    Log("🔥 BACKGROUND ERROR");
//                    LogError(ex);
//                }

//                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
//            }
//        }

//        #region Handle Data

//        public async Task ProcessDataAsync()
//        {
//            Log("===== START ProcessDataAsync =====");
//            Log($"LastAutoId hiện tại = {_lastAutoId}");

//            var rows = await GetDataVacuumTodayAsync();

//            Log($"Đọc được {rows.Count} dòng từ vacuum_data");

//            if (rows.Count == 0)
//            {
//                Log("Không có data mới → kết thúc.");
//                Log("===== END ProcessDataAsync =====");
//                return;
//            }

//            await InsertDataToMesAsync(rows);

//            _lastAutoId = rows[^1].AutoId;
//            Log($"Cập nhật LastAutoId = {_lastAutoId}");
//            Log("===== END ProcessDataAsync =====");
//        }

//        private async Task<List<VacuumDataDto>> GetDataVacuumTodayAsync()
//        {
//            var result = new List<VacuumDataDto>();

//            using var conn = new MySqlConnection(_defaultConn);
//            await conn.OpenAsync();

//            var cmd = new MySqlCommand(@"
//                SELECT *
//                FROM vacuum_data
//                WHERE AutoId > @LastAutoId
//                  AND BeginTime >= CURDATE()
//                  AND BeginTime < CURDATE() + INTERVAL 1 DAY
//                ORDER BY AutoId ASC
//            ", conn);

//            cmd.Parameters.AddWithValue("@LastAutoId", _lastAutoId);

//            using var reader = await cmd.ExecuteReaderAsync();

//            while (await reader.ReadAsync())
//            {
//                result.Add(new VacuumDataDto
//                {
//                    AutoId = reader.GetInt32("AutoId"),
//                    BarCode = GetStringSafe(reader, "BarCode"),
//                    ModelName = GetStringSafe(reader, "ModelName"),
//                    InputTime = GetStringSafe(reader, "InputTime"),
//                    VacuumData = GetStringSafe(reader, "VacuumData"),
//                    VacuumTime = GetStringSafe(reader, "VacuumTime"),
//                    State = GetStringSafe(reader, "State"),
//                    StationNo = GetStringSafe(reader, "StationNo"),
//                    BeginTime = GetStringSafe(reader, "BeginTime"),
//                    EndTime = GetStringSafe(reader, "EndTime"),
//                    VacuumLow = GetStringSafe(reader, "VacuumLow"),
//                    VacuumUp = GetStringSafe(reader, "VacuumUp"),
//                    Low_Result = GetStringSafe(reader, "Low_Result"),
//                    Up_Result = GetStringSafe(reader, "Up_Result"),
//                    UploadStatus = GetStringSafe(reader, "UploadStatus"),
//                    UploadTime = GetStringSafe(reader, "UploadTime"),
//                    Valid = reader.IsDBNull(reader.GetOrdinal("Valid"))
//                        ? null
//                        : reader.GetInt32("Valid"),
//                    Bak1 = GetStringSafe(reader, "Bak1"),
//                    Bak2 = GetStringSafe(reader, "Bak2"),
//                    Bak3 = GetStringSafe(reader, "Bak3"),
//                    Bak4 = GetStringSafe(reader, "Bak4")
//                });
//            }

//            return result;
//        }

//        private async Task InsertDataToMesAsync(List<VacuumDataDto> rows)
//        {
//            using var conn = new MySqlConnection(_mesConn);
//            await conn.OpenAsync();

//            foreach (var row in rows)
//            {
//                DateTime? endTime =
//                    DateTime.TryParse(row.InputTime, out var et) ? et : null;

//                if (endTime == null) continue;

//                var checkCmd = new MySqlCommand(@"
//                    SELECT 1 FROM bns_pm_vacuuming_t
//                    WHERE End_Time = @End_Time
//                    LIMIT 1
//                ", conn);

//                checkCmd.Parameters.AddWithValue("@End_Time", endTime);

//                if (await checkCmd.ExecuteScalarAsync() != null)
//                    continue;

//                var cmd = new MySqlCommand(@"
//                    INSERT INTO bns_pm_vacuuming_t
//                    (
//                        ID, WorkUser_Barcode, Start_Time, End_Time,
//                        Operation_Time, RoughVacuum_Volume,
//                        Work_Cell_Code, Result, System_Time
//                    )
//                    VALUES
//                    (
//                        UUID(), @WorkUser_Barcode, @Start_Time, @End_Time,
//                        @Operation_Time, @RoughVacuum_Volume,
//                        @Work_Cell_Code, @Result, NOW()
//                    )
//                ", conn);

//                int? roughVacuum =
//                    decimal.TryParse(row.VacuumData, out var vd)
//                        ? (int)Math.Truncate(vd)
//                        : null;

//                int result = row.State == "2" ? 1 : 0;

//                cmd.Parameters.AddWithValue("@WorkUser_Barcode", row.BarCode ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@Start_Time",
//                    DateTime.TryParse(row.BeginTime, out var st) ? st : (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@End_Time", endTime);
//                cmd.Parameters.AddWithValue("@Operation_Time",
//                    DateTime.TryParse(row.VacuumTime, out var ot) ? ot : (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@RoughVacuum_Volume", roughVacuum ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@Work_Cell_Code", row.StationNo ?? (object)DBNull.Value);
//                cmd.Parameters.AddWithValue("@Result", result);

//                await cmd.ExecuteNonQueryAsync();
//            }
//        }

//        private string? GetStringSafe(DbDataReader reader, string col)
//        {
//            int i = reader.GetOrdinal(col);
//            return reader.IsDBNull(i) ? null : reader.GetString(i);
//        }
//        #endregion

//        private void Log(string message)
//        {
//            lock (_lock)
//            {
//                var today = DateTime.Now.ToString("yyyyMMdd");
//                if (today != _currentDate)
//                {
//                    _currentDate = today;
//                    _logFilePath = GetLogFilePath();
//                }

//                File.AppendAllText(
//                    _logFilePath,
//                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}",
//                    Encoding.UTF8
//                );
//            }
//        }

//        private void LogError(Exception ex)
//        {
//            Log(ex.ToString());
//        }
//    }

//    public class VacuumDataDto
//    {
//        public int AutoId { get; set; }
//        public string? BarCode { get; set; }
//        public string? ModelName { get; set; }
//        public string? InputTime { get; set; }
//        public string? VacuumData { get; set; }
//        public string? VacuumTime { get; set; }
//        public string? State { get; set; }
//        public string? StationNo { get; set; }
//        public string? BeginTime { get; set; }
//        public string? EndTime { get; set; }
//        public string? VacuumLow { get; set; }
//        public string? VacuumUp { get; set; }
//        public string? Low_Result { get; set; }
//        public string? Up_Result { get; set; }
//        public string? UploadStatus { get; set; }
//        public string? UploadTime { get; set; }
//        public int? Valid { get; set; }
//        public string? Bak1 { get; set; }
//        public string? Bak2 { get; set; }
//        public string? Bak3 { get; set; }
//        public string? Bak4 { get; set; }
//    }
//}
