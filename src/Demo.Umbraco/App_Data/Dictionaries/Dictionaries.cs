﻿using MomentJs.Net.Globalization;
using Umbraco.DictionaryBuilder.Factories;
using Umbraco.DictionaryBuilder.Models;

namespace Umbraco.Web
{
    public partial class Dictionaries : DictionaryFactory
    {
        public override void EnsureDictionaries()
        {
            DictionaryModel settings = Create("Settings");
            DictionaryModel dateTime = Create("Settings.DateTime", settings);
            DictionaryModel months = Create("Settings.DateTime.Months", dateTime, culture => string.Join("_", GlobalizationProvider.Instance.Months(culture)));
        }
    }
}