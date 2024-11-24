using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Human human = new Human("noName", 0);

app.Run(async (context) =>
{

	var response = context.Response;
	var request = context.Request;
	response.ContentType = "text/html; charset=utf-8";
	if (request.Path == "/postData")
	{
		var responseText = "Некорректные данные";
		if (request.HasJsonContentType())
		{
			var jsoneOptions = new JsonSerializerOptions();
			jsoneOptions.Converters.Add(new HumanConverter());
			human = await request.ReadFromJsonAsync<Human>(jsoneOptions);
			if (human != null)
				responseText = $"Name: {human.Name} Age: {human.Age}";
			await response.WriteAsJsonAsync(new { text = responseText });

		}
	}
	else if (request.Path == "/api/user")
	{
		await response.WriteAsync($"Имя пользователя: {human.Name}. Возраст пользователя {human.Age} лет.");
	}
	else
	{
		await response.SendFileAsync("html/index.html");
	}
});


app.Run();
public record class Human(string Name, int Age);
public class HumanConverter : JsonConverter<Human>
{
	public override Human? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var humanName = "Undefined";
		var humanAge = 0;
		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				var propertyName = reader.GetString();
				reader.Read();
				switch (propertyName?.ToLower())
				{
					case "age" when reader.TokenType == JsonTokenType.Number:
						humanAge = reader.GetInt32();
						break;
					case "age" when reader.TokenType == JsonTokenType.String:
						string? stringValue = reader.GetString();
						if (int.TryParse(stringValue, out int value))
							humanAge = value;
						break;
					case "name":
						string? name = reader.GetString();
						if (name != null)
							humanName = name;
						break;
				}
			}
		}
		return new Human(humanName, humanAge);
	}
	public override void Write(Utf8JsonWriter writer, Human value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteString("name", value.Name);
		writer.WriteNumber("age", value.Age);

		writer.WriteEndObject();
	}
}