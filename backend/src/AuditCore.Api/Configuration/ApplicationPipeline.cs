namespace AuditCore.Api.Configuration;

public static class ApplicationPipeline
{
    public static WebApplication UseAuditCorePipeline(
        this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var useHttpsRedirection =
            app.Configuration.GetValue("Security:UseHttpsRedirection", true);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else if (!app.Environment.IsEnvironment("Testing") && useHttpsRedirection)
        {
            app.UseHsts();
        }

        if (!app.Environment.IsEnvironment("Testing") && useHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
