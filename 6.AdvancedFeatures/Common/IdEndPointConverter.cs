using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Common
{
    public class IdEndPointConverter : TypeConverter
    {
        // Overrides the CanConvertFrom method of TypeConverter.
        // The ITypeDescriptorContext interface provides the context for the
        // conversion. Typically, this interface is used at design time to
        // provide information about the design-time container.
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
                string[] v = ((string)value).Split(new char[] { ':' });
                if (!IPAddress.TryParse(v[0], out IPAddress address))
                {
                    if (v[0].Equals("loopback", StringComparison.OrdinalIgnoreCase))
                    {
                        address = IPAddress.Loopback;
                    }
                    else if (v[0].Equals("IPv6Loopback", StringComparison.OrdinalIgnoreCase))
                    {
                        address = IPAddress.IPv6Loopback;
                    }
                    else if (v[0].Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        address = IPAddress.Loopback;
                    }
                    else
                    {
                        throw new System.FormatException($"IP地址（{v[0]}）不正确");
                    }
                }
                return new IPEndPoint(address, int.Parse(v[1]));
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}