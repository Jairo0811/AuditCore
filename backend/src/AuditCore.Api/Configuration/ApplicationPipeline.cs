namespace AuditCore.Api.Configuration;

public static class ApplicationPipeline
{
    public static WebApplication UseAuditCorePipeline(
        this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHsts();
        }

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
