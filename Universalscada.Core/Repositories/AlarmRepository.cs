using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Universalscada.core;
using Universalscada.core.Models;
using Universalscada.Models;

namespace Universalscada.Repositories
{
    public class AlarmRepository
    {
        private readonly string _connectionString = AppConfig.PrimaryConnectionString;

        public List<AlarmReportItem> GetAlarmReport(DateTime startTime, DateTime endTime, int? machineId)
        {
            var reportItems = new List<AlarmReportItem>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                // İç içe SELECT yerine JOIN kullanarak optimize edilmiş sorgu
                // SQLite'ta tarih karşılaştırması string formatında yapılır
                string query = @"
                    SELECT 
                        m.MachineName,
                        ad.AlarmNumber,
                        ad.AlarmText,
                        active_event.EventTimestamp AS StartTime,
                        (SELECT MIN(ia.EventTimestamp) 
                         FROM alarm_history ia 
                         WHERE ia.MachineId = active_event.MachineId 
                           AND ia.AlarmDefinitionId = active_event.AlarmDefinitionId 
                           AND ia.EventType = 'INACTIVE' 
                           AND ia.EventTimestamp > active_event.EventTimestamp) AS EndTime
                    FROM alarm_history AS active_event
                    JOIN machines AS m ON active_event.MachineId = m.Id
                    JOIN alarm_definitions AS ad ON active_event.AlarmDefinitionId = ad.Id
                    WHERE 
                        active_event.EventType = 'ACTIVE' AND
                        active_event.EventTimestamp BETWEEN @StartTime AND @EndTime " +
                    (machineId.HasValue ? "AND active_event.MachineId = @MachineId " : "") +
                    @"ORDER BY StartTime DESC;";

                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                if (machineId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@MachineId", machineId.Value);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new AlarmReportItem
                        {
                            MachineName = reader.GetString(reader.GetOrdinal("MachineName")),
                            AlarmNumber = reader.GetInt32(reader.GetOrdinal("AlarmNumber")),
                            AlarmText = reader.GetString(reader.GetOrdinal("AlarmText")),
                            StartTime = reader.GetDateTime(reader.GetOrdinal("StartTime")),
                            EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("EndTime"))
                        };

                        if (item.EndTime.HasValue)
                        {
                            TimeSpan duration = item.EndTime.Value - item.StartTime;
                            item.Duration = duration.ToString(@"hh\:mm\:ss");
                        }
                        else
                        {
                            item.Duration = "Aktif";
                        }
                        reportItems.Add(item);
                    }
                }
            }
            return reportItems;
        }

        // Diğer metotların da benzer şekilde SqliteConnection'a çevrilmesi gerekir.
        // Örnek: GetTopAlarmsByFrequency
        public List<TopAlarmData> GetTopAlarmsByFrequency(DateTime startTime, DateTime endTime, int limit = 5)
        {
            var topAlarms = new List<TopAlarmData>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        ad.AlarmText, 
                        COUNT(ah.Id) as AlarmCount
                    FROM alarm_history ah
                    JOIN alarm_definitions ad ON ah.AlarmDefinitionId = ad.Id
                    WHERE 
                        ah.EventType = 'ACTIVE' AND
                        ah.EventTimestamp BETWEEN @StartTime AND @EndTime
                    GROUP BY ad.AlarmText
                    ORDER BY AlarmCount DESC
                    LIMIT @Limit;"; // SQLite LIMIT destekler

                var cmd = new SqliteCommand(query, connection);
                cmd.Parameters.AddWithValue("@StartTime", startTime);
                cmd.Parameters.AddWithValue("@EndTime", endTime);
                cmd.Parameters.AddWithValue("@Limit", limit);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        topAlarms.Add(new TopAlarmData
                        {
                            AlarmText = reader.GetString(0),
                            Count = reader.GetInt32(1)
                        });
                    }
                }
            }
            return topAlarms;
        }
    }
}