using Library.Data.Repositories;
using Library.Business;
using Library.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- RFC 7807 ProblemDetails Servisi ---
builder.Services.AddProblemDetails();

// --- VERÝTABANI BAÐLANTISINI BURADA EKLÝYORUZ ---
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// ------------------------------------------------

// Controller verilerine eriþimi soyutlamak için repository DI kaydý (DIP çözümü)
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();
builder.Services.AddScoped<IBorrowService, BorrowService>();

// Business Servislerinin Eklenmesi (Dependency Injection)
builder.Services.AddScoped<IPenaltyFeeCalculator, PenaltyFeeCalculator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();