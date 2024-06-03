using Ccs_server.data;
using Ccs_server.SORT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.ML.Data;
using Nager.VideoStream;
using System.Text;
using System;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder();
builder.Services.AddDbContext<ApplicationContext>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
    });

var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/login", (LoginData loginData, ApplicationContext db) =>
{ 
    User? user = db.users.ToList().FirstOrDefault(u=>u.Name==loginData.login && u.Password==loginData.Password);

    if (user is null) return Results.Unauthorized();

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Id.ToString()) };
    // создаем JWT-токен
    var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);


    List<Camera> userCameras = db.cameras.ToList().FindAll(c=>c.User == user);

    // формируем ответ
    var response = new
    {
        access_token = encodedJwt,
        username = user.Name,
        userId = user.Id,
        cameras = userCameras,
    };

    return Results.Json(response);
});

app.Map("/camera_data/{cameraId}", [Authorize] (int cameraId, HttpContext context, ApplicationContext db) =>
{
    DateTime fromDate = DateTime.ParseExact(context.Request.Query["from_date"].ToString(),"M/d/yyyy", null);
    DateTime toDate = DateTime.ParseExact(context.Request.Query["to_date"].ToString(),"M/d/yyyy", null);
    List<Value> values = db.values.ToList().FindAll(v=>(v.CameraId == cameraId) && (v.Time <= toDate) && (v.Time>=fromDate));

    var response = new
    {
        cameraValues = values,

    };


    return context.Response.WriteAsJsonAsync(response);
});

app.Map("/cameras/{id}", [Authorize] (int id, HttpContext context, ApplicationContext db) =>
{
    User user = db.users.ToList().First(u => u.Id == id);
    var camerasList = db.cameras.ToList();
    var response = new
    {
        cameras = camerasList,
    };
    return context.Response.WriteAsJsonAsync(response);
});

app.MapPost("/adddamera", (AddCameraData cameraData, ApplicationContext db) =>
{
    string responseMessage = "";
    try
    {
        var newCamera = new Camera
        {
            Name = cameraData.Name,
            Address = cameraData.Address,
            UserId = cameraData.UserId,
            Fbx1 = cameraData.Fbx1,
            Fbx2 = cameraData.Fbx2,
            Fby1 = cameraData.Fby1,
            Fby2 = cameraData.Fby2,
            Sbx1 = cameraData.Sbx1,
            Sbx2 = cameraData.Sbx2,
            Sby1 = cameraData.Sby1,
            Sby2 = cameraData.Sby2,
            User = db.users.ToList().First(u=>u.Id == cameraData.UserId),
        };
        db.cameras.Add(newCamera);
        db.SaveChanges();
        responseMessage = "Новая камера создана";
    }
    catch (Exception ex)
    {
        responseMessage = "Ошибка добавления камеры: "+ex.Message;
    }
    var response = new
    {
        message =responseMessage,
    };

    return Results.Json(response);
}).RequireAuthorization();

app.Map("/deletecam/{id}", [Authorize] (int id, HttpContext context, ApplicationContext db) =>
{
    string responseMessage = "";
    try
    {
        var camera = db.cameras.First(c => c.Id == id);
        db.cameras.Remove(camera);
        db.SaveChanges();
        responseMessage = $"Камера {camera.Name} успешно удалена";
    }
    catch (Exception ex)
    {
        responseMessage = $"Ошибка удаления камеры: ${ex.Message}";
    }
    var response = new
    {
        message = responseMessage,
    };
    return context.Response.WriteAsJsonAsync(response);
});

app.MapPost("/adduser", (RegisterData registerData, ApplicationContext db) =>
{

    User user = new User
    {
        Email = registerData.Email,
        Name = registerData.Login,
        Password = registerData.Password,
    };
    db.users.Add(user);
    db.SaveChanges();
    var response = new
    {
        message = "Пользователь зарегистрирован"
    };
    return Results.Json(response);
});

app.Run();


public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // издатель токена
    public const string AUDIENCE = "MyAuthClient"; // потребитель токена
    const string KEY = "mysupersecret_secretsecretsecretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}


//void NewImageReceived(byte[] imageData)
//{
//    MLImage frame;
//    using (var stream = new MemoryStream(imageData))
//    {
//        frame = MLImage.CreateFromStream(stream);
//    }
//    var detBoxes = detector.Detect(frame);
//    var finalBoxes = sort.Update(detBoxes);

//    if (prevBoxes.Count > 0)
//    {
//        count += CountClients(finalBoxes, prevBoxes);
//    }
//    frameCount++;
//    if (frameCount == 15 * 60)
//    {
//        using (ApplicationContext db = new ApplicationContext())
//        {
//            var value = new Value { Time = DateTime.UtcNow, PersonCount = count, CameraId = 1 };
//            db.values.Add(value);
//            db.SaveChanges();
//        }
//    }
//}

//int CountClients(List<Box> currentBoxes, List<Box> prevBoxes)
//{
//    // здесь будет логика подсчета
//    return 0;
//}

record class LoginData(string login, string Password);
record class RegisterData(string Login, string Password, string Email);
record class AddCameraData(string Name, string Address, int UserId, int Fbx1, int Fbx2, int Fby1, int Fby2, int Sbx1, int Sbx2, int Sby1, int Sby2);