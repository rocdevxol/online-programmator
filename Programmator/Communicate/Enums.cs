using System.ComponentModel;

namespace Programmator.Communicate
{
	public class Enums
	{
		/// <summary>
		/// Команды для общения с устройствами в сети
		/// </summary>
		public enum Commands : byte
		{
			ZeroCommand = 0x00,

			VersionSoft = 0x01,
			StatusSystem = 0x02,	//

			ReadFlash = 0x03,	// Прочитать Flash/EEPROM/Region
			EraseFlash = 0x04,	// Очистить Flash/EEPROM/Region
			ProgramVerifyFlash = 0x05,	// Запрограммировать Flash/EEPROM/Region

			FinishProgramFlash = 0x06,	// Сброс флага FlashRewrite в процессоре
			RebootDevice = 0x07,	// Перезапуск устройства
		}

		[TypeConverter(typeof(Converters.EnumDescriptionTypeConverter))]
		public enum Machine : byte
		{
			[Description("Неизвество")]
			UndefinedMachine = 0,
			[Description("КНТ-41")]
			KHT_41 = 41,
			[Description("КНТ-42")]
			KHT_42 = 42,
			[Description("КНТ-43")]
			KHT_43 = 43,
			[Description("КНТ-44")]
			KHT_44 = 44,
			[Description("КНТ-45")]
			KHT_45 = 45,
			[Description("КНТ-46")]
			KHT_46 = 46,
			[Description("КТ1М")]
			KT1M = 60,
			[Description("КТ1М-01")]
			KT1M01 = 61,
			[Description("КТО")]
			Oplot = 70,
			[Description("АПК")]
			APK = 80,
			[Description("КРЛ-08")]
			KRL8 = 85,
			[Description("КТНМ-5")]
			KTHM5 = 86,
			[Description("UNKNOWN")]
			UNKNOWN = 255,
		}

		[TypeConverter(typeof(Converters.EnumDescriptionTypeConverter))]
		public enum CpuIdentificator : byte
		{
			[Description("Unknown")]
			UndefinedCpu = 0x00,
			/// <summary>
			/// C2000 Texas Instruments
			/// </summary>
			[Description("C2000")]
			C2000 = 0x33,
			/// <summary>
			/// STM32
			/// </summary>
			[Description("STM32")]
			STM32 = 0x53,
		}

		[TypeConverter(typeof(Converters.EnumDescriptionTypeConverter))]
		public enum Status
		{
			[Description("OK")]
			OK = 0x00,
			[Description("ERROR")]
			ERROR = 0x01,
			[Description("BUSY")]
			BUSY = 0x02,
			[Description("TIMEOUT")]
			TIMEOUT = 0x03
		}

        /// <summary>
        /// Состояние прогресса для прогресс бара
        /// </summary>
		public enum StatusProgress
		{
			Disable,
			Work,
			Alarm,
			Finish,
			Inderterminate
		}

        /// <summary>
        /// Выполняемая функция в программе
        /// </summary>
        public enum FunctionalMode
        {
            Disable,
			VersionSoftware,

            EraseFlash,
            EraseEeprom,
            EraseRegion,

            ProgramFlash,
            ProgramEeprom,
            ProgramRegion,

            ReadFlash,
            ReadEeprom,
            ReadRegion,

			UpdateEeprom,
			RebootDevice
        }


    }
}
