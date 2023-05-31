﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace mattmc3.dotmore.Extensions {
	public static class EnumExtensions {
		/// <summary>
		/// Gets the description from any System.ComponentModel.DataAnnotations.DisplayAttribute or
		/// System.ComponentModel.DescriptionAttribute decorating the enumeration value.
		/// Null is returned if no description is available.
		/// </summary>
		public static string GetDescription(this Enum value) {
			var result = GetDisplayAttributeValue(value, x => x.DisplayName);
			if (result != null) return result;

			FieldInfo fieldInfo = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] dscrAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (dscrAttributes.Length > 0) {
				return dscrAttributes[0].Description;
			}

			return null;
		}

		/// <summary>
		/// Gets the short name from any System.ComponentModel.DataAnnotations.DisplayAttribute.
		/// Null is returned if no short name is available.
		/// </summary>
		public static string GetShortName(this Enum value) {
			return GetDisplayAttributeValue(value, x => x.DisplayName);
		}

		/// <summary>
		/// Gets the name from any System.ComponentModel.DataAnnotations.DisplayAttribute.
		/// The enum value's name is returned if DisplayAttribute name is available.
		/// </summary>
		public static string GetName(this Enum value) {
			return GetDisplayAttributeValue(value, x => x.DisplayName) ?? value.ToString();
		}

		private static string GetDisplayAttributeValue(Enum value, Func<DisplayNameAttribute, string> fn) {
			FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

			DisplayNameAttribute[] dispAttributes = (DisplayNameAttribute[])fieldInfo.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false);
			return (dispAttributes.Length > 0) ? fn(dispAttributes[0]) : null;
		}
	}
}
