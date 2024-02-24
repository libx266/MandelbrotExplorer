using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mandelbrot_Explorer
{
    public class ColorConverter : JsonConverter<System.Drawing.Color>
    {
        public override System.Drawing.Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;
                int r = root.GetProperty("R").GetInt32();
                int g = root.GetProperty("G").GetInt32();
                int b = root.GetProperty("B").GetInt32();
                int a = root.GetProperty("A").GetInt32();
                return System.Drawing.Color.FromArgb(a, r, g, b);
            }
        }

        public override void Write(Utf8JsonWriter writer, System.Drawing.Color value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);
            writer.WriteNumber("A", value.A);
            writer.WriteEndObject();
        }
    }
}
