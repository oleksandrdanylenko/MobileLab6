﻿using System;
using System.Globalization;
using MvvmCross.Platform.Converters;

namespace MusicStoreMobile.Core.Converters
{
    public class InvertedValueConverter : MvxValueConverter<bool, bool>
    {
        protected override bool Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }
    }
}
