using Project.DataMining;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    var csvPath = Path.GetFullPath(
        Path.Combine(app.Environment.ContentRootPath, "..", "Project.DataMining", "Data", "online_retail.csv"));

    if (!File.Exists(csvPath))
    {
        app.Logger.LogWarning("CSV file not found for data cleaning test: {CsvPath}", csvPath);
    }
    else
    {
        var cleaner = new DataCleaner();
        var baskets = cleaner.ProcessRetailData(csvPath);
        app.Logger.LogInformation("Valid invoice count for training: {Count}", baskets.Count);

        var firstBasket = baskets.FirstOrDefault();
        if (firstBasket is null)
        {
            app.Logger.LogInformation("No valid baskets to display.");
        }
        else
        {
            app.Logger.LogInformation("Invoice: {InvoiceNo}", firstBasket.InvoiceNo);
            app.Logger.LogInformation("Items: {Items}", string.Join(", ", firstBasket.Items));
        }

        if (baskets.Count == 0)
        {
            app.Logger.LogInformation("Skip Apriori: no baskets to process.");
        }
        else
        {
            var apriori = new AprioriEngine();
            // Suggested params: minSupport = 0.01, minConfidence = 0.2
            var rules = apriori.GenerateRules(baskets, minSupport: 0.01, minConfidence: 0.2);

            app.Logger.LogInformation("Found {Count} association rules.", rules.Count);

            var topRules = rules.Take(10).ToList();
            foreach (var rule in topRules)
            {
                app.Logger.LogInformation(
                    "If customer buys [{A}] => suggest [{B}] (Confidence: {Conf:P2}, Lift: {Lift:F2})",
                    rule.ProductA,
                    rule.ProductB,
                    rule.Confidence,
                    rule.Lift);
            }
        }
    }
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
