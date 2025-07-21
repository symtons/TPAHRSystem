//// =============================================================================
//// STORED PROCEDURE SERVICE - HELPER FOR SAFE SP EXECUTION
//// File: TPAHRSystem.API/Services/StoredProcedureService.cs (NEW FILE)
//// =============================================================================

//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using System.Data;
//using TPAHRSystem.Infrastructure.Data;

//namespace TPAHRSystem.API.Services
//{
//    public interface IStoredProcedureService
//    {
//        Task<T?> ExecuteStoredProcedureAsync<T>(string procedureName, Dictionary<string, object> parameters, Func<IDataReader, T> mapper);
//        Task<List<T>> ExecuteStoredProcedureListAsync<T>(string procedureName, Dictionary<string, object> parameters, Func<IDataReader, T> mapper);
//        Task<bool> ExecuteStoredProcedureNonQueryAsync(string procedureName, Dictionary<string, object> parameters);
//    }

//    public class StoredProcedureService : IStoredProcedureService
//    {
//        private readonly TPADbContext _context;
//        private readonly ILogger<StoredProcedureService> _logger;

//        public StoredProcedureService(TPADbContext context, ILogger<StoredProcedureService> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        /// <summary>
//        /// Execute a stored procedure and return a single result
//        /// </summary>
//        public async Task<T?> ExecuteStoredProcedureAsync<T>(string procedureName, Dictionary<string, object> parameters, Func<IDataReader, T> mapper)
//        {
//            try
//            {
//                using var command = _context.Database.GetDbConnection().CreateCommand();
//                command.CommandText = $"EXEC {procedureName} {string.Join(", ", parameters.Keys.Select(k => $"@{k}"))}";
//                command.CommandType = CommandType.Text;

//                foreach (var param in parameters)
//                {
//                    command.Parameters.Add(new SqlParameter($"@{param.Key}", param.Value ?? DBNull.Value));
//                }

//                await _context.Database.OpenConnectionAsync();

//                using var reader = await command.ExecuteReaderAsync();

//                if (await reader.ReadAsync())
//                {
//                    return mapper(reader);
//                }

//                return default(T);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error executing stored procedure {procedureName}");
//                throw;
//            }
//            finally
//            {
//                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
//                {
//                    await _context.Database.CloseConnectionAsync();
//                }
//            }
//        }

//        /// <summary>
//        /// Execute a stored procedure and return a list of results
//        /// </summary>
//        public async Task<List<T>> ExecuteStoredProcedureListAsync<T>(string procedureName, Dictionary<string, object> parameters, Func<IDataReader, T> mapper)
//        {
//            var results = new List<T>();

//            try
//            {
//                using var command = _context.Database.GetDbConnection().CreateCommand();
//                command.CommandText = $"EXEC {procedureName} {string.Join(", ", parameters.Keys.Select(k => $"@{k}"))}";
//                command.CommandType = CommandType.Text;

//                foreach (var param in parameters)
//                {
//                    command.Parameters.Add(new SqlParameter($"@{param.Key}", param.Value ?? DBNull.Value));
//                }

//                await _context.Database.OpenConnectionAsync();

//                using var reader = await command.ExecuteReaderAsync();

//                while (await reader.ReadAsync())
//                {
//                    results.Add(mapper(reader));
//                }

//                return results;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error executing stored procedure {procedureName}");
//                throw;
//            }
//            finally
//            {
//                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
//                {
//                    await _context.Database.CloseConnectionAsync();
//                }
//            }
//        }

//        /// <summary>
//        /// Execute a stored procedure without returning data
//        /// </summary>
//        public async Task<bool> ExecuteStoredProcedureNonQueryAsync(string procedureName, Dictionary<string, object> parameters)
//        {
//            try
//            {
//                using var command = _context.Database.GetDbConnection().CreateCommand();
//                command.CommandText = $"EXEC {procedureName} {string.Join(", ", parameters.Keys.Select(k => $"@{k}"))}";
//                command.CommandType = CommandType.Text;

