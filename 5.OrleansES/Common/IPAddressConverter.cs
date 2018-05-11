using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Net;


namespace Common
{
    public class IPAddressConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string v = (string)value;
                if (!IPAddress.TryParse(v, out IPAddress address))
                {
                    if (v.Equals("loopback", StringComparison.OrdinalIgnoreCase))
                    {
                        address = IPAddress.Loopback;
                    }
                    else if (v.Equals("IPv6Loopback", StringComparison.OrdinalIgnoreCase))
                    {
                        address = IPAddress.IPv6Loopback;
                    }
                    else if (v.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        address = IPAddress.Loopback;
                    }
                    else
                    {
                        throw new System.FormatException($"IP地址（{v}）不正确");
                    }
                }
                return address;
            }
            return base.ConvertFrom(context, culture, value);
        }

    }
}