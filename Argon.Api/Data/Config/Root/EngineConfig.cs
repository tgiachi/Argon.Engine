﻿using System;
using System.Globalization;
using Argon.Api.Data.Config.Common;
using Argon.Api.Utils;
using Neon.Api.Utils;
using YamlDotNet.Serialization;

namespace Argon.Api.Data.Config.Root
{
	public class EngineConfig
	{
		[YamlMember(Alias = "home_directory")]
		public string HomeDirectory { get; set; }

		[YamlMember(Alias = "use_swagger")]
		public bool UseSwagger { get; set; }

		[YamlMember(Alias = "logger")]
		public LoggerConfig Logger { get; set; }

		[YamlMember(Alias = "secret_key")]
		public string SecretKey { get; set; }

		[YamlMember(Alias = "time_zone")]
		public string TimeZone { get; set; }

		[YamlMember(Alias = "unit_system")]
		public string UnitSystem { get; set; }

		[YamlMember(Alias = "language")]
		public string Language { get; set; }

		[YamlMember(Alias = "uuid")]
		public string Uuid { get; set; }

		public EngineConfig()
		{
			Uuid = Guid.NewGuid().ToString();
			HomeDirectory = "./argon";
			Logger = new LoggerConfig();
			UseSwagger = true;
			SecretKey = RandomStringUtils.RandomString(32);
			TimeZone = TimeZoneUtils.ToTzdb(TimeZoneInfo.Local);
			UnitSystem = "metric";
			Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
		}
	}
}
