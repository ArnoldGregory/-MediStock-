using MySqlConnector;
using Newtonsoft.Json;
using NLog;
using MediStock.API.Helpers;
using MediStock.API.Models;
using System.Data;
using static MediStock.API.Models.DBHandler;

namespace MediStock.API.Models
{
    public class DBHandler : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private MySqlConnection connection;
        private string connectionstring;

        public DBHandler(string connstring)
        {
            connection = new MySqlConnection(connstring);
            this.connection.Open();
            connectionstring = connstring;
        }

        public void Dispose()
        {
            connection.Close();
        }

        #region Databases
        public enum DataBaseObject
        {
            HostDB
        }
        public string GetDataBaseConnection(DataBaseObject databaseobject)
        {
            string connection_string = databaseobject switch
            {
                DataBaseObject.HostDB => connectionstring,
                _ => connectionstring,
            };
            return connection_string;
        }
        #endregion

        #region Generic Methods

        public DataTable GetRecords(string module, string param1 = "", string param2 = "", string param3 = "", string param4 = "")
        {
            DataTable dt = new();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_records", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_module", module);
                cmd.Parameters.AddWithValue("@p_param1", param1);
                cmd.Parameters.AddWithValue("@p_param2", param2);
                cmd.Parameters.AddWithValue("@p_param3", param3);
                cmd.Parameters.AddWithValue("@p_param4", param4);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetRecords: module: '" + module + "  '," + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public DataTable GetRecordsById(string module, Int64 id, string param1 = "", string param2 = "")
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_records_by_id", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_module", module);
                cmd.Parameters.AddWithValue("@p_record_id", id);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetRecordsById: MODULE: " + module + " " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public bool DeleteRecord(Int64 id, Int64 deleted_by, string module)
        {
            try
            {
                int i = 0;
                using (MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    using MySqlCommand cmd = new MySqlCommand("delete_records", connect);
                    connect.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_recordid", id);
                    cmd.Parameters.AddWithValue("@p_deleted_by", deleted_by);
                    cmd.Parameters.AddWithValue("@p_module", module);
                    i = (int)cmd.ExecuteNonQuery();
                }
                if (i >= 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("DeleteRecord: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return false;
            }
        }

        public bool AddAuditTrail(AuditTrailModel model)
        {
            try
            {
                int i = 0;
                using (MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    using (MySqlCommand cmd = new MySqlCommand("add_audit_trail", connect))
                    {
                        connect.Open();
                        cmd.CommandType = CommandType.StoredProcedure;

                        MySqlParameter outputParam = new MySqlParameter("@p_id", MySqlDbType.Int32);
                        outputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParam);

                        cmd.Parameters.AddWithValue("@p_user_name", model.user_name);
                        cmd.Parameters.AddWithValue("@p_action_type", model.action_type);
                        cmd.Parameters.AddWithValue("@p_action_description", model.action_description);
                        cmd.Parameters.AddWithValue("@p_page_accessed", model.page_accessed);
                        cmd.Parameters.AddWithValue("@p_client_ip_address", model.client_ip_address);
                        cmd.Parameters.AddWithValue("@p_session_id", model.session_id);
                        cmd.Parameters.AddWithValue("@p_created_on", model.created_on);

                        i = cmd.ExecuteNonQuery();
                    }
                }
                if (i >= 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddAuditTrail: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return false;
            }
        }

        public DataTable GetAdhocData(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand(sql, connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetAdhocData: " + " sql: " + sql + " - " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public DataTable GetAdhocData(string query, MySqlParameter[] parameters)
        {
            DataTable dataTable = new DataTable();
            using (MySqlConnection conn = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        public string GetScalarItem(string sql)
        {
            string scalaritem = "";
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand command = new MySqlCommand(sql, connect);
                connect.Open();
                scalaritem = command.ExecuteScalar()?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                logger.Error("GetScalarItem: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                scalaritem = "";
            }
            return scalaritem;
        }

        public async Task<int> ExecuteNonQuery(string query, object parameters = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var prop in parameters.GetType().GetProperties())
                            {
                                command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(parameters));
                            }
                        }
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        logger.Info($"ExecuteNonQuery: Rows affected = {rowsAffected}");
                        return rowsAffected;
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                logger.Error("ExecuteNonQuery: " + sqlEx.Message + " - " + sqlEx.StackTrace + " - " + sqlEx.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("ExecuteNonQuery: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                throw;
            }
        }

        /// <summary>
        /// Executes an INSERT and returns the auto-increment id from the SAME
        /// connection (LAST_INSERT_ID() is connection-scoped).
        /// </summary>
        public long ExecuteInsertReturnId(string query, object parameters = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var prop in parameters.GetType().GetProperties())
                                command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(parameters));
                        }
                        connection.Open();
                        command.ExecuteNonQuery();
                        return command.LastInsertedId;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("ExecuteInsertReturnId: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return 0;
            }
        }

        public async Task<DataTable> GetAdhocDataAsync(string query)
        {
            var dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    await connection.OpenAsync();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                logger.Error("GetAdhocDataAsync: " + sqlEx.Message + " - " + sqlEx.StackTrace + " - " + sqlEx.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("GetAdhocDataAsync: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                throw;
            }
            return dataTable;
        }

        #endregion

        #region Auth Methods

        public DataTable ValidateUserLogin(string user_type, string email_address)  
        {
            DataTable dt = new();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("validate_login", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@username", email_address);
                cmd.Parameters.AddWithValue("@profiletype", user_type);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("ValidateUserLogin: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public Int64 AddRefreshToken(long userId, string hashedToken, DateTime expiresAt)
        {
            logger.Info("******* Start AddRefreshToken Process *********");
            Int64 newId = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_refresh_token", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_user_id", userId);
                cmd.Parameters.AddWithValue("@p_hashed_token", hashedToken);
                cmd.Parameters.AddWithValue("@p_expires_at", expiresAt);

                int rows = cmd.ExecuteNonQuery();
                newId = cmd.LastInsertedId > 0 ? cmd.LastInsertedId : (rows > 0 ? 1 : 0);

                logger.Info($"Refresh token added - result id: {newId}");
            }
            catch (Exception ex)
            {
                logger.Error("AddRefreshToken: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
            }
            logger.Info("******* End AddRefreshToken Process *********");
            return newId;
        }

        public DataTable GetActiveRefreshTokens()
        {
            DataTable dt = new();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_active_refresh_tokens", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetActiveRefreshTokens: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public long GetUserIdFromRefreshToken(string plainToken)
        {
            logger.Info("******* Start GetUserIdFromRefreshToken Process *********");
            long userId = 0;
            try
            {
                DataTable dt = GetRecords("get_active_refresh_tokens");
                foreach (DataRow row in dt.Rows)
                {
                    string storedHashed = row["token"].ToString();
                    if (BCrypt.Net.BCrypt.Verify(plainToken, storedHashed))
                    {
                        bool revoked = row["revoked_at"] != DBNull.Value;
                        DateTime expires = Convert.ToDateTime(row["expires_at"]);
                        if (!revoked && DateTime.UtcNow <= expires)
                        {
                            userId = Convert.ToInt64(row["user_id"]);
                            logger.Info($"Valid refresh token found - user_id: {userId}");
                            break;
                        }
                    }
                }
                if (userId == 0)
                    logger.Info("No valid refresh token match found");
            }
            catch (Exception ex)
            {
                logger.Error("GetUserIdFromRefreshToken: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
            }
            logger.Info("******* End GetUserIdFromRefreshToken Process *********");
            return userId;
        }

        public bool RevokeRefreshToken(string plainToken, string ipAddress)
        {
            logger.Info("******* Start RevokeRefreshToken Process *********");
            bool success = false;
            try
            {
                DataTable dt = GetRecords("get_active_refresh_tokens");
                foreach (DataRow row in dt.Rows)
                {
                    string storedHashed = row["token"].ToString();
                    if (BCrypt.Net.BCrypt.Verify(plainToken, storedHashed))
                    {
                        using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                        using MySqlCommand cmd = new MySqlCommand("revoke_refresh_token", connect);
                        connect.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_token", storedHashed);
                        cmd.Parameters.AddWithValue("@p_ip", ipAddress ?? "unknown");
                        int rows = cmd.ExecuteNonQuery();
                        success = rows > 0;
                        logger.Info($"RevokeRefreshToken - rows affected: {rows}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("RevokeRefreshToken: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
            }
            logger.Info("******* End RevokeRefreshToken Process *********");
            return success;
        }

        public bool RevokeAllUserRefreshTokens(long userId)
        {
            logger.Info("******* Start RevokeAllUserRefreshTokens Process *********");
            bool success = false;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("revoke_all_user_refresh_tokens", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_user_id", userId);
                int rows = cmd.ExecuteNonQuery();
                logger.Info($"Revoked {rows} tokens for user {userId}");
                success = true;
            }
            catch (Exception ex)
            {
                logger.Error("RevokeAllUserRefreshTokens: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
            }
            logger.Info("******* End RevokeAllUserRefreshTokens Process *********");
            return success;
        }

        public Int64 RizikiSaveOtp(Int64 userId, string userType, string email, string? mobile,
                                   string otpCode, string purpose, string otpRef)
        {
            Int64 newId = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("riziki_save_otp", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@in_user_id", userId);
                cmd.Parameters.AddWithValue("@in_user_type", userType);
                cmd.Parameters.AddWithValue("@in_email", email);
                cmd.Parameters.AddWithValue("@in_mobile", (object?)mobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_otp_code", otpCode);
                cmd.Parameters.AddWithValue("@in_purpose", purpose);
                cmd.Parameters.AddWithValue("@in_otp_ref", otpRef);
                cmd.Parameters.Add("@out_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                newId = Convert.ToInt64(cmd.Parameters["@out_id"].Value ?? 0);
            }
            catch (Exception ex)
            {
                logger.Error("RizikiSaveOtp: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return newId;
        }

        public DataTable RizikiVerifyOtp(string email, string otpCode, string purpose)
        {
            DataTable dt = new();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("riziki_verify_otp", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@in_email", email);
                cmd.Parameters.AddWithValue("@in_otp_code", otpCode);
                cmd.Parameters.AddWithValue("@in_purpose", purpose);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("RizikiVerifyOtp: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public bool GetUserChangePasswordFlag(Int64 userId)
        {
            try
            {
                using MySqlConnection c = new(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new(
                    "SELECT change_password FROM p_external_portal_user WHERE id = @id LIMIT 1", c);
                c.Open();
                cmd.Parameters.AddWithValue("@id", userId);
                var val = cmd.ExecuteScalar();
                if (val == null || val == DBNull.Value) return false;
                return Convert.ToBoolean(val);
            }
            catch (Exception ex) { logger.Error("GetUserChangePasswordFlag: " + ex.Message); return false; }
        }

        public bool SetChangePasswordFlag(Int64 userId)
        {
            try
            {
                using MySqlConnection c = new(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new(
                    "UPDATE p_external_portal_user SET change_password = 1 WHERE id = @id LIMIT 1", c);
                c.Open();
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("SetChangePasswordFlag: " + ex.Message);
                return false;
            }
        }

        public bool ClearChangePasswordFlag(string email)
        {
            try
            {
                using MySqlConnection c = new(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new(
                    "UPDATE p_external_portal_user SET change_password = 0 WHERE email = @email LIMIT 1", c);
                c.Open();
                cmd.Parameters.AddWithValue("@email", email);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("ClearChangePasswordFlag: " + ex.Message);
                return false;
            }
        }

        public bool PortalPasswordReset(string email, string password, string profile_type)
        {
            try
            {
                int i = 0;
                using (MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    using MySqlCommand cmd = new MySqlCommand("client_password_reset", connect);
                    connect.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_email", email);
                    cmd.Parameters.AddWithValue("@p_password", password);
                    cmd.Parameters.AddWithValue("@p_profiletype", profile_type);
                    i = (int)cmd.ExecuteNonQuery();
                }
                if (i >= 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("PortalPasswordReset: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return false;
            }
        }

        public bool UpdateJWT(string jwt, Int64 user_id)
        {
            try
            {
                int i = 0;
                using (MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    using MySqlCommand cmd = new MySqlCommand("update_jwt_token", connect);
                    connect.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_id", user_id);
                    cmd.Parameters.AddWithValue("@p_jwt", jwt);
                    i = (int)cmd.ExecuteNonQuery();
                }
                if (i >= 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateJWT: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return false;
            }
        }

        #endregion

        #region Domain Methods

        public bool AddPharmacy(PharmacyModel m)
        {
            logger.Info("******* Start AddPharmacy Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_pharmacy", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_name", (object?)m.name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_slug", (object?)m.slug ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_address", (object?)m.address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_phone", (object?)m.phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_email", (object?)m.email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_license_no", (object?)m.license_no ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_owner_name", (object?)m.owner_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddPharmacy: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddUser(PharmacyUserModel m)
        {
            logger.Info("******* Start AddUser Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_user", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_role_id", m.role_id);
                cmd.Parameters.AddWithValue("@in_first_name", (object?)m.first_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_last_name", (object?)m.last_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_email", (object?)m.email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_password", (object?)m.password ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_phone", (object?)m.phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_is_active", m.is_active);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddUser: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddProduct(ProductModel m)
        {
            logger.Info("******* Start AddProduct Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_product", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_category_id", m.category_id);
                cmd.Parameters.AddWithValue("@in_name", (object?)m.name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_description", (object?)m.description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_sku", (object?)m.sku ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_barcode", (object?)m.barcode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_cost_price", m.cost_price);
                cmd.Parameters.AddWithValue("@in_selling_price", m.selling_price);
                cmd.Parameters.AddWithValue("@in_reorder_level", m.reorder_level);
                cmd.Parameters.AddWithValue("@in_unit_of_measure", (object?)m.unit_of_measure ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_is_active", m.is_active);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddProduct: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool UpdateProduct(ProductModel m)
        {
            logger.Info("******* Start UpdateProduct Process *********");
            try
            {
                int i = 0;
                using (MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB)))
                {
                    using MySqlCommand cmd = new MySqlCommand("update_product", connect);
                    connect.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@in_id", m.id);
                    cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                    cmd.Parameters.AddWithValue("@in_category_id", m.category_id);
                    cmd.Parameters.AddWithValue("@in_name", (object?)m.name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@in_description", (object?)m.description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@in_sku", (object?)m.sku ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@in_barcode", (object?)m.barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@in_cost_price", m.cost_price);
                    cmd.Parameters.AddWithValue("@in_selling_price", m.selling_price);
                    cmd.Parameters.AddWithValue("@in_reorder_level", m.reorder_level);
                    cmd.Parameters.AddWithValue("@in_unit_of_measure", (object?)m.unit_of_measure ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@in_is_active", m.is_active);
                    i = (int)cmd.ExecuteNonQuery();
                }
                if (i >= 1) return true;
                else return false;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateProduct: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return false;
            }
        }

        public bool AddCategory(ProductCategoryModel m)
        {
            logger.Info("******* Start AddCategory Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_category", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_name", (object?)m.name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_description", (object?)m.description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddCategory: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddCustomer(CustomerModel m)
        {
            logger.Info("******* Start AddCustomer Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_customer", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_first_name", (object?)m.first_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_last_name", (object?)m.last_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_email", (object?)m.email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_phone", (object?)m.phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_address", (object?)m.address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_date_of_birth", (object?)m.date_of_birth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_gender", (object?)m.gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_customer_type", (object?)m.customer_type ?? "Retail");
                cmd.Parameters.AddWithValue("@in_credit_limit", m.credit_limit);
                cmd.Parameters.AddWithValue("@in_payment_terms", (object?)m.payment_terms ?? "Cash");
                cmd.Parameters.AddWithValue("@in_is_active", m.is_active ? 1 : 0);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddCustomer: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddSupplier(SupplierModel m)
        {
            logger.Info("******* Start AddSupplier Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_supplier", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_name", (object?)m.name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_contact_person", (object?)m.contact_person ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_email", (object?)m.email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_phone", (object?)m.phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_address", (object?)m.address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_city", (object?)m.city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_country", (object?)m.country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddSupplier: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool UpdateSupplier(SupplierModel m)
        {
            logger.Info("******* Start UpdateSupplier Process *********");
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("update_supplier", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@in_id", m.id);
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_name", (object?)m.name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_contact_person", (object?)m.contact_person ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_email", (object?)m.email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_phone", (object?)m.phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_address", (object?)m.address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_city", (object?)m.city ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_country", (object?)m.country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_is_active", m.is_active ? 1 : 0);
                int rows = cmd.ExecuteNonQuery();
                return rows >= 0;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateSupplier: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddSale(SaleModel m)
        {
            logger.Info("******* Start AddSale Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("create_sale", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_sale_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_customer_id", (object?)m.customer_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_user_id", m.user_id);
                cmd.Parameters.AddWithValue("@in_total_amount", m.total_amount);
                cmd.Parameters.AddWithValue("@in_discount", m.discount);
                cmd.Parameters.AddWithValue("@in_tax", m.tax);
                cmd.Parameters.AddWithValue("@in_net_amount", m.net_amount);
                cmd.Parameters.AddWithValue("@in_amount_paid", m.amount_paid);
                cmd.Parameters.AddWithValue("@in_payment_method", (object?)m.payment_method ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_notes", (object?)m.notes ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_sale_id"].Value != null && cmd.Parameters["@p_sale_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_sale_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddSale: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddSaleItems(Int64 saleId, Int64 pharmacyId, List<SaleItemModel> items)
        {
            logger.Info("******* Start AddSaleItems Process *********");
            try
            {
                foreach (var item in items)
                {
                    using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                    using MySqlCommand cmd = new MySqlCommand("add_sale_item", connect);
                    connect.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@in_sale_id", saleId);
                    cmd.Parameters.AddWithValue("@in_product_id", item.product_id);
                    cmd.Parameters.AddWithValue("@in_quantity", item.quantity);
                    cmd.Parameters.AddWithValue("@in_unit_price", item.unit_price);
                    cmd.Parameters.AddWithValue("@in_discount", item.discount);
                    cmd.Parameters.AddWithValue("@in_total", item.total);
                    cmd.ExecuteNonQuery();
                }

                foreach (var item in items)
                {
                    using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                    using MySqlCommand cmd = new MySqlCommand("deduct_stock_on_sale", connect);
                    connect.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                    cmd.Parameters.AddWithValue("@p_product_id", item.product_id);
                    cmd.Parameters.AddWithValue("@p_batch_id", (object?)item.batch_id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@p_quantity", item.quantity);
                    cmd.ExecuteNonQuery();
                }
                logger.Info("******* End AddSaleItems Process *********");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("AddSaleItems: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool ReceiveStock(Int64 poId, ReceiveStockModel m)
        {
            logger.Info("******* Start ReceiveStock Process *********");
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("receive_stock", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@in_po_id", poId);
                cmd.Parameters.AddWithValue("@in_received_by", m.received_by);
                cmd.Parameters.AddWithValue("@in_quantity_received", m.quantity_received);
                cmd.Parameters.AddWithValue("@in_notes", (object?)m.notes ?? DBNull.Value);
                cmd.ExecuteNonQuery();
                logger.Info("******* End ReceiveStock Process *********");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("ReceiveStock: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddPurchaseOrder(PurchaseOrderModel m)
        {
            logger.Info("******* Start AddPurchaseOrder Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_purchase_order", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_supplier_id", m.supplier_id);
                cmd.Parameters.AddWithValue("@in_product_id", m.product_id);
                cmd.Parameters.AddWithValue("@in_quantity", m.quantity);
                cmd.Parameters.AddWithValue("@in_unit_cost", m.unit_cost);
                cmd.Parameters.AddWithValue("@in_total_cost", m.total_cost);
                cmd.Parameters.AddWithValue("@in_expected_date", (object?)m.expected_date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_notes", (object?)m.notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddPurchaseOrder: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddExpense(ExpenseModel m)
        {
            logger.Info("******* Start AddExpense Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_expense", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_category", (object?)m.category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_description", (object?)m.description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_amount", m.amount);
                cmd.Parameters.AddWithValue("@in_expense_date", m.expense_date);
                cmd.Parameters.AddWithValue("@in_payment_method", (object?)m.payment_method ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_notes", (object?)m.notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddExpense: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddPatient(PatientModel m)
        {
            logger.Info("******* Start AddPatient Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_patient", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_first_name", (object?)m.first_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_last_name", (object?)m.last_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_date_of_birth", (object?)m.date_of_birth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_gender", (object?)m.gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_phone", (object?)m.phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_email", (object?)m.email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_address", (object?)m.address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_allergies", (object?)m.allergies ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_medical_history", (object?)m.medical_history ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddPatient: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddPrescription(PrescriptionModel m)
        {
            logger.Info("******* Start AddPrescription Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_prescription", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_patient_id", m.patient_id);
                cmd.Parameters.AddWithValue("@in_doctor_name", (object?)m.doctor_name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_hospital", (object?)m.hospital ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_prescription_date", m.prescription_date);
                cmd.Parameters.AddWithValue("@in_notes", (object?)m.notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddPrescription: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public bool AddDDAEntry(DDAModel m)
        {
            logger.Info("******* Start AddDDAEntry Process *********");
            Int64 i = 0;
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_dda_entry", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@in_pharmacy_id", m.pharmacy_id);
                cmd.Parameters.AddWithValue("@in_patient_id", m.patient_id);
                cmd.Parameters.AddWithValue("@in_prescription_id", (object?)m.prescription_id ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_product_id", m.product_id);
                cmd.Parameters.AddWithValue("@in_quantity", m.quantity);
                cmd.Parameters.AddWithValue("@in_dispensed_date", m.dispensed_date);
                cmd.Parameters.AddWithValue("@in_notes", (object?)m.notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@in_created_by", m.created_by);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_id"].Value != null && cmd.Parameters["@p_id"].Value != DBNull.Value)
                    i = Convert.ToInt64(cmd.Parameters["@p_id"].Value.ToString());

                if (i <= 0) i = cmd.LastInsertedId;

                if (i >= 1) { m.id = i; return true; }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error("AddDDAEntry: " + ex.Message + " - " + ex.StackTrace + " - " + (ex.InnerException?.ToString() ?? ""));
                return false;
            }
        }

        public DataTable GetDashboardSummary(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_dashboard_summary", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetDashboardSummary: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public DataTable GetPharmacyIdBySlug(string slug)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_pharmacy_by_slug", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_slug", slug);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetPharmacyIdBySlug: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        public string GetCallerIdByEmail(string email)
        {
            string callerId = "";
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand(
                    "SELECT id FROM p_external_portal_user WHERE email = @email LIMIT 1", connect);
                connect.Open();
                cmd.Parameters.AddWithValue("@email", email);
                var scalar = cmd.ExecuteScalar();
                if (scalar != null && scalar != DBNull.Value)
                    callerId = scalar.ToString() ?? "";
            }
            catch (Exception ex)
            {
                logger.Error("GetCallerIdByEmail: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return callerId;
        }

        #endregion

        #region Menu Methods

        public DataTable GetMenu(int profileId, string type, string menuName)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_menu", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_profile_id", profileId);
                cmd.Parameters.AddWithValue("@p_type", type);
                cmd.Parameters.AddWithValue("@p_menu_name", menuName);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetMenu: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
            }
            return dt;
        }

        // All master menu items from menu_access_data with a per-role access flag.
        public DataTable GetMenuAccessWithState(int roleId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand(
                    "SELECT mad.id, mad.main_menu_name, mad.sub_menu_name, mad.menu_icon, mad.menu_order, " +
                    "mad.sub_menu_order, mad.page_url, mad.menu_type, " +
                    "CASE WHEN ma.id IS NOT NULL THEN 1 ELSE 0 END AS has_access " +
                    "FROM menu_access_data mad " +
                    "LEFT JOIN menu_access ma ON ma.role_id = @p_role_id " +
                    "  AND ma.main_menu_name = mad.main_menu_name " +
                    "  AND COALESCE(ma.sub_menu_name,'') = COALESCE(mad.sub_menu_name,'') " +
                    "  AND ma.can_access = 1 " +
                    "ORDER BY mad.menu_order, mad.sub_menu_order", connect);
                cmd.Parameters.AddWithValue("@p_role_id", roleId);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetMenuAccessWithState: " + ex.Message + " - " + ex.StackTrace);
            }
            return dt;
        }

        // Set / revoke access for a single menu item on a role (name-keyed, matches get_menu).
        public bool SetMenuAccess(int roleId, string mainMenuName, string subMenuName,
            string pageUrl, string menuIcon, int menuOrder, int subMenuOrder, bool canAccess)
        {
            try
            {
                mainMenuName = mainMenuName?.Replace("'", "''") ?? "";
                subMenuName = (subMenuName ?? "").Replace("'", "''");
                pageUrl = "~" + (pageUrl ?? "").Replace("~", "").Replace("'", "''");
                menuIcon = (menuIcon ?? "fa-circle").Replace("'", "''");

                string existsSql = $"SELECT id FROM menu_access WHERE role_id = {roleId} " +
                    $"AND main_menu_name = '{mainMenuName}' AND COALESCE(sub_menu_name,'') = '{subMenuName}'";
                DataTable exists = GetAdhocData(existsSql);

                if (canAccess)
                {
                    if (exists.Rows.Count > 0)
                    {
                        GetAdhocData($"UPDATE menu_access SET can_access = 1, page_url = '{pageUrl}', " +
                            $"menu_icon = '{menuIcon}', menu_order = {menuOrder}, sub_menu_order = {subMenuOrder} " +
                            $"WHERE id = {exists.Rows[0]["id"]}");
                    }
                    else
                    {
                        GetAdhocData($"INSERT INTO menu_access (role_id, main_menu_name, sub_menu_name, menu_icon, page_url, can_access, menu_order, sub_menu_order) " +
                            $"VALUES ({roleId}, '{mainMenuName}', '{subMenuName}', '{menuIcon}', '{pageUrl}', 1, {menuOrder}, {subMenuOrder})");
                    }
                }
                else
                {
                    GetAdhocData($"UPDATE menu_access SET can_access = 0, menu_order = {menuOrder}, sub_menu_order = {subMenuOrder} " +
                        $"WHERE role_id = {roleId} AND main_menu_name = '{mainMenuName}' AND COALESCE(sub_menu_name,'') = '{subMenuName}'");
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("SetMenuAccess: " + ex.Message + " - " + ex.StackTrace + " - " + ex.InnerException);
                return false;
            }
        }

        #endregion

        #region Admin Methods

        public DataTable GetUsersByPharmacy(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_users_by_pharmacy", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetUsersByPharmacy: " + ex.Message + " - " + ex.StackTrace);
            }
            return dt;
        }

        public DataTable GetUserById(Int64 id)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_user_by_id", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_id", id);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetUserById: " + ex.Message + " - " + ex.StackTrace);
            }
            return dt;
        }

        public DataTable GetExternalUsersByPharmacy(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                connect.Open();
                using MySqlCommand cmd = new MySqlCommand(
                    "SELECT id, pharmacy_id, first_name, last_name, email, mobile, role_id, IF(locked=1,0,1) AS is_active, avatar, " +
                    "CONCAT(first_name, ' ', last_name) AS fullName, created_on AS last_login_date " +
                    "FROM p_external_portal_user WHERE pharmacy_id = @pid AND is_deleted = 0", connect);
                cmd.Parameters.AddWithValue("@pid", pharmacyId);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetExternalUsersByPharmacy: " + ex.Message + " - " + ex.StackTrace);
            }
            return dt;
        }

        public DataTable GetExternalUserById(Int64 id)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                connect.Open();
                using MySqlCommand cmd = new MySqlCommand(
                    "SELECT id, pharmacy_id, first_name, last_name, email, mobile, role_id, IF(locked=1,0,1) AS is_active, avatar " +
                    "FROM p_external_portal_user WHERE id = @id AND is_deleted = 0", connect);
                cmd.Parameters.AddWithValue("@id", id);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                sd.Fill(dt);
            }
            catch (Exception ex)
            {
                logger.Error("GetExternalUserById: " + ex.Message + " - " + ex.StackTrace);
            }
            return dt;
        }

        public bool AdminUpdateUser(Int64 id, string? firstName, string? lastName,
            string? email, string? mobile, int? roleId, bool isActive)
        {
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("update_user", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_id", id);
                cmd.Parameters.AddWithValue("@p_first_name", (object?)firstName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_last_name", (object?)lastName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_email", (object?)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_mobile", (object?)mobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_role_id", (object?)roleId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_is_active", isActive ? 1 : 0);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("AdminUpdateUser: " + ex.Message + " - " + ex.StackTrace);
                return false;
            }
        }

        public bool AdminResetPassword(Int64 userId, string hashedPassword)
        {
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("admin_reset_password", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_user_id", userId);
                cmd.Parameters.AddWithValue("@p_new_password", hashedPassword);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("AdminResetPassword: " + ex.Message + " - " + ex.StackTrace);
                return false;
            }
        }

        #endregion

        #region SuperAdmin Methods

        public DataTable GetAllPharmacies()
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_all_pharmacies", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetAllPharmacies: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetAllUsers(Int64 excludeUserId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_all_users", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_exclude_user_id", excludeUserId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetAllUsers: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetPlatformStats()
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_platform_stats", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetPlatformStats: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetPlatformAudit(int limit)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_platform_audit", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_limit", limit);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetPlatformAudit: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public (Int64 pharmacyId, Int64 userId, int errorCode, string errorDesc) AddPharmacyPlatform(
            string name, string slug, string? phone, string? email, string? address,
            string? licenseNumber, string currency, string? ownerFirst, string? ownerLast,
            string ownerEmail, string? ownerMobile, string ownerPassword, string portalPassword, Int64 createdBy)
        {
            Int64 pharmacyId = 0;
            Int64 userId = 0;
            int errorCode = 0;
            string errorDesc = "OK";
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("add_pharmacy_platform", connect);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@p_pharmacy_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@p_user_id", MySqlDbType.Int64).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@p_error_code", MySqlDbType.Int32).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@p_error_desc", MySqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@p_name", (object?)name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_slug", (object?)slug ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_phone", (object?)phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_email", (object?)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_address", (object?)address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_license_number", (object?)licenseNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_currency", (object?)currency ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_owner_first", (object?)ownerFirst ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_owner_last", (object?)ownerLast ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_owner_email", (object?)ownerEmail ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_owner_mobile", (object?)ownerMobile ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_owner_password", (object?)ownerPassword ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_portal_password", (object?)portalPassword ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@p_created_by", createdBy);
                cmd.ExecuteNonQuery();

                if (cmd.Parameters["@p_pharmacy_id"].Value != null && cmd.Parameters["@p_pharmacy_id"].Value != DBNull.Value)
                    pharmacyId = Convert.ToInt64(cmd.Parameters["@p_pharmacy_id"].Value);
                if (cmd.Parameters["@p_user_id"].Value != null && cmd.Parameters["@p_user_id"].Value != DBNull.Value)
                    userId = Convert.ToInt64(cmd.Parameters["@p_user_id"].Value);
                if (cmd.Parameters["@p_error_code"].Value != null && cmd.Parameters["@p_error_code"].Value != DBNull.Value)
                    errorCode = Convert.ToInt32(cmd.Parameters["@p_error_code"].Value);
                if (cmd.Parameters["@p_error_desc"].Value != null && cmd.Parameters["@p_error_desc"].Value != DBNull.Value)
                    errorDesc = cmd.Parameters["@p_error_desc"].Value.ToString() ?? "OK";
            }
            catch (Exception ex)
            {
                logger.Error("AddPharmacyPlatform: " + ex.Message + " - " + ex.StackTrace);
                errorCode = -1;
                errorDesc = "An unexpected error occurred";
            }
            return (pharmacyId, userId, errorCode, errorDesc);
        }

        public (bool success, string message) UpdatePharmacyStatus(Int64 pharmacyId, bool isActive)
        {
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("update_pharmacy_status", connect);
                cmd.Parameters.Add("@p_error_code", MySqlDbType.Int32).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@p_error_desc", MySqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                cmd.Parameters.AddWithValue("@p_is_active", isActive ? 1 : 0);
                cmd.ExecuteNonQuery();

                int errorCode = cmd.Parameters["@p_error_code"].Value != null && cmd.Parameters["@p_error_code"].Value != DBNull.Value
                    ? Convert.ToInt32(cmd.Parameters["@p_error_code"].Value) : 0;
                string errorDesc = cmd.Parameters["@p_error_desc"].Value != null && cmd.Parameters["@p_error_desc"].Value != DBNull.Value
                    ? cmd.Parameters["@p_error_desc"].Value.ToString() ?? "OK" : "OK";

                return (errorCode == 0, errorDesc);
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePharmacyStatus: " + ex.Message + " - " + ex.StackTrace);
                return (false, "An unexpected error occurred");
            }
        }

        #endregion

        #region Dashboard Detail Methods

        public DataTable GetStockSummary(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_stock_summary", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetStockSummary: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetSalesStats(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_sales_stats", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetSalesStats: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetExpiringItems(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_expiring_items", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetExpiringItems: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetAlerts(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_alerts", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetAlerts: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetMySales(Int64 pharmacyId, Int64 userId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_my_sales", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                cmd.Parameters.AddWithValue("@p_user_id", userId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetMySales: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        public DataTable GetPendingOrders(Int64 pharmacyId)
        {
            DataTable dt = new DataTable();
            try
            {
                using MySqlConnection connect = new MySqlConnection(GetDataBaseConnection(DataBaseObject.HostDB));
                using MySqlCommand cmd = new MySqlCommand("get_pending_orders", connect);
                using MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
                connect.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@p_pharmacy_id", pharmacyId);
                sd.Fill(dt);
            }
            catch (Exception ex) { logger.Error("GetPendingOrders: " + ex.Message + " - " + ex.StackTrace); }
            return dt;
        }

        #endregion
    }
}