//                foreach (var param in parameters)
//                {
//                    command.Parameters.Add(new SqlParameter($"@{param.Key}", param.Value ?? DBNull.Value));
//                }

//                await _context.Database.OpenConnectionAsync();

//                var rowsAffected = await command.ExecuteNonQueryAsync();
//                return rowsAffected > 0;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error executing stored procedure {procedureName}");
//                throw;
//            }
//            finally
//            {
//                if (_context.Database.GetDbConnection().State == ConnectionState.Open)
//                {
//                    await _context.Database.CloseConnectionAsync();
//                }
//            }
//        }
//    }

//    // =============================================================================
//    // RESULT MAPPERS FOR STORED PROCEDURES
//    // =============================================================================

//    public static class StoredProcedureMappers
//    {
//        public static EmployeeCreationResult MapEmployeeCreationResult(IDataReader reader)
//        {
//            return new EmployeeCreationResult
//            {
//                EmployeeId = reader.IsDBNull("EmployeeId") ? 0 : reader.GetInt32("EmployeeId"),
//                EmployeeNumber = reader.IsDBNull("EmployeeNumber") ? string.Empty : reader.GetString("EmployeeNumber"),
//                EmployeeName = reader.IsDBNull("EmployeeName") ? string.Empty : reader.GetString("EmployeeName"),
//                OnboardingTasks = reader.IsDBNull("OnboardingTasks") ? 0 : reader.GetInt32("OnboardingTasks"),
//                Department = reader.IsDBNull("Department") ? string.Empty : reader.GetString("Department"),
//                Message = reader.IsDBNull("Message") ? string.Empty : reader.GetString("Message"),
//                ErrorMessage = reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage"),
//                Success = reader.IsDBNull("ErrorMessage") || string.IsNullOrEmpty(reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage"))
//            };
//        }

//        public static TaskCompletionResult MapTaskCompletionResult(IDataReader reader)
//        {
//            return new TaskCompletionResult
//            {
//                Message = reader.IsDBNull("Message") ? string.Empty : reader.GetString("Message"),
//                CompletedTasks = reader.IsDBNull("CompletedTasks") ? 0 : reader.GetInt32("CompletedTasks"),
//                TotalTasks = reader.IsDBNull("TotalTasks") ? 0 : reader.GetInt32("TotalTasks"),
//                CompletionPercentage = reader.IsDBNull("CompletionPercentage") ? 0 : reader.GetDecimal("CompletionPercentage"),
//                OnboardingStatus = reader.IsDBNull("OnboardingStatus") ? string.Empty : reader.GetString("OnboardingStatus"),
//                ErrorMessage = reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage"),
//                Success = reader.IsDBNull("ErrorMessage") || string.IsNullOrEmpty(reader.IsDBNull("ErrorMessage") ? null : reader.GetString("ErrorMessage"))
//            };
//        }
//    }

//    // =============================================================================
//    // RESULT MODELS FOR STORED PROCEDURES
//    // =============================================================================

//    public class EmployeeCreationResult
//    {
//        public int EmployeeId { get; set; }
//        public string EmployeeNumber { get; set; } = string.Empty;
//        public string EmployeeName { get; set; } = string.Empty;
//        public int OnboardingTasks { get; set; }
//        public string Department { get; set; } = string.Empty;
//        public string Message { get; set; } = string.Empty;
//        public string? ErrorMessage { get; set; }
//        public bool Success { get; set; }
//    }

//    public class TaskCompletionResult
//    {
//        public string Message { get; set; } = string.Empty;
//        public int CompletedTasks { get; set; }
//        public int TotalTasks { get; set; }
//        public decimal CompletionPercentage { get; set; }
//        public string OnboardingStatus { get; set; } = string.Empty;
//        public string? ErrorMessage { get; set; }
//        public bool Success { get; set; }
//    }
//}