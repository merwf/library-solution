using Library.Data;
using Microsoft.EntityFrameworkCore;
using Library.Business.Interfaces;
using Library.Business.Concrete;
using Library.Data.Repositories.Interfaces;
using Library.Data.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// --- RFC 7807 ProblemDetails Servisi ---
builder.Services.AddProblemDetails();

// --- VERÝTABANI BAÐLANTISI ---
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- REPOSITORY KAYITLARI (Data Layer) ---
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();

// --- BUSINESS SERVICE KAYITLARI (Yeni Eklenenler) ---
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();

// --- CALCULATOR & CONFIG SERVÝSLERÝ ---
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