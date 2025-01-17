﻿using System;
using System.Globalization;
using System.Linq;
using LinqToDB;
using LinqToDB.SqlQuery;
using NUnit.Framework;

namespace Tests.Linq
{
	using PN = ProviderName;

	public static class TestedExtensions
	{

		class DatePartBuilder: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "year";        break;
					case Sql.DateParts.Quarter     : partStr = "quarter";     break;
					case Sql.DateParts.Month       : partStr = "month";       break;
					case Sql.DateParts.DayOfYear   : partStr = "dayofyear";   break;
					case Sql.DateParts.Day         : partStr = "day";         break;
					case Sql.DateParts.Week        : partStr = "week";        break;
					case Sql.DateParts.WeekDay     : partStr = "weekday";     break;
					case Sql.DateParts.Hour        : partStr = "hour";        break;
					case Sql.DateParts.Minute      : partStr = "minute";      break;
					case Sql.DateParts.Second      : partStr = "second";      break;
					case Sql.DateParts.Millisecond : partStr = "millisecond"; break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				builder.AddExpression("part", partStr);
			}
		}

		class DatePartBuilderMySql: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr = null;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "year";        break;
					case Sql.DateParts.Quarter     : partStr = "quarter";     break;
					case Sql.DateParts.Month       : partStr = "month";       break;
					case Sql.DateParts.DayOfYear   :
						builder.Expression = "DayOfYear({date})";
						break;
					case Sql.DateParts.Day         : partStr = "day";         break;
					case Sql.DateParts.Week        : partStr = "week";        break;
					case Sql.DateParts.WeekDay     :
						builder.Expression = "WeekDay(Date_Add({date}, interval 1 day))";
						builder.ResultExpression = builder.Inc(builder.ConvertToSqlExpression(Precedence.Primary));
						break;
					case Sql.DateParts.Hour        : partStr = "hour";        break;
					case Sql.DateParts.Minute      : partStr = "minute";      break;
					case Sql.DateParts.Second      : partStr = "second";      break;
					case Sql.DateParts.Millisecond : partStr = "millisecond"; break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (partStr != null)
					builder.AddExpression("part", partStr);
			}
		}

		class DatePartBuilderPostgre: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr = null;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "year";    break;
					case Sql.DateParts.Quarter     : partStr = "quarter"; break;
					case Sql.DateParts.Month       : partStr = "month";   break;
					case Sql.DateParts.DayOfYear   : partStr = "doy";     break;
					case Sql.DateParts.Day         : partStr = "day";     break;
					case Sql.DateParts.Week        : partStr = "week";    break;
					case Sql.DateParts.WeekDay     :
						builder.Expression = "Extract(dow from {date})";
						builder.ResultExpression = builder.Inc(builder.ConvertToSqlExpression(Precedence.Primary));
						break;
					case Sql.DateParts.Hour        : partStr = "hour";    break;
					case Sql.DateParts.Minute      : partStr = "minute";  break;
					case Sql.DateParts.Second      : partStr = "second";  break;
					case Sql.DateParts.Millisecond :
						builder.Expression = "Cast(To_Char({date}, 'MS') as int)";
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(part), part, null);
				}

				if (partStr != null)
					builder.AddExpression("part", partStr);
			}
		}

		class DatePartBuilderSqLite: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr = null;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "Y"; break;
					case Sql.DateParts.Quarter     :
						builder.Expression = "Cast(strFTime('%m', {date}) as int)";
						builder.ResultExpression = builder.Inc(builder.Div(builder.Dec(builder.ConvertToSqlExpression(Precedence.Primary)), 3));
						break;
					case Sql.DateParts.Month       : partStr = "m"; break;
					case Sql.DateParts.DayOfYear   : partStr = "j"; break;
					case Sql.DateParts.Day         : partStr = "d"; break;
					case Sql.DateParts.Week        : partStr = "W"; break;
					case Sql.DateParts.WeekDay     :
						builder.Expression = "Cast(strFTime('%w', {date}) as int)";
						builder.ResultExpression = builder.Inc(builder.ConvertToSqlExpression(Precedence.Primary));
						break;
					case Sql.DateParts.Hour        : partStr = "H"; break;
					case Sql.DateParts.Minute      : partStr = "M"; break;
					case Sql.DateParts.Second      : partStr = "S"; break;
					case Sql.DateParts.Millisecond : 
						builder.Expression = "Cast(strFTime('%f', {date}) * 1000 as int) % 1000";
						builder.Extension.Precedence = Precedence.Multiplicative;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(part), part, null);
				}

				if (partStr != null)
					builder.AddExpression("part", partStr);
			}
		}

		class DatePartBuilderAccess: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr;
				var part    = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "yyyy"; break;
					case Sql.DateParts.Quarter     : partStr = "q";    break;
					case Sql.DateParts.Month       : partStr = "m";    break;
					case Sql.DateParts.DayOfYear   : partStr = "y";    break;
					case Sql.DateParts.Day         : partStr = "d";    break;
					case Sql.DateParts.Week        : partStr = "ww";   break;
					case Sql.DateParts.WeekDay     : partStr = "w";    break;
					case Sql.DateParts.Hour        : partStr = "h";    break;
					case Sql.DateParts.Minute      : partStr = "n";    break;
					case Sql.DateParts.Second      : partStr = "s";    break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				builder.AddExpression("part", partStr);
			}
		}


		class DatePartBuilderSapHana: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "Year({date})";                     break;
					case Sql.DateParts.Quarter     : partStr = "Floor((Month({date})-1) / 3) + 1"; break;
					case Sql.DateParts.Month       : partStr = "Month({date})";                    break;
					case Sql.DateParts.DayOfYear   : partStr = "DayOfYear({date})";                break;
					case Sql.DateParts.Day         : partStr = "DayOfMonth({date})";               break;
					case Sql.DateParts.Week        : partStr = "Week({date})";                     break;
					case Sql.DateParts.WeekDay     : partStr = "MOD(Weekday({date}) + 1, 7) + 1";  break;
					case Sql.DateParts.Hour        : partStr = "Hour({date})";                     break;
					case Sql.DateParts.Minute      : partStr = "Minute({date})";                   break;
					case Sql.DateParts.Second      : partStr = "Second({date})";                   break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				builder.Expression = partStr;
			}
		}

		class DatePartBuilderOracleInformixDb2: Sql.IExtensionCallBuilder
		{
			public void Build(Sql.ISqExtensionBuilder builder)
			{
				string partStr;
				var part = builder.GetValue<Sql.DateParts>("part");
				switch (part)
				{
					case Sql.DateParts.Year        : partStr = "To_Number(To_Char({date}, 'YYYY'))";                  break;
					case Sql.DateParts.Quarter     : partStr = "To_Number(To_Char({date}, 'Q'))";                     break;
					case Sql.DateParts.Month       : partStr = "To_Number(To_Char({date}, 'MM'))";                    break;
					case Sql.DateParts.DayOfYear   : partStr = "To_Number(To_Char({date}, 'DDD'))";                   break;
					case Sql.DateParts.Day         : partStr = "To_Number(To_Char({date}, 'DD'))";                    break;
					case Sql.DateParts.Week        : partStr = "To_Number(To_Char({date}, 'WW'))";                    break;
					case Sql.DateParts.WeekDay     : partStr = "Mod(1 + Trunc({date}) - Trunc({date}, 'IW'), 7) + 1"; break;
					case Sql.DateParts.Hour        : partStr = "To_Number(To_Char({date}, 'HH24'))";                  break;
					case Sql.DateParts.Minute      : partStr = "To_Number(To_Char({date}, 'MI'))";                    break;
					case Sql.DateParts.Second      : partStr = "To_Number(To_Char({date}, 'SS'))";                    break;
					case Sql.DateParts.Millisecond : partStr = "To_Number(To_Char({date}, 'FF'))";                    break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				builder.Expression = partStr;
			}
		}

		[Sql.Extension(               "DatePart({part}, {date})",                 ServerSideOnly = false, BuilderType = typeof(DatePartBuilder))]
		[Sql.Extension(PN.DB2,        "",                                         ServerSideOnly = false, BuilderType = typeof(DatePartBuilderOracleInformixDb2))] // TODO: Not checked
		[Sql.Extension(PN.Informix,   "",                                         ServerSideOnly = false, BuilderType = typeof(DatePartBuilderOracleInformixDb2))] // TODO: Not checked
		[Sql.Extension(PN.MySql,      "Extract({part} from {date})",              ServerSideOnly = false, BuilderType = typeof(DatePartBuilderMySql))]
		[Sql.Extension(PN.PostgreSQL, "Extract({part} from {date})",              ServerSideOnly = false, BuilderType = typeof(DatePartBuilderPostgre))]
		[Sql.Extension(PN.Firebird,   "Extract({part} from {date})",              ServerSideOnly = false, BuilderType = typeof(DatePartBuilder))]
		[Sql.Extension(PN.SQLite,     "Cast(StrFTime('%{part}', {date}) as int)", ServerSideOnly = false, BuilderType = typeof(DatePartBuilderSqLite))]
		[Sql.Extension(PN.Access,     "DatePart('{part}', {date})",               ServerSideOnly = false, BuilderType = typeof(DatePartBuilderAccess))]
		[Sql.Extension(PN.SapHana,    "",                                         ServerSideOnly = false, BuilderType = typeof(DatePartBuilderSapHana))]
		[Sql.Extension(PN.Oracle,     "",                                         ServerSideOnly = false, BuilderType = typeof(DatePartBuilderOracleInformixDb2))]
		public static int? DatePart(this Sql.ISqlExtension ext, Sql.DateParts part, [ExprParameter] DateTime? date)
		{
			if (date == null)
				return null;

			switch (part)
			{
				case Sql.DateParts.Year        : return date.Value.Year;
				case Sql.DateParts.Quarter     : return (date.Value.Month - 1) / 3 + 1;
				case Sql.DateParts.Month       : return date.Value.Month;
				case Sql.DateParts.DayOfYear   : return date.Value.DayOfYear;
				case Sql.DateParts.Day         : return date.Value.Day;
				case Sql.DateParts.Week        : return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date.Value, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
				case Sql.DateParts.WeekDay     : return ((int)date.Value.DayOfWeek + 1 + Sql.DateFirst + 6) % 7 + 1;
				case Sql.DateParts.Hour        : return date.Value.Hour;
				case Sql.DateParts.Minute      : return date.Value.Minute;
				case Sql.DateParts.Second      : return date.Value.Second;
				case Sql.DateParts.Millisecond : return date.Value.Millisecond;
			}

			throw new InvalidOperationException();
		}
	}

	public class SqlExtensionTests : TestBase
	{
		#region DatePart

		[Test, DataContextSource]
		public void DatePartYear(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Year, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Year, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartQuarter(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Quarter, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Quarter, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartMonth(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Month, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Month, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartDayOfYear(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.DayOfYear, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.DayOfYear, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartDay(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Day, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Day, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartWeek(string context)
		{
			using (var db = GetDataContext(context))
				(from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Week, t.DateTimeValue))).ToList();
		}

		[Test, DataContextSource]
		public void DatePartWeekDay(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.WeekDay, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.WeekDay, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartHour(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Hour, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Hour, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartMinute(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Minute, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Minute, t.DateTimeValue)));
		}

		[Test, DataContextSource]
		public void DatePartSecond(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Second, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Second, t.DateTimeValue)));
		}

		[Test, DataContextSource(ProviderName.Informix, ProviderName.MySql, ProviderName.Access, ProviderName.SapHana, TestProvName.MariaDB, TestProvName.MySql57)]
		public void DatePartMillisecond(string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from t in    Types select           Sql.Ext.DatePart(Sql.DateParts.Millisecond, t.DateTimeValue),
					from t in db.Types select Sql.AsSql(Sql.Ext.DatePart(Sql.DateParts.Millisecond, t.DateTimeValue)));
		}

		#endregion
	}
}