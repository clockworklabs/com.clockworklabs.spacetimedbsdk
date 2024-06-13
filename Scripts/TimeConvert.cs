 using System;

 namespace SpacetimeDB
 {
     /// <summary>
     /// Handles conversion between long (Rust Timestamps / microseconds since UNIX epoch)
     /// and DateTimeOffset (UTC)
     /// </summary>
     public static class TimeConvert
     {
         /// <summary>
         /// Converts from long (Rust Timestamps / microseconds since UNIX epoch) to DateTimeOffset (UTC) 
         /// </summary>
         public static DateTimeOffset FromMicrosecondsTimestamp(long microseconds)
         {
             long ticks = microseconds * 10;
             return DateTime.UnixEpoch.AddTicks(ticks);
         }

         /// <summary>
         /// Converts from DateTimeOffset (UTC) to long (Rust Timestamps / microsecondssince UNIX epoch)
         /// </summary>
         public static long ToMicrosecondsTimestamp(DateTimeOffset dateTimeOffset)
         {
             TimeSpan elapsed = dateTimeOffset - DateTime.UnixEpoch;
             return elapsed.Ticks / 10;
         }

         /// <summary>
         /// DateTimeOffset.Now (UTC) to long (Rust Timestamps / microseconds since UNIX epoch)
         /// </summary>
         public static long MicrosecondsTimestamp()
         {
             TimeSpan elapsed = DateTimeOffset.Now - DateTime.UnixEpoch;
             return elapsed.Ticks / 10;
         }

         /// <summary>
         /// Converts DateTimeOffset (UTC) to a string in ISO 8601 format.
         /// (!) Less accurate than `ToMicrosecondsTimestamp()`; only use this if required.
         /// </summary>
         [Obsolete("This is only useful if your server *requires* ISO 8601, " +
                   "but it's less accurate than long-based nanoseconds and may be desync'd: " +
                   "If possible, instead use `ToMicrosecondsTimestamp()`")]
         public static string ToTimestampIso8601(DateTimeOffset dateTimeOffset) =>
             dateTimeOffset.ToString("o"); // 'o' format specifier for ISO 8601
     }
 }